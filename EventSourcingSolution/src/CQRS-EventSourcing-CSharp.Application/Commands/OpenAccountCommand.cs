using System;
using System.Collections.Generic;
using System.Text;

namespace CQRS_EventSourcing_CSharp.Application.Commands
{
    public record OpenAccountCommand
    {
        public string OwnerName { get; init; }
        public string Currency { get; init; } = "USD";
    }
}
