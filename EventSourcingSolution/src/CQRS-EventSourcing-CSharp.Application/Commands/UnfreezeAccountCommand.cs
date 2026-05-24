

namespace CQRS_EventSourcing_CSharp.Application.Commands
{
    public record UnfreezeAccountCommand
        {
            public Guid AccountId { get; init; }
            public string Reason { get; init; } = string.Empty;
        }
}