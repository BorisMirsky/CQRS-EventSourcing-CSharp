using CQRS_EventSourcing_CSharp.Application.Abstractions;
using CQRS_EventSourcing_CSharp.Application.Commands;
using CQRS_EventSourcing_CSharp.Application.Common;
using CQRS_EventSourcing_CSharp.Domain.Aggregates;



namespace CQRS_EventSourcing_CSharp.Application.CommandHandlers
{
    public class FreezeAccountHandler : ICommandHandler<FreezeAccountCommand>
        {
            private readonly IEventStore _eventStore;
            private readonly IReadModelRepository _readModelRepository;

            public FreezeAccountHandler(IEventStore eventStore, IReadModelRepository readModelRepository)
                {
                    _eventStore = eventStore;
                    _readModelRepository = readModelRepository;
                }

            public async Task Handle(FreezeAccountCommand command, CancellationToken cancellationToken)
            {
                var events = await _eventStore.LoadEventsAsync(command.AccountId, cancellationToken);
                var account = new BankAccount();
                account.LoadFromHistory(events);

                account.Freeze(command.Reason);
                await _eventStore.SaveEventsAsync(account.Id, account.GetUncommittedEvents(), cancellationToken);

                await _readModelRepository.UpdateAccountBalance(account.Id, account.Balance, account.IsFrozen, account.Version, cancellationToken);
                account.ClearUncommittedEvents();
            }
    }

}