using System;
using System.Collections.Generic;
using System.Text;
using CQRS_EventSourcing_CSharp.Application.Commands;
using CQRS_EventSourcing_CSharp.Application.Common;
using CQRS_EventSourcing_CSharp.Domain.Aggregates;



namespace CQRS_EventSourcing_CSharp.Application.CommandHandlers
{
    public class DepositMoneyHandler : ICommandHandler<DepositMoneyCommand>
    {
        private readonly IEventStore _eventStore;

        public DepositMoneyHandler(IEventStore eventStore)
        {
            _eventStore = eventStore;
        }

        public async Task Handle(DepositMoneyCommand command, CancellationToken cancellationToken = default)
        {
            // Загружаем события агрегата из EventStore
            var events = await _eventStore.LoadEventsAsync(command.AccountId, cancellationToken);

            // Восстанавливаем агрегат
            var account = new BankAccount();
            account.LoadFromHistory(events);

            // Выполняем бизнес-логику
            account.Deposit(command.Amount, command.Currency, command.Description);

            // Сохраняем новые события
            await _eventStore.SaveEventsAsync(account.Id, account.GetUncommittedEvents(), cancellationToken);

            account.ClearUncommittedEvents();
        }
    }
}
