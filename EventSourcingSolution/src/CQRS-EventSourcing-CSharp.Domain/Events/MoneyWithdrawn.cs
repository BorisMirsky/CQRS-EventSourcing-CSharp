
namespace CQRS_EventSourcing_CSharp.Domain.Events
{
    public record MoneyWithdrawn : IEvent
    {
        public Guid AggregateId { get; }
        public decimal Amount { get; }
        public string Currency { get; }
        public string Description { get; }
        public DateTime OccurredAt { get; }
        
        public MoneyWithdrawn(Guid aggregateId, decimal amount, string currency, string description, DateTime occurredAt)
        {
            AggregateId = aggregateId;
            Amount = amount;
            Currency = currency;
            Description = description;
            OccurredAt = occurredAt;
        }
    }
}