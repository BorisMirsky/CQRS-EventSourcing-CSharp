using System;
using System.Collections.Generic;
using System.Text;

namespace CQRS_EventSourcing_CSharp.Application.DTO
{
    public record AccountBalanceReadDTO
    {
        public Guid AccountId { get; init; }
        public decimal BalanceAmount { get; init; }
        public string Currency { get; init; } = string.Empty;
        public bool IsFrozen { get; init; }
        public int Version { get; init; }
        public DateTime LastUpdated { get; init; }
    }
}
