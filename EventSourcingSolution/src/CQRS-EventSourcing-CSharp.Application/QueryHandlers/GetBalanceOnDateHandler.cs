using CQRS_EventSourcing_CSharp.Application.Abstractions;
using CQRS_EventSourcing_CSharp.Application.Common;
using CQRS_EventSourcing_CSharp.Application.Queries;
using CQRS_EventSourcing_CSharp.Domain.Aggregates;
using CQRS_EventSourcing_CSharp.Domain.ValueObjects;
using System;
using System.Collections.Generic;
using System.Text;



namespace CQRS_EventSourcing_CSharp.Application.QueryHandlers
{
    public class GetBalanceOnDateHandler
    {
        private readonly IEventStore _eventStore;

        public GetBalanceOnDateHandler(IEventStore eventStore)
        {
            _eventStore = eventStore;
        }

        public async Task<Money?> Handle(GetBalanceOnDateQuery query, CancellationToken cancellationToken)
        {
            var events = await _eventStore.LoadEventsAsync(query.AccountId, cancellationToken);
            // Фильтруем события до указанной даты включительно
            var eventsUntilDate = events.Where(e => e.OccurredAt <= query.Date).ToList();
            if (!eventsUntilDate.Any())
                return null;

            var account = new BankAccount();
            account.LoadFromHistory(eventsUntilDate);
            return account.Balance;
        }
    }
}
