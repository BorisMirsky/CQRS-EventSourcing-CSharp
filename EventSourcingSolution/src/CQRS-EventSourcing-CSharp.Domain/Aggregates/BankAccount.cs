
using CQRS_EventSourcing_CSharp.Domain.ValueObjects;
using CQRS_EventSourcing_CSharp.Domain.Events;
using System;
using System.Collections.Generic;
using System.Text;


namespace CQRS_EventSourcing_CSharp.Domain.Aggregates
{
    public class BankAccount
    {
        private readonly List<IEvent> _uncommittedEvents = new();
        public Guid Id { get; private set; }
        public Money Balance { get; private set; }
        public string OwnerName { get; private set; }
        public bool IsFrozen { get; private set; }
        public int Version { get; private set; } = -1; // -1 означает, что агрегат новый (нет событий)

        // Для восстановления из событий
        public BankAccount()
        {
        }

        // Создание нового счёта
        public static BankAccount Open(string ownerName, string currency, Guid? accountId = null)
        {
            var account = new BankAccount();
            var @event = new AccountOpened(
                accountId ?? Guid.NewGuid(),
                ownerName,
                currency,
                DateTime.UtcNow
            );
            account.Apply(@event);
            account._uncommittedEvents.Add(@event);
            return account;
        }

        // Пополнение счёта
        public void Deposit(decimal amount, string currency, string description)
        {
            if (IsFrozen)
                throw new InvalidOperationException("Cannot deposit to frozen account");

            if (string.IsNullOrWhiteSpace(description))
                description = "Deposit";

            var @event = new MoneyDeposited(
                Id,
                amount,
                currency,
                description,
                DateTime.UtcNow
            );
            Apply(@event);
            _uncommittedEvents.Add(@event);
        }

        // Применение события к агрегату (восстановление состояния)
        private void Apply(AccountOpened @event)
        {
            Id = @event.AggregateId;
            OwnerName = @event.OwnerName;
            Balance = new Money(0, @event.Currency);
            Version++;
        }

        private void Apply(MoneyDeposited @event)
        {
            Balance = Balance.Add(new Money(@event.Amount, @event.Currency));
            Version++;
        }

        // Получить неприменённые события
        public IReadOnlyList<IEvent> GetUncommittedEvents() => _uncommittedEvents.AsReadOnly();

        // Очистить неприменённые события (после сохранения)
        public void ClearUncommittedEvents() => _uncommittedEvents.Clear();

        // Загрузка истории событий из EventStore
        public void LoadFromHistory(IEnumerable<IEvent> events)
        {
            foreach (var @event in events)
            {
                ApplyDynamic(@event);
            }
        }


        

        private void ApplyDynamic(IEvent @event)
        {
            switch (@event)
            {
            case AccountOpened e:
                Apply(e);
                break;
            case MoneyDeposited e:
                Apply(e);
                break;
            case MoneyWithdrawn e:
                Apply(e);
                break;
            case AccountFrozen e:
                Apply(e);
                break;
            case AccountUnfrozen e:
                Apply(e);
                break;
            default:
                throw new InvalidOperationException($"Unknown event type: {@event.GetType().Name}");
            }
        }

        public void Withdraw(decimal amount, string currency, string description)
        {
            if (IsFrozen)
                throw new InvalidOperationException("Cannot withdraw from frozen account");
            
            if (string.IsNullOrWhiteSpace(description))
                description = "Withdrawal";
            
            // Проверка достаточности средств
            var withdrawAmount = new Money(amount, currency);
            if (Balance.Amount < withdrawAmount.Amount)
                throw new InvalidOperationException($"Insufficient funds. Balance: {Balance.Amount}, requested: {amount}");
            
            var @event = new MoneyWithdrawn(
                Id,
                amount,
                currency,
                description,
                DateTime.UtcNow
            );
            Apply(@event);
            _uncommittedEvents.Add(@event);
        }

        public void Freeze(string reason)
        {
            if (IsFrozen)
                throw new InvalidOperationException("Account is already frozen");
            
            var @event = new AccountFrozen(Id, reason ?? "No reason provided", DateTime.UtcNow);
            Apply(@event);
            _uncommittedEvents.Add(@event);
        }

        public void Unfreeze(string reason)
        {
            if (!IsFrozen)
                throw new InvalidOperationException("Account is not frozen");
            
            var @event = new AccountUnfrozen(Id, reason ?? "No reason provided", DateTime.UtcNow);
            Apply(@event);
            _uncommittedEvents.Add(@event);
        }

        private void Apply(MoneyWithdrawn @event)
        {
            Balance = Balance.Subtract(new Money(@event.Amount, @event.Currency));
            Version++;
        }

        private void Apply(AccountFrozen @event)
        {
            IsFrozen = true;
            Version++;
        }

        private void Apply(AccountUnfrozen @event)
        {
            IsFrozen = false;
            Version++;
        }


    }
}
