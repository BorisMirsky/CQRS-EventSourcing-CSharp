using CQRS_EventSourcing_CSharp.Application.Abstractions;
using CQRS_EventSourcing_CSharp.Application.Common;
using CQRS_EventSourcing_CSharp.Domain.Aggregates;
using CQRS_EventSourcing_CSharp.Domain.Events;
using Microsoft.Data.Sqlite;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;



namespace CQRS_EventSourcing_CSharp.DataAccess.EventStore
{
    public class SqliteEventStore : IEventStore
    {
        private readonly string _connectionString;
        private readonly JsonSerializerOptions _jsonOptions;
        private readonly ISnapshotRepository _snapshotRepository;
        private const int SnapshotThreshold = 50; // каждые 'n' событий делаем снимок

        public SqliteEventStore(string connectionString, ISnapshotRepository snapshotRepository)
        {
            _connectionString = connectionString;
            _snapshotRepository = snapshotRepository;
            _jsonOptions = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
            DbSchema.EnsureDatabase(_connectionString);
        }


        public async Task SaveEventsAsync(Guid aggregateId, IEnumerable<IEvent> events, CancellationToken cancellationToken = default)
        {
            var eventsList = events.ToList();
            if (!eventsList.Any()) return;

            using var connection = new SqliteConnection(_connectionString);
            await connection.OpenAsync(cancellationToken);

            var currentVersion = await GetCurrentVersionAsync(connection, aggregateId, cancellationToken);
            using var transaction = await connection.BeginTransactionAsync(cancellationToken);

            try
            {
                var version = currentVersion;
                foreach (var @event in eventsList)
                {
                    version++;
                    // сохраняем событие
                }
                await transaction.CommitAsync(cancellationToken);

                // После сохранения проверяем, нужно ли сделать снимок
                var newTotalVersion = currentVersion + eventsList.Count;
                if (ShouldTakeSnapshot(newTotalVersion))
                {
                    // Загружаем полный агрегат для создания снимка
                    var allEvents = await LoadEventsAsync(aggregateId, cancellationToken);
                    var account = new BankAccount();
                    account.LoadFromHistory(allEvents);
                    // Сохраняем снимок (сериализуем агрегат)
                    await _snapshotRepository.SaveSnapshotAsync(aggregateId, account, account.Version, cancellationToken);
                }
            }
            catch (SqliteException ex) when (ex.Message.Contains("UNIQUE constraint failed"))
            {
                await transaction.RollbackAsync(cancellationToken);
                throw new InvalidOperationException($"Concurrency conflict for aggregate {aggregateId}.", ex);
            }
        }


        private bool ShouldTakeSnapshot(int version)
        {
            // Делаем снимок, когда версия кратна SnapshotThreshold, но не на 0
            return version > 0 && version % SnapshotThreshold == 0;
        }


        public async Task<IEnumerable<IEvent>> LoadEventsAsync(Guid aggregateId, CancellationToken cancellationToken = default)
        {
            // 1. Пытаемся загрузить последний снимок
            var snapshot = await _snapshotRepository.GetLatestSnapshotAsync(aggregateId, cancellationToken);
            int startVersion = -1;
            var events = new List<IEvent>();

            if (snapshot != null)
            {
                startVersion = snapshot.Version;
                // Снимок не даёт нам сразу готовый агрегат, мы его используем только чтобы пропустить загрузку старых событий.
                // Но для восстановления состояния агрегата мы всё равно должны применить события после снимка.
                // Поэтому возвращаем события, начиная со следующей версии.
            }

            using var connection = new SqliteConnection(_connectionString);
            await connection.OpenAsync(cancellationToken);

            var command = connection.CreateCommand();
            if (snapshot != null)
            {
                command.CommandText = @"
            SELECT event_type, event_data
            FROM event_store
            WHERE aggregate_id = @aggregate_id AND aggregate_version > @start_version
            ORDER BY aggregate_version ASC
        ";
                command.Parameters.AddWithValue("@start_version", startVersion);
            }
            else
            {
                command.CommandText = @"
            SELECT event_type, event_data
            FROM event_store
            WHERE aggregate_id = @aggregate_id
            ORDER BY aggregate_version ASC
        ";
            }
            command.Parameters.AddWithValue("@aggregate_id", aggregateId.ToString());

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
                nameof(MoneyWithdrawn) => JsonSerializer.Deserialize<MoneyWithdrawn>(eventData, _jsonOptions),
                nameof(AccountFrozen) => JsonSerializer.Deserialize<AccountFrozen>(eventData, _jsonOptions),
                nameof(AccountUnfrozen) => JsonSerializer.Deserialize<AccountUnfrozen>(eventData, _jsonOptions),
                _ => throw new InvalidOperationException($"Unknown event type: {eventType}")
            };
        }


        public async Task<BankAccount> LoadAggregateAsync(Guid aggregateId, CancellationToken cancellationToken)
        {
            // Загружаем снимок
            var snapshot = await _snapshotRepository.GetLatestSnapshotAsync(aggregateId, cancellationToken);
            var account = new BankAccount();

            if (snapshot != null)
            {
                // Десериализуем снимок в BankAccountSnapshot
                var snapshotObj = JsonSerializer.Deserialize<BankAccountSnapshot>(snapshot.SnapshotData, _jsonOptions);
                account.LoadFromSnapshot(snapshotObj!);
            }

            // Загружаем события после версии снимка (или все, если снимка нет)
            using var connection = new SqliteConnection(_connectionString);
            await connection.OpenAsync(cancellationToken);

            var command = connection.CreateCommand();
            if (snapshot != null)
            {
                command.CommandText = @"
            SELECT event_type, event_data
            FROM event_store
            WHERE aggregate_id = @aggregate_id AND aggregate_version > @version
            ORDER BY aggregate_version ASC
        ";
                command.Parameters.AddWithValue("@version", snapshot.Version);
            }
            else
            {
                command.CommandText = @"
            SELECT event_type, event_data
            FROM event_store
            WHERE aggregate_id = @aggregate_id
            ORDER BY aggregate_version ASC
        ";
            }
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

            account.LoadFromHistory(events);
            return account;
        }
    }
}
