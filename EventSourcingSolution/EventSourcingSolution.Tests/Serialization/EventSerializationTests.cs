using CQRS_EventSourcing_CSharp.Domain.Events;
using FluentAssertions;
using System.Text.Json;
using Xunit;




namespace EventSourcingSolution.Tests.Serialization;

public class EventSerializationTests
{
    private readonly JsonSerializerOptions _options = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    [Fact]
    public void SerializeDeserialize_AccountOpened_ShouldPreserveData()
    {
        var original = new AccountOpened(
            Guid.NewGuid(),
            "John Doe",
            "USD",
            DateTime.UtcNow
        );

        var json = JsonSerializer.Serialize(original, _options);
        var deserialized = JsonSerializer.Deserialize<AccountOpened>(json, _options);

        deserialized.Should().NotBeNull();
        deserialized!.AggregateId.Should().Be(original.AggregateId);
        deserialized.OwnerName.Should().Be(original.OwnerName);
        deserialized.Currency.Should().Be(original.Currency);
        deserialized.OccurredAt.Should().BeCloseTo(original.OccurredAt, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public void SerializeDeserialize_MoneyDeposited_ShouldPreserveData()
    {
        var original = new MoneyDeposited(
            Guid.NewGuid(),
            150.75m,
            "USD",
            "Salary",
            DateTime.UtcNow
        );

        var json = JsonSerializer.Serialize(original, _options);
        var deserialized = JsonSerializer.Deserialize<MoneyDeposited>(json, _options);

        deserialized.Should().NotBeNull();
        deserialized!.AggregateId.Should().Be(original.AggregateId);
        deserialized.Amount.Should().Be(original.Amount);
        deserialized.Currency.Should().Be(original.Currency);
        deserialized.Description.Should().Be(original.Description);
        deserialized.OccurredAt.Should().BeCloseTo(original.OccurredAt, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public void SerializeDeserialize_MoneyWithdrawn_ShouldPreserveData()
    {
        var original = new MoneyWithdrawn(
            Guid.NewGuid(),
            50.00m,
            "USD",
            "Cash withdrawal",
            DateTime.UtcNow
        );

        var json = JsonSerializer.Serialize(original, _options);
        var deserialized = JsonSerializer.Deserialize<MoneyWithdrawn>(json, _options);

        deserialized.Should().NotBeNull();
        deserialized!.AggregateId.Should().Be(original.AggregateId);
        deserialized.Amount.Should().Be(original.Amount);
        deserialized.Currency.Should().Be(original.Currency);
        deserialized.Description.Should().Be(original.Description);
        deserialized.OccurredAt.Should().BeCloseTo(original.OccurredAt, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public void SerializeDeserialize_AccountFrozen_ShouldPreserveData()
    {
        var original = new AccountFrozen(
            Guid.NewGuid(),
            "Fraud suspicion",
            DateTime.UtcNow
        );

        var json = JsonSerializer.Serialize(original, _options);
        var deserialized = JsonSerializer.Deserialize<AccountFrozen>(json, _options);

        deserialized.Should().NotBeNull();
        deserialized!.AggregateId.Should().Be(original.AggregateId);
        deserialized.Reason.Should().Be(original.Reason);
        deserialized.OccurredAt.Should().BeCloseTo(original.OccurredAt, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public void SerializeDeserialize_AccountUnfrozen_ShouldPreserveData()
    {
        var original = new AccountUnfrozen(
            Guid.NewGuid(),
            "Suspicion cleared",
            DateTime.UtcNow
        );

        var json = JsonSerializer.Serialize(original, _options);
        var deserialized = JsonSerializer.Deserialize<AccountUnfrozen>(json, _options);

        deserialized.Should().NotBeNull();
        deserialized!.AggregateId.Should().Be(original.AggregateId);
        deserialized.Reason.Should().Be(original.Reason);
        deserialized.OccurredAt.Should().BeCloseTo(original.OccurredAt, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public void Deserialize_InvalidEventType_ShouldThrowException()
    {
        // Arrange
        string invalidJson = @"{ ""eventType"": ""NonExistentEvent"", ""aggregateId"": ""something"" }";

        // Act
        Action act = () => JsonSerializer.Deserialize<AccountOpened>(invalidJson, _options);

        // Assert
        act.Should().Throw<JsonException>();
    }
}