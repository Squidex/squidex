// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.CLI.Commands.Implementation;
using Squidex.Infrastructure;

namespace Squidex.Domain.Apps.Entities.Apps.Templates
{
    internal sealed class CLILogger : ILogger, ILogLine
    {
        public static readonly CLILogger Instance = new CLILogger();

        private CLILogger()
        {
        }

        public void StepFailed(string reason)
        {
            throw new DomainException($"Template failed with {reason}");
        }

        public void StepSkipped(string reason)
        {
        }

        public void StepStart(string process)
        {
        }

        public void StepSuccess(string? details = null)
        {
        }

        public void WriteLine()
        {
        }

        public void WriteLine(string message)
        {
        }

        public void WriteLine(string message, params object?[] args)
        {
        }

        public void Dispose()
        {
        }

        public ILogLine WriteSameLine()
        {
            return this;
        }
    }
}
