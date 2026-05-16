using System;
using System.Collections.Generic;
using System.Text;

namespace CQRS_EventSourcing_CSharp.DataAccess.EventStore
{
    public class EventEntity
    {
        public int Id { get; set; }
        public string AggregateId { get; set; } = string.Empty;
        public int AggregateVersion { get; set; }
        public string EventType { get; set; } = string.Empty;
        public string EventData { get; set; } = string.Empty;
        public string CreatedAt { get; set; } = string.Empty;
    }
}
