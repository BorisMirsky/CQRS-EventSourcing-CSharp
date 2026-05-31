using System;
using System.Collections.Generic;
using System.Text;

namespace CQRS_EventSourcing_CSharp.Application.Queries
{
    public record GetBalanceOnDateQuery
    {
        public Guid AccountId { get; init; }
        public DateTime Date { get; init; }
    }
}
