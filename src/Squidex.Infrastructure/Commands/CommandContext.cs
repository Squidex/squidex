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
        private readonly ICommand command;
        private readonly ICommandBus commandBus;
        private readonly Guid contextId = Guid.NewGuid();
        private Tuple<object> result;

        public ICommand Command
        {
            get { return command; }
        }

        public ICommandBus CommandBus
        {
            get { return commandBus; }
        }

        public Guid ContextId
        {
            get { return contextId; }
        }

        public bool IsCompleted
        {
            get { return result != null; }
        }

        public CommandContext(ICommand command, ICommandBus commandBus)
        {
            Guard.NotNull(command, nameof(command));
            Guard.NotNull(commandBus, nameof(commandBus));

            this.command = command;
            this.commandBus = commandBus;
        }

        public CommandContext Complete(object resultValue = null)
        {
            result = Tuple.Create(resultValue);

            return this;
        }

        public T Result<T>()
        {
            return (T)result?.Item1;
        }
    }
}