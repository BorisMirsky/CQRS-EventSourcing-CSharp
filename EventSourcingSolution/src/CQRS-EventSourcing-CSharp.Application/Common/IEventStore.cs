using System;
using System.Collections.Generic;
using System.Text;
using CQRS_EventSourcing_CSharp.Domain.Events;



namespace CQRS_EventSourcing_CSharp.Application.Common
{
    public interface IEventStore
    {
        Task SaveEventsAsync(Guid aggregateId, IEnumerable<IEvent> events, CancellationToken cancellationToken = default);
        Task<IEnumerable<IEvent>> LoadEventsAsync(Guid aggregateId, CancellationToken cancellationToken = default);
    }
}
