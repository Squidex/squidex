// ==========================================================================
//  CommandContext.cs
//  PinkParrot Headless CMS
// ==========================================================================
//  Copyright (c) PinkParrot Group
//  All rights reserved.
// ==========================================================================

using System;

namespace PinkParrot.Infrastructure.CQRS.Commands
{
    public sealed class CommandContext
    {
        private readonly IDomainObjectFactory factory;
        private readonly IDomainObjectRepository repository;
        private readonly ICommand command;
        private Exception exception;
        private bool isSucceeded;

        public ICommand Command
        {
            get { return command; }
        }

        public IDomainObjectFactory Factory
        {
            get { return factory; }
        }

        public IDomainObjectRepository Repository
        {
            get { return repository; }
        }

        public bool IsHandled
        {
            get { return isSucceeded || exception != null; }
        }

        public bool IsSucceeded
        {
            get { return isSucceeded; }
        }

        public Exception Exception
        {
            get { return exception; }
        }

        public CommandContext(IDomainObjectFactory factory, IDomainObjectRepository repository, ICommand command)
        {
            Guard.NotNull(command, nameof(command));
            Guard.NotNull(factory, nameof(factory));
            Guard.NotNull(repository, nameof(repository));

            this.command = command;
            this.factory = factory;
            this.repository = repository;
        }

        public void MarkSucceeded()
        {
            isSucceeded = true;
        }

        public void MarkFailed(Exception handlerException)
        {
            exception = handlerException;
        }
    }
}