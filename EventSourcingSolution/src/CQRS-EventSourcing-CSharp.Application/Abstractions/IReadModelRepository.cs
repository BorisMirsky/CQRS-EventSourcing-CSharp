using CQRS_EventSourcing_CSharp.Domain.ValueObjects;
using CQRS_EventSourcing_CSharp.Application.DTO;
using System;
using System.Collections.Generic;
using System.Text;



namespace CQRS_EventSourcing_CSharp.Application.Abstractions
{
    public interface IReadModelRepository
    {
        Task UpdateAccountBalance(Guid accountId, Money balance, bool isFrozen, int version, CancellationToken cancellationToken = default);
        Task AddTransactionHistory(Guid transactionId, Guid accountId, string type, Money amount, Money balanceAfter, string description, DateTime createdAt, CancellationToken cancellationToken = default);
        Task<AccountBalanceReadDTO?> GetAccountBalance(Guid accountId, CancellationToken cancellationToken = default);
        Task<IEnumerable<TransactionHistoryReadDTO>> GetTransactionHistory(Guid accountId, CancellationToken cancellationToken = default);
    }
}
