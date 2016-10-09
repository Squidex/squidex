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
        private readonly ICommand command;
        private Exception exception;
        private bool isSucceeded;
        
        public ICommand Command
        {
            get { return command; }
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

        public CommandContext(ICommand command)
        {
            Guard.NotNull(command, nameof(command));

            this.command = command;
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