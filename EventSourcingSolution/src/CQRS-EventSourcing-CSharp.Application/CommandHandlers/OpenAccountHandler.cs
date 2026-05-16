using System;
using System.Collections.Generic;
using System.Text;
using CQRS_EventSourcing_CSharp.Application.Commands;
using CQRS_EventSourcing_CSharp.Application.Common;
using CQRS_EventSourcing_CSharp.Domain.Aggregates;



namespace CQRS_EventSourcing_CSharp.Application.CommandHandlers
{
    public class OpenAccountHandler : ICommandHandler<OpenAccountCommand>
    {
        private readonly IEventStore _eventStore;

        public OpenAccountHandler(IEventStore eventStore)
        {
            _eventStore = eventStore;
        }

        public async Task Handle(OpenAccountCommand command, CancellationToken cancellationToken = default)
        {
            // Создаём агрегат
            var account = BankAccount.Open(command.OwnerName, command.Currency);

            // Сохраняем события
            await _eventStore.SaveEventsAsync(account.Id, account.GetUncommittedEvents(), cancellationToken);

            account.ClearUncommittedEvents();
        }
    }
}
