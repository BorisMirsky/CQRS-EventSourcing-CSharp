using CQRS_EventSourcing_CSharp.Application.Abstractions;
using CQRS_EventSourcing_CSharp.Application.Commands;
using CQRS_EventSourcing_CSharp.Application.Common;
using CQRS_EventSourcing_CSharp.Domain.Aggregates;



namespace CQRS_EventSourcing_CSharp.Application.CommandHandlers
{
    public class OpenAccountHandler : ICommandHandler<OpenAccountCommand>
    {
        private readonly IEventStore _eventStore;
        private readonly IReadModelRepository _readModelRepository;

        public OpenAccountHandler(IEventStore eventStore, IReadModelRepository readModelRepository)
        {
            _eventStore = eventStore;
            _readModelRepository = readModelRepository;
        }

        public async Task Handle(OpenAccountCommand command, CancellationToken cancellationToken)
        {
            var account = BankAccount.Open(command.OwnerName, command.Currency);
            await _eventStore.SaveEventsAsync(account.Id, account.GetUncommittedEvents(), cancellationToken);
            // Инициализация read-модели
            await _readModelRepository.UpdateAccountBalance(account.Id, account.Balance, account.IsFrozen, account.Version, cancellationToken);
            // Нет истории транзакций при открытии (или добавить событие открытия как транзакцию)
            account.ClearUncommittedEvents();
        }
    }
}
