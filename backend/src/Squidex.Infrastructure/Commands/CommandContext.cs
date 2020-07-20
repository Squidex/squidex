// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;

namespace Squidex.Infrastructure.Commands
{
    public sealed class CommandContext
    {
        private Tuple<object?>? result;

        public DomainId ContextId { get; } = DomainId.NewGuid();

        public ICommand Command { get; }

        public ICommandBus CommandBus { get; }

        public object? PlainResult
        {
            get { return result?.Item1; }
        }

        public bool IsCompleted
        {
            get { return result != null; }
        }

        public CommandContext(ICommand command, ICommandBus commandBus)
        {
            Guard.NotNull(command, nameof(command));
            Guard.NotNull(commandBus, nameof(commandBus));

            Command = command;
            CommandBus = commandBus;
        }

        public CommandContext Complete(object? resultValue = null)
        {
            result = Tuple.Create(resultValue);

            return this;
        }

        public T Result<T>()
        {
            return (T)result?.Item1!;
        }
    }
}