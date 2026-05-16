using System;
using System.Collections.Generic;
using System.Text;

namespace CQRS_EventSourcing_CSharp.Domain.Events
{
    public record AccountOpened : IEvent
    {
        public Guid AggregateId { get; }
        public string OwnerName { get; }
        public string Currency { get; }
        public DateTime OccurredAt { get; }

        public AccountOpened(Guid aggregateId, string ownerName, string currency, DateTime occurredAt)
        {
            AggregateId = aggregateId;
            OwnerName = ownerName;
            Currency = currency;
            OccurredAt = occurredAt;
        }
    }
}
