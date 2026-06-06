using CQRS_EventSourcing_CSharp.Application.Abstractions;
using CQRS_EventSourcing_CSharp.Application.DTO;
using Microsoft.Data.Sqlite;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;




namespace CQRS_EventSourcing_CSharp.DataAccess.Snapshots
{
    public class SqliteSnapshotRepository : ISnapshotRepository
    {
        private readonly string _connectionString;
        private readonly JsonSerializerOptions _jsonOptions;

        public SqliteSnapshotRepository(string connectionString)
        {
            _connectionString = connectionString;
            _jsonOptions = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
        }

        public async Task<SnapshotDTO?> GetLatestSnapshotAsync(Guid aggregateId, CancellationToken cancellationToken = default)
        {
            using var connection = new SqliteConnection(_connectionString);
            await connection.OpenAsync(cancellationToken);

            var command = connection.CreateCommand();
            command.CommandText = "SELECT aggregate_id, snapshot_data, version, created_at FROM aggregate_snapshots WHERE aggregate_id = @aggregate_id";
            command.Parameters.AddWithValue("@aggregate_id", aggregateId.ToString());

            using var reader = await command.ExecuteReaderAsync(cancellationToken);
            if (await reader.ReadAsync(cancellationToken))
            {
                return new SnapshotDTO
                {
                    AggregateId = Guid.Parse(reader.GetString(0)),
                    SnapshotData = reader.GetString(1),
                    Version = reader.GetInt32(2),
                    CreatedAt = DateTime.Parse(reader.GetString(3))
                };
            }
            return null;
        }

        public async Task SaveSnapshotAsync(Guid aggregateId, object snapshot, int version, CancellationToken cancellationToken = default)
        {
            using var connection = new SqliteConnection(_connectionString);
            await connection.OpenAsync(cancellationToken);

            var snapshotJson = JsonSerializer.Serialize(snapshot, snapshot.GetType(), _jsonOptions);

            var command = connection.CreateCommand();
            command.CommandText = @"
            INSERT OR REPLACE INTO aggregate_snapshots (aggregate_id, snapshot_data, version, created_at)
            VALUES (@aggregate_id, @snapshot_data, @version, @created_at)
        ";
            command.Parameters.AddWithValue("@aggregate_id", aggregateId.ToString());
            command.Parameters.AddWithValue("@snapshot_data", snapshotJson);
            command.Parameters.AddWithValue("@version", version);
            command.Parameters.AddWithValue("@created_at", DateTime.UtcNow.ToString("O"));

            await command.ExecuteNonQueryAsync(cancellationToken);
        }
    }
}
