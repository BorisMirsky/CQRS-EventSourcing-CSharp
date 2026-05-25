using CQRS_EventSourcing_CSharp.Application.Commands;
using CQRS_EventSourcing_CSharp.Application.Common;
using CQRS_EventSourcing_CSharp.Domain.Aggregates;


namespace CQRS_EventSourcing_CSharp.Application.CommandHandlers
{
    public class WithdrawMoneyHandler : ICommandHandler<WithdrawMoneyCommand>
{
    private readonly IEventStore _eventStore;
    
    public WithdrawMoneyHandler(IEventStore eventStore)
    {
        _eventStore = eventStore;
    }
    
    public async Task Handle(WithdrawMoneyCommand command, CancellationToken cancellationToken = default)
    {
        var events = await _eventStore.LoadEventsAsync(command.AccountId, cancellationToken);
        var account = new BankAccount();
        account.LoadFromHistory(events);
        
        account.Withdraw(command.Amount, command.Currency, command.Description);
        
        await _eventStore.SaveEventsAsync(account.Id, account.GetUncommittedEvents(), cancellationToken);
        account.ClearUncommittedEvents();
    }
}

}

