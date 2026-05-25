using System;
using System.Collections.Generic;
using System.Text;

namespace CQRS_EventSourcing_CSharp.Application.CommandHandlers
{
    public interface ICommandHandler<TCommand>
    {
        Task Handle(TCommand command, CancellationToken cancellationToken = default);
    }
}
