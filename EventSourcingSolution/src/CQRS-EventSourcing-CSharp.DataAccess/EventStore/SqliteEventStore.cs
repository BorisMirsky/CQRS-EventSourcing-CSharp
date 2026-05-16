using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using CQRS_EventSourcing_CSharp.Application.Common;
using CQRS_EventSourcing_CSharp.Domain.Events;
using Microsoft.Data.Sqlite;



namespace CQRS_EventSourcing_CSharp.DataAccess.EventStore
{
    public class SqliteEventStore : IEventStore
    {
        private readonly string _connectionString;
        private readonly JsonSerializerOptions _jsonOptions;

        public SqliteEventStore(string connectionString)
        {
            _connectionString = connectionString;
            _jsonOptions = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };

            // Создаём таблицу при инициализации
            DbSchema.EnsureDatabase(_connectionString);
        }

        public async Task SaveEventsAsync(Guid aggregateId, IEnumerable<IEvent> events, CancellationToken cancellationToken = default)
        {
            var eventsList = events.ToList();
            if (!eventsList.Any())
                return;

            using var connection = new SqliteConnection(_connectionString);
            await connection.OpenAsync(cancellationToken);

            // Получаем текущую версию агрегата
            var currentVersion = await GetCurrentVersionAsync(connection, aggregateId, cancellationToken);

            using var transaction = await connection.BeginTransactionAsync(cancellationToken);

            try
            {
                var version = currentVersion;

                foreach (var @event in eventsList)
                {
                    version++;

                    var eventType = @event.GetType().Name;
                    var eventData = JsonSerializer.Serialize(@event, @event.GetType(), _jsonOptions);

                    var command = connection.CreateCommand();
                    command.CommandText = @"
                    INSERT INTO event_store (aggregate_id, aggregate_version, event_type, event_data, created_at)
                    VALUES (@aggregate_id, @aggregate_version, @event_type, @event_data, @created_at)
                ";
                    command.Parameters.AddWithValue("@aggregate_id", aggregateId.ToString());
                    command.Parameters.AddWithValue("@aggregate_version", version);
                    command.Parameters.AddWithValue("@event_type", eventType);
                    command.Parameters.AddWithValue("@event_data", eventData);
                    command.Parameters.AddWithValue("@created_at", DateTime.UtcNow.ToString("O"));

                    await command.ExecuteNonQueryAsync(cancellationToken);
                }

                await transaction.CommitAsync(cancellationToken);
            }
            catch (SqliteException ex) when (ex.Message.Contains("UNIQUE constraint failed"))
            {
                await transaction.RollbackAsync(cancellationToken);
                throw new InvalidOperationException($"Concurrency conflict for aggregate {aggregateId}. Another transaction has been saved.", ex);
            }
        }

        public async Task<IEnumerable<IEvent>> LoadEventsAsync(Guid aggregateId, CancellationToken cancellationToken = default)
        {
            using var connection = new SqliteConnection(_connectionString);
            await connection.OpenAsync(cancellationToken);

            var command = connection.CreateCommand();
            command.CommandText = @"
            SELECT event_type, event_data
            FROM event_store
            WHERE aggregate_id = @aggregate_id
            ORDER BY aggregate_version ASC
        ";
            command.Parameters.AddWithValue("@aggregate_id", aggregateId.ToString());

            var events = new List<IEvent>();

            using var reader = await command.ExecuteReaderAsync(cancellationToken);
            while (await reader.ReadAsync(cancellationToken))
            {
                var eventType = reader.GetString(0);
                var eventData = reader.GetString(1);

                var eventInstance = DeserializeEvent(eventType, eventData);
                if (eventInstance != null)
                    events.Add(eventInstance);
            }

            return events;
        }

        private async Task<int> GetCurrentVersionAsync(SqliteConnection connection, Guid aggregateId, CancellationToken cancellationToken)
        {
            var command = connection.CreateCommand();
            command.CommandText = @"
            SELECT MAX(aggregate_version)
            FROM event_store
            WHERE aggregate_id = @aggregate_id
        ";
            command.Parameters.AddWithValue("@aggregate_id", aggregateId.ToString());

            var result = await command.ExecuteScalarAsync(cancellationToken);
            return result == DBNull.Value ? -1 : Convert.ToInt32(result);
        }

        private IEvent? DeserializeEvent(string eventType, string eventData)
        {
            return eventType switch
            {
                nameof(AccountOpened) => JsonSerializer.Deserialize<AccountOpened>(eventData, _jsonOptions),
                nameof(MoneyDeposited) => JsonSerializer.Deserialize<MoneyDeposited>(eventData, _jsonOptions),
                _ => throw new InvalidOperationException($"Unknown event type: {eventType}")
            };
        }
    }
}
