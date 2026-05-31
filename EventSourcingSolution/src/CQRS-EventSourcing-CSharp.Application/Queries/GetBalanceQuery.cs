using System;
using System.Collections.Generic;
using System.Text;

namespace CQRS_EventSourcing_CSharp.Application.Queries
{
    public record GetBalanceQuery
    {
        public Guid AccountId { get; init; }
    }
}
