using System;
using System.Collections.Generic;
using System.Text;

namespace CQRS_EventSourcing_CSharp.Application.DTO
{
    public record TransactionHistoryReadDTO
    {
        public Guid TransactionId { get; init; }
        public Guid AccountId { get; init; }
        public string Type { get; init; } = string.Empty;
        public decimal Amount { get; init; }
        public decimal BalanceAfter { get; init; }
        public string Description { get; init; } = string.Empty;
        public DateTime CreatedAt { get; init; }
    }
}
