using System;

namespace PinkParrot.Infrastructure.CQRS.Commands
{
    public interface ICommandContext
    {
        ICommand Command { get; }
        Exception Exception { get; }
        IDomainObjectFactory Factory { get; }
        bool IsFailed { get; }
        bool IsHandled { get; }
        bool IsSucceeded { get; }
        IDomainObjectRepository Repository { get; }
    }
}