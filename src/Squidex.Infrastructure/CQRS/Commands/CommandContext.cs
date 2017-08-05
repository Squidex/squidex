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
        private Exception exception;
        private Tuple<object> result;

        public ICommand Command
        {
            get { return command; }
        }

        public bool IsHandled
        {
            get { return IsSucceeded || IsFailed; }
        }

        public bool IsFailed
        {
            get { return exception != null; }
        }

        public bool IsSucceeded
        {
            get { return result != null; }
        }

        public Exception Exception
        {
            get { return exception; }
        }

        public Guid ContextId
        {
            get { return contextId; }
        }

        public CommandContext(ICommand command)
        {
            Guard.NotNull(command, nameof(command));

            this.command = command;
        }

        public void Succeed(object resultValue = null)
        {
            if (IsHandled)
            {
                return;
            }

            result = Tuple.Create(resultValue);
        }

        public void Fail(Exception handlerException)
        {
            if (IsFailed)
            {
                return;
            }

            exception = handlerException;
        }

        public T Result<T>()
        {
            return (T)result?.Item1;
        }
    }
}