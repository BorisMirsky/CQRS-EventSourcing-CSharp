using System;
using System.Collections.Generic;
using System.Text;

namespace CQRS_EventSourcing_CSharp.Application.DTO
{
    public record SnapshotDTO
    {
        public Guid AggregateId { get; init; }
        public string SnapshotData { get; init; } = string.Empty;
        public int Version { get; init; }
        public DateTime CreatedAt { get; init; }
    }
}
