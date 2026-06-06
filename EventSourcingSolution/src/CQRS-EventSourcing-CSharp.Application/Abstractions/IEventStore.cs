using CQRS_EventSourcing_CSharp.Domain.Aggregates;
using CQRS_EventSourcing_CSharp.Domain.Events;


namespace CQRS_EventSourcing_CSharp.Application.Common
{
    public interface IEventStore
    {

        Task<IEnumerable<IEvent>> LoadEventsAsync(Guid aggregateId, CancellationToken cancellationToken = default);
        Task SaveEventsAsync(Guid aggregateId, IEnumerable<IEvent> events, CancellationToken cancellationToken = default);
        Task<BankAccount> LoadAggregateAsync(Guid aggregateId, CancellationToken cancellationToken = default);

    }
}
