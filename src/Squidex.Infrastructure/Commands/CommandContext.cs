// ==========================================================================
//  CommandContext.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System;

namespace Squidex.Infrastructure.CQRS.Commands
{
    public sealed class CommandContext
    {
        private readonly ICommand command;
        private readonly Guid contextId = Guid.NewGuid();
        private Tuple<object> result;

        public ICommand Command
        {
            get { return command; }
        }

        public Guid ContextId
        {
            get { return contextId; }
        }

        public bool IsCompleted
        {
            get { return result != null; }
        }

        public CommandContext(ICommand command)
        {
            Guard.NotNull(command, nameof(command));

            this.command = command;
        }

        public void Complete(object resultValue = null)
        {
            result = Tuple.Create(resultValue);
        }

        public T Result<T>()
        {
            return (T)result?.Item1;
        }
    }
}