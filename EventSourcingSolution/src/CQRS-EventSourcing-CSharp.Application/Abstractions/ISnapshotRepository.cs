using System;
using System.Collections.Generic;
using System.Text;
using CQRS_EventSourcing_CSharp.Application.DTO;



namespace CQRS_EventSourcing_CSharp.Application.Abstractions
{
    public interface ISnapshotRepository
    {
        Task<SnapshotDTO?> GetLatestSnapshotAsync(Guid aggregateId, CancellationToken cancellationToken = default);
        Task SaveSnapshotAsync(Guid aggregateId, object snapshot, int version, CancellationToken cancellationToken = default);
    }
}
