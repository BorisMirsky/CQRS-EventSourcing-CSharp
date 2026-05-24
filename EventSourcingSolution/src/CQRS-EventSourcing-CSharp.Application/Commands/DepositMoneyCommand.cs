using System;
using System.Collections.Generic;
using System.Text;

namespace CQRS_EventSourcing_CSharp.Application.Commands
{
    public record DepositMoneyCommand
    {
        public Guid AccountId { get; init; }
        public decimal Amount { get; init; }
        public string Currency { get; init; } = "USD";
        public string Description { get; init; } = "Deposit";
    }
}
