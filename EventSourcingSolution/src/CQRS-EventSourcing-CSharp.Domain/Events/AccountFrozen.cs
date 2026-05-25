namespace CQRS_EventSourcing_CSharp.Domain.Events
{
        public record AccountFrozen : IEvent
        {
            public Guid AggregateId { get; }
            public string Reason { get; }
            public DateTime OccurredAt { get; }
            
            public AccountFrozen(Guid aggregateId, string reason, DateTime occurredAt)
            {
                AggregateId = aggregateId;
                Reason = reason;
                OccurredAt = occurredAt;
            }
        }
}