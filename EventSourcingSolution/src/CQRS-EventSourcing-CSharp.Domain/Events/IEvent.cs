using System;
using System.Collections.Generic;
using System.Text;

namespace CQRS_EventSourcing_CSharp.Domain.Events
{
    public interface IEvent
    {
        Guid AggregateId { get; }
        DateTime OccurredAt { get; }
    }
}
