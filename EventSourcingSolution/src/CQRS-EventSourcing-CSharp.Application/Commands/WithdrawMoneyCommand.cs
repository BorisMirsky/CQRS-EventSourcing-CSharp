

namespace CQRS_EventSourcing_CSharp.Application.Commands
{
    public record WithdrawMoneyCommand
        {
            public Guid AccountId { get; init; }
            public decimal Amount { get; init; }
            public string Currency { get; init; } = "USD";
            public string Description { get; init; } = "Withdrawal";
        }
}