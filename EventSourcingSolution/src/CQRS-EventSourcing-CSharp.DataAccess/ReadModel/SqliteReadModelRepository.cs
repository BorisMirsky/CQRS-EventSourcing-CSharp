using CQRS_EventSourcing_CSharp.Application.Abstractions;
using CQRS_EventSourcing_CSharp.Application.DTO;
using CQRS_EventSourcing_CSharp.Domain.ValueObjects;
using Microsoft.Data.Sqlite;
using System;
using System.Collections.Generic;
using System.Data;
using System.Text;
using System.Text.Json;



namespace CQRS_EventSourcing_CSharp.DataAccess.ReadModel
{
    public class SqliteReadModelRepository : IReadModelRepository
    {
        private readonly string _connectionString;

        public SqliteReadModelRepository(string connectionString)
        {
            _connectionString = connectionString;
        }

        public async Task UpdateAccountBalance(Guid accountId, Money balance, bool isFrozen, int version, CancellationToken cancellationToken)
        {
            using var connection = new SqliteConnection(_connectionString);
            await connection.OpenAsync(cancellationToken);

            var command = connection.CreateCommand();
            command.CommandText = @"
            INSERT OR REPLACE INTO account_balances (account_id, balance_amount, currency, is_frozen, last_updated, version)
            VALUES (@account_id, @balance_amount, @currency, @is_frozen, @last_updated, @version)
        ";
            command.Parameters.AddWithValue("@account_id", accountId.ToString());
            command.Parameters.AddWithValue("@balance_amount", balance.Amount);
            command.Parameters.AddWithValue("@currency", balance.Currency);
            command.Parameters.AddWithValue("@is_frozen", isFrozen ? 1 : 0);
            command.Parameters.AddWithValue("@last_updated", DateTime.UtcNow.ToString("O"));
            command.Parameters.AddWithValue("@version", version);

            await command.ExecuteNonQueryAsync(cancellationToken);
        }

        public async Task AddTransactionHistory(Guid transactionId, Guid accountId, string type, Money amount, Money balanceAfter, string description, DateTime createdAt, CancellationToken cancellationToken)
        {
            using var connection = new SqliteConnection(_connectionString);
            await connection.OpenAsync(cancellationToken);

            var command = connection.CreateCommand();
            command.CommandText = @"
            INSERT INTO transaction_history (transaction_id, account_id, type, amount, balance_after, description, created_at)
            VALUES (@transaction_id, @account_id, @type, @amount, @balance_after, @description, @created_at)
        ";
            command.Parameters.AddWithValue("@transaction_id", transactionId.ToString());
            command.Parameters.AddWithValue("@account_id", accountId.ToString());
            command.Parameters.AddWithValue("@type", type);
            command.Parameters.AddWithValue("@amount", amount.Amount);
            command.Parameters.AddWithValue("@balance_after", balanceAfter.Amount);
            command.Parameters.AddWithValue("@description", description ?? "");
            command.Parameters.AddWithValue("@created_at", createdAt.ToString("O"));

            await command.ExecuteNonQueryAsync(cancellationToken);
        }

        public async Task<AccountBalanceReadDTO?> GetAccountBalance(Guid accountId, CancellationToken cancellationToken)
        {
            using var connection = new SqliteConnection(_connectionString);
            await connection.OpenAsync(cancellationToken);

            var command = connection.CreateCommand();
            command.CommandText = "SELECT account_id, balance_amount, currency, is_frozen, version, last_updated FROM account_balances WHERE account_id = @account_id";
            command.Parameters.AddWithValue("@account_id", accountId.ToString());

            using var reader = await command.ExecuteReaderAsync(cancellationToken);
            if (await reader.ReadAsync(cancellationToken))
            {
                return new AccountBalanceReadDTO
                {
                    AccountId = Guid.Parse(reader.GetString(0)),
                    BalanceAmount = reader.GetDecimal(1),
                    Currency = reader.GetString(2),
                    IsFrozen = reader.GetInt32(3) == 1,
                    Version = reader.GetInt32(4),
                    LastUpdated = DateTime.Parse(reader.GetString(5))
                };
            }
            return null;
        }

        public async Task<IEnumerable<TransactionHistoryReadDTO>> GetTransactionHistory(Guid accountId, CancellationToken cancellationToken)
        {
            var result = new List<TransactionHistoryReadDTO>();
            using var connection = new SqliteConnection(_connectionString);
            await connection.OpenAsync(cancellationToken);

            var command = connection.CreateCommand();
            command.CommandText = "SELECT transaction_id, account_id, type, amount, balance_after, description, created_at FROM transaction_history WHERE account_id = @account_id ORDER BY created_at DESC";
            command.Parameters.AddWithValue("@account_id", accountId.ToString());

            using var reader = await command.ExecuteReaderAsync(cancellationToken);
            while (await reader.ReadAsync(cancellationToken))
            {
                result.Add(new TransactionHistoryReadDTO
                {
                    TransactionId = Guid.Parse(reader.GetString(0)),
                    AccountId = Guid.Parse(reader.GetString(1)),
                    Type = reader.GetString(2),
                    Amount = reader.GetDecimal(3),
                    BalanceAfter = reader.GetDecimal(4),
                    Description = reader.GetString(5),
                    CreatedAt = DateTime.Parse(reader.GetString(6))
                });
            }
            return result;
        }
    }
}
