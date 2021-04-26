// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

namespace Squidex.Infrastructure.Commands
{
    public sealed class CommandContext
    {
        public DomainId ContextId { get; } = DomainId.NewGuid();

        public ICommand Command { get; }

        public ICommandBus CommandBus { get; }

        public object? PlainResult { get; private set; }

        public bool IsCompleted
        {
            get => PlainResult != null;
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
            PlainResult = resultValue ?? None.Value;

            return this;
        }

        public T Result<T>()
        {
            return (T)PlainResult!;
        }
    }
}