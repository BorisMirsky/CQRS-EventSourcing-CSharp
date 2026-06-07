using CQRS_EventSourcing_CSharp.Application.Abstractions;
using CQRS_EventSourcing_CSharp.Application.Commands;
using CQRS_EventSourcing_CSharp.Application.Common;
using CQRS_EventSourcing_CSharp.Domain.ValueObjects;



namespace CQRS_EventSourcing_CSharp.Application.CommandHandlers
{
    public class DepositMoneyHandler : ICommandHandler<DepositMoneyCommand>
    {
        private readonly IEventStore _eventStore;
        private readonly IReadModelRepository _readModelRepository;

        public DepositMoneyHandler(IEventStore eventStore, IReadModelRepository readModelRepository)
        {
            _eventStore = eventStore;
            _readModelRepository = readModelRepository;
        }

        public async Task Handle(DepositMoneyCommand command, CancellationToken cancellationToken = default)
        {
            var account = await _eventStore.LoadAggregateAsync(command.AccountId, cancellationToken);
            account.Deposit(command.Amount, command.Currency, command.Description);
            await _eventStore.SaveEventsAsync(account.Id, account.GetUncommittedEvents(), cancellationToken);
            // Обновление read-модели
            await _readModelRepository.UpdateAccountBalance(account.Id, account.Balance, account.IsFrozen, account.Version, cancellationToken);
            // Добавляем запись в историю (генерируем transactionId)
            var transactionId = Guid.NewGuid();
            await _readModelRepository.AddTransactionHistory(transactionId, account.Id, "Deposit", new Money(command.Amount, command.Currency), account.Balance, command.Description, DateTime.UtcNow, cancellationToken);
            account.ClearUncommittedEvents();
        }
    }
}
