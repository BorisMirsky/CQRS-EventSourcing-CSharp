using CQRS_EventSourcing_CSharp.Domain.Aggregates;
using CQRS_EventSourcing_CSharp.Domain.Events;
using CQRS_EventSourcing_CSharp.Domain.ValueObjects;
using FluentAssertions;
using Xunit;



namespace EventSourcingSolution.Tests.Domain;

public class BankAccountTests
{
    [Fact]
    public void Open_ShouldCreateAccountWithZeroBalanceAndNotFrozen()
    {
        // Arrange
        var ownerName = "John Doe";
        var currency = "USD";

        // Act
        var account = BankAccount.Open(ownerName, currency);

        // Assert
        account.Id.Should().NotBeEmpty();
        account.OwnerName.Should().Be(ownerName);
        account.Balance.Should().Be(Money.Zero(currency));
        account.IsFrozen.Should().BeFalse();
        account.Version.Should().Be(0); // первое событие даст версию 0 после Apply, но лучше проверить GetUncommittedEvents
        var events = account.GetUncommittedEvents();
        events.Should().HaveCount(1);
        events[0].Should().BeOfType<AccountOpened>();
        var openedEvent = events[0] as AccountOpened;
        openedEvent!.OwnerName.Should().Be(ownerName);
        openedEvent!.Currency.Should().Be(currency);
    }

    [Fact]
    public void Deposit_ShouldIncreaseBalanceAndProduceEvent()
    {
        // Arrange
        var account = BankAccount.Open("Test", "USD");
        account.ClearUncommittedEvents(); // очищаем событие открытия

        // Act
        account.Deposit(100, "USD", "Test deposit");

        // Assert
        account.Balance.Amount.Should().Be(100);
        var events = account.GetUncommittedEvents();
        events.Should().HaveCount(1);
        events[0].Should().BeOfType<MoneyDeposited>();
        var depositEvent = events[0] as MoneyDeposited;
        depositEvent!.Amount.Should().Be(100);
        depositEvent!.Currency.Should().Be("USD");
        depositEvent!.Description.Should().Be("Test deposit");
    }

    [Fact]
    public void Withdraw_WhenSufficientFunds_ShouldDecreaseBalanceAndProduceEvent()
    {
        // Arrange
        var account = BankAccount.Open("Test", "USD");
        account.Deposit(200, "USD", "Initial");
        account.ClearUncommittedEvents();

        // Act
        account.Withdraw(100, "USD", "Withdraw for test");

        // Assert
        account.Balance.Amount.Should().Be(100);
        var events = account.GetUncommittedEvents();
        events.Should().HaveCount(1);
        events[0].Should().BeOfType<MoneyWithdrawn>();
        var withdrawEvent = events[0] as MoneyWithdrawn;
        withdrawEvent!.Amount.Should().Be(100);
        withdrawEvent!.Description.Should().Be("Withdraw for test");
    }

    [Fact]
    public void Withdraw_WhenInsufficientFunds_ShouldThrowExceptionAndNotProduceEvents()
    {
        // Arrange
        var account = BankAccount.Open("Test", "USD");
        account.Deposit(50, "USD", "Initial");
        account.ClearUncommittedEvents();

        // Act
        Action act = () => account.Withdraw(100, "USD", "Too much");

        // Assert
        act.Should().Throw<InvalidOperationException>()
           .WithMessage("*Insufficient funds*");
        account.GetUncommittedEvents().Should().BeEmpty();
        account.Balance.Amount.Should().Be(50); // баланс не изменился
    }

    [Fact]
    public void Freeze_ShouldMarkAccountAsFrozen()
    {
        // Arrange
        var account = BankAccount.Open("Test", "USD");
        account.ClearUncommittedEvents();

        // Act
        account.Freeze("Test freeze");

        // Assert
        account.IsFrozen.Should().BeTrue();
        var events = account.GetUncommittedEvents();
        events.Should().HaveCount(1);
        events[0].Should().BeOfType<AccountFrozen>();
        var freezeEvent = events[0] as AccountFrozen;
        freezeEvent!.Reason.Should().Be("Test freeze");
    }

    [Fact]
    public void Freeze_WhenAlreadyFrozen_ShouldThrowException()
    {
        // Arrange
        var account = BankAccount.Open("Test", "USD");
        account.Freeze("First freeze");
        account.ClearUncommittedEvents();

        // Act
        Action act = () => account.Freeze("Second freeze");

        // Assert
        act.Should().Throw<InvalidOperationException>()
           .WithMessage("*already frozen*");
        account.GetUncommittedEvents().Should().BeEmpty();
    }

    [Fact]
    public void Unfreeze_ShouldMarkAccountAsNotFrozen()
    {
        // Arrange
        var account = BankAccount.Open("Test", "USD");
        account.Freeze("Freeze");
        account.ClearUncommittedEvents();

        // Act
        account.Unfreeze("Unfreeze");

        // Assert
        account.IsFrozen.Should().BeFalse();
        var events = account.GetUncommittedEvents();
        events.Should().HaveCount(1);
        events[0].Should().BeOfType<AccountUnfrozen>();
        var unfreezeEvent = events[0] as AccountUnfrozen;
        unfreezeEvent!.Reason.Should().Be("Unfreeze");
    }

    [Fact]
    public void Unfreeze_WhenNotFrozen_ShouldThrowException()
    {
        // Arrange
        var account = BankAccount.Open("Test", "USD");
        account.ClearUncommittedEvents();

        // Act
        Action act = () => account.Unfreeze("Unfreeze when not frozen");

        // Assert
        act.Should().Throw<InvalidOperationException>()
           .WithMessage("*not frozen*");
        account.GetUncommittedEvents().Should().BeEmpty();
    }

    [Fact]
    public void Deposit_WhenFrozen_ShouldThrowException()
    {
        // Arrange
        var account = BankAccount.Open("Test", "USD");
        account.Freeze("Freeze");
        account.ClearUncommittedEvents();

        // Act
        Action act = () => account.Deposit(100, "USD", "Deposit frozen");

        // Assert
        act.Should().Throw<InvalidOperationException>()
           .WithMessage("*frozen*");
        account.GetUncommittedEvents().Should().BeEmpty();
    }

    [Fact]
    public void Withdraw_WhenFrozen_ShouldThrowException()
    {
        // Arrange
        var account = BankAccount.Open("Test", "USD");
        account.Deposit(200, "USD", "Initial");
        account.Freeze("Freeze");
        account.ClearUncommittedEvents();

        // Act
        Action act = () => account.Withdraw(50, "USD", "Withdraw frozen");

        // Assert
        act.Should().Throw<InvalidOperationException>()
           .WithMessage("*frozen*");
        account.GetUncommittedEvents().Should().BeEmpty();
    }

    [Fact]
    public void LoadFromHistory_ShouldReconstructStateCorrectly()
    {
        // Arrange
        var accountId = Guid.NewGuid();
        var events = new IEvent[]
        {
            new AccountOpened(accountId, "Test", "USD", DateTime.UtcNow),
            new MoneyDeposited(accountId, 100, "USD", "Deposit 1", DateTime.UtcNow),
            new MoneyDeposited(accountId, 50, "USD", "Deposit 2", DateTime.UtcNow),
            new MoneyWithdrawn(accountId, 30, "USD", "Withdraw 1", DateTime.UtcNow),
            new AccountFrozen(accountId, "Freeze", DateTime.UtcNow),
            new AccountUnfrozen(accountId, "Unfreeze", DateTime.UtcNow)
        };

        var account = new BankAccount();

        // Act
        account.LoadFromHistory(events);

        // Assert
        account.Id.Should().Be(accountId);
        account.OwnerName.Should().Be("Test");
        account.Balance.Amount.Should().Be(120); // 100+50-30
        account.Balance.Currency.Should().Be("USD");
        account.IsFrozen.Should().BeFalse();
        account.Version.Should().Be(events.Length - 1); // последний Apply увеличил версию до 5 (т.к. начали с -1)
    }
}
