using CQRS_EventSourcing_CSharp.Application.Abstractions;
using CQRS_EventSourcing_CSharp.Application.Commands;
using CQRS_EventSourcing_CSharp.Application.Common;
using CQRS_EventSourcing_CSharp.Domain.Aggregates;
using CQRS_EventSourcing_CSharp.Domain.ValueObjects;


namespace CQRS_EventSourcing_CSharp.Application.CommandHandlers
{
    public class WithdrawMoneyHandler : ICommandHandler<WithdrawMoneyCommand>
    {
        private readonly IEventStore _eventStore;
        private readonly IReadModelRepository _readModelRepository;

        public WithdrawMoneyHandler(IEventStore eventStore, IReadModelRepository readModelRepository)
        {
            _eventStore = eventStore;
            _readModelRepository = readModelRepository;

        }

        public async Task Handle(WithdrawMoneyCommand command, CancellationToken cancellationToken)
        {
            var account = await _eventStore.LoadAggregateAsync(command.AccountId, cancellationToken);
            account.Withdraw(command.Amount, command.Currency, command.Description);
            await _eventStore.SaveEventsAsync(account.Id, account.GetUncommittedEvents(), cancellationToken);
            await _readModelRepository.UpdateAccountBalance(account.Id, account.Balance, account.IsFrozen, account.Version, cancellationToken);
            var transactionId = Guid.NewGuid();
            await _readModelRepository.AddTransactionHistory(transactionId, account.Id, "Withdrawal", new Money(command.Amount, command.Currency), account.Balance, command.Description, DateTime.UtcNow, cancellationToken);
            account.ClearUncommittedEvents();
        }
    }

}

