using CQRS_EventSourcing_CSharp.Application.Commands;
using CQRS_EventSourcing_CSharp.Application.Common;
using CQRS_EventSourcing_CSharp.Domain.Aggregates;



namespace CQRS_EventSourcing_CSharp.Application.CommandHandlers
{
    public class UnfreezeAccountHandler : ICommandHandler<UnfreezeAccountCommand>
    {
        private readonly IEventStore _eventStore;
        
        public UnfreezeAccountHandler(IEventStore eventStore)
        {
            _eventStore = eventStore;
        }
        
        public async Task Handle(UnfreezeAccountCommand command, CancellationToken cancellationToken = default)
        {
            var events = await _eventStore.LoadEventsAsync(command.AccountId, cancellationToken);
            var account = new BankAccount();
            account.LoadFromHistory(events);
            
            account.Unfreeze(command.Reason);
            
            await _eventStore.SaveEventsAsync(account.Id, account.GetUncommittedEvents(), cancellationToken);
            account.ClearUncommittedEvents();
        }
}
}