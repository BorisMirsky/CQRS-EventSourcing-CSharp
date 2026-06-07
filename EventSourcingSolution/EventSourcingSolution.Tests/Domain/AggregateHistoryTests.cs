using CQRS_EventSourcing_CSharp.Domain.Aggregates;
using CQRS_EventSourcing_CSharp.Domain.Events;
using FluentAssertions;




namespace EventSourcingSolution.Tests.Domain;

public class AggregateHistoryTests
{
    [Fact]
    public void LoadFromHistory_WithNoEvents_ShouldKeepDefaultState()
    {
        // Arrange
        var account = new BankAccount();
        var emptyEvents = new List<IEvent>();

        // Act
        account.LoadFromHistory(emptyEvents);

        // Assert
        account.Id.Should().BeEmpty();
        account.OwnerName.Should().BeNull();
        account.Balance.Amount.Should().Be(0);
        account.IsFrozen.Should().BeFalse();
        account.Version.Should().Be(-1); // начальная версия до событий
    }

    [Fact]
    public void LoadFromHistory_WithOnlyAccountOpened_ShouldSetIdAndZeroBalance()
    {
        // Arrange
        var accountId = Guid.NewGuid();
        var events = new IEvent[]
        {
            new AccountOpened(accountId, "Alice", "EUR", DateTime.UtcNow)
        };
        var account = new BankAccount();

        // Act
        account.LoadFromHistory(events);

        // Assert
        account.Id.Should().Be(accountId);
        account.OwnerName.Should().Be("Alice");
        account.Balance.Amount.Should().Be(0);
        account.Balance.Currency.Should().Be("EUR");
        account.IsFrozen.Should().BeFalse();
        account.Version.Should().Be(0);
    }

    [Fact]
    public void LoadFromHistory_WithDepositsAndWithdrawals_ShouldCalculateCorrectBalance()
    {
        // Arrange
        var accountId = Guid.NewGuid();
        var events = new IEvent[]
        {
            new AccountOpened(accountId, "Bob", "USD", DateTime.UtcNow),
            new MoneyDeposited(accountId, 100, "USD", "First", DateTime.UtcNow),
            new MoneyDeposited(accountId, 50, "USD", "Second", DateTime.UtcNow),
            new MoneyWithdrawn(accountId, 30, "USD", "Spending", DateTime.UtcNow),
            new MoneyDeposited(accountId, 20, "USD", "Bonus", DateTime.UtcNow)
        };
        var account = new BankAccount();

        // Act
        account.LoadFromHistory(events);

        // Assert
        account.Balance.Amount.Should().Be(140); // 100+50-30+20 = 140
        account.Version.Should().Be(4); // 5 событий, версия начинается с -1, после 5 событий = 4
    }

    [Fact]
    public void LoadFromHistory_WithFreezeAndUnfreeze_ShouldRestoreFrozenState()
    {
        // Arrange
        var accountId = Guid.NewGuid();
        var events = new IEvent[]
        {
            new AccountOpened(accountId, "Charlie", "USD", DateTime.UtcNow),
            new AccountFrozen(accountId, "Fraud", DateTime.UtcNow),
            new AccountUnfrozen(accountId, "Resolved", DateTime.UtcNow),
            new AccountFrozen(accountId, "Second freeze", DateTime.UtcNow)
        };
        var account = new BankAccount();

        // Act
        account.LoadFromHistory(events);

        // Assert
        account.IsFrozen.Should().BeTrue();
        account.Version.Should().Be(3);
    }

    [Fact]
    public void LoadFromHistory_ShouldApplyEventsInOrder()
    {
        // Arrange: два депозита и один вывод, но порядок важен
        var accountId = Guid.NewGuid();
        var events = new IEvent[]
        {
            new AccountOpened(accountId, "Dave", "USD", DateTime.UtcNow),
            new MoneyDeposited(accountId, 100, "USD", "Deposit A", DateTime.UtcNow),
            new MoneyWithdrawn(accountId, 40, "USD", "Withdrawal B", DateTime.UtcNow),
            new MoneyDeposited(accountId, 200, "USD", "Deposit C", DateTime.UtcNow)
        };
        var account = new BankAccount();

        // Act
        account.LoadFromHistory(events);

        // Assert
        // После открытия 0, +100 = 100, -40 = 60, +200 = 260
        account.Balance.Amount.Should().Be(260);
        account.Version.Should().Be(3);
    }
}