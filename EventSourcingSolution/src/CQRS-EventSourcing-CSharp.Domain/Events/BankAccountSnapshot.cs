using System;
using System.Collections.Generic;
using System.Text;

namespace CQRS_EventSourcing_CSharp.Domain.Events
{
    public record BankAccountSnapshot
    {
        public Guid Id { get; init; }
        public decimal BalanceAmount { get; init; }
        public string Currency { get; init; } = "USD";
        public string OwnerName { get; init; } = string.Empty;
        public bool IsFrozen { get; init; }
        public int Version { get; init; }
    }
}
