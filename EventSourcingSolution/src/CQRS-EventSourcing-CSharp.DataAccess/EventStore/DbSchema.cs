using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Data.Sqlite;



namespace CQRS_EventSourcing_CSharp.DataAccess.EventStore
{
    public static class DbSchema
    {
        public static void EnsureDatabase(string connectionString)
        {
            using var connection = new SqliteConnection(connectionString);
            connection.Open();

            var command = connection.CreateCommand();
            command.CommandText = @"
            CREATE TABLE IF NOT EXISTS event_store (
                id INTEGER PRIMARY KEY AUTOINCREMENT,
                aggregate_id TEXT NOT NULL,
                aggregate_version INTEGER NOT NULL,
                event_type TEXT NOT NULL,
                event_data TEXT NOT NULL,
                created_at TEXT NOT NULL,
                UNIQUE(aggregate_id, aggregate_version)
            );
            
            CREATE INDEX IF NOT EXISTS idx_event_store_aggregate 
            ON event_store(aggregate_id, aggregate_version);
        ";

            command.ExecuteNonQuery();
        }
    }
}
