using System;
using System.Collections.Generic;
using System.Text;

namespace CQRS_EventSourcing_CSharp.Domain.Events
{
    public record MoneyDeposited : IEvent
    {
        public Guid AggregateId { get; }
        public decimal Amount { get; }
        public string Currency { get; }
        public string Description { get; }
        public DateTime OccurredAt { get; }

        public MoneyDeposited(Guid aggregateId, decimal amount, string currency, string description, DateTime occurredAt)
        {
            AggregateId = aggregateId;
            Amount = amount;
            Currency = currency;
            Description = description;
            OccurredAt = occurredAt;
        }
    }
}
