// ==========================================================================
//  GuardAppPattern.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================
using System;
using System.Linq;
using Squidex.Domain.Apps.Core.Apps;
using Squidex.Domain.Apps.Write.Apps.Commands;
using Squidex.Infrastructure;

namespace Squidex.Domain.Apps.Write.Apps.Guards
{
    public static class GuardAppPattern
    {
        public static void CanApply(AppPatterns patterns, AddPattern command)
        {
            Guard.NotNull(command, nameof(command));

            Validate.It(() => "Cannot add pattern.", error =>
            {
                if (command.Id == Guid.Empty)
                {
                    error(new ValidationError("Id can not be empty Guid.", "Id"));
                }

                if (string.IsNullOrWhiteSpace(command.Name))
                {
                    error(new ValidationError("Pattern name can not be empty.", "Name"));
                }

                if (string.IsNullOrWhiteSpace(command.Pattern))
                {
                    error(new ValidationError("Pattern can not be empty.", "Name"));
                }

                if (patterns.Values.Any(x => x.Name.Equals(command.Name.ToLower(), StringComparison.OrdinalIgnoreCase)))
                {
                    error(new ValidationError("Pattern name is already assigned.", "Name"));
                }

                if (patterns.Values.Any(x => x.Pattern == command.Pattern))
                {
                    error(new ValidationError("Pattern already exists.", "Pattern"));
                }
            });
        }

        public static void CanApply(AppPatterns patterns, DeletePattern command)
        {
            Guard.NotNull(command, nameof(command));

            Validate.It(() => "Cannot remove pattern.", error =>
            {
                if (!patterns.ContainsKey(command.Id))
                {
                    error(new ValidationError("Pattern not found.", nameof(command.Id)));
                }
            });
        }

        public static void CanApply(AppPatterns patterns, UpdatePattern command)
        {
            Guard.NotNull(command, nameof(command));

            Validate.It(() => "Cannot update pattern.", error =>
            {
                if (string.IsNullOrWhiteSpace(command.Name))
                {
                    error(new ValidationError("Pattern name can not be empty.", "Name"));
                }

                if (string.IsNullOrWhiteSpace(command.Pattern))
                {
                    error(new ValidationError("Pattern can not be empty.", "Name"));
                }

                if (!patterns.ContainsKey(command.Id))
                {
                    error(new ValidationError("Pattern not found.", nameof(command.Id)));
                }

                if (patterns.Any(x => x.Key != command.Id
                    && x.Value.Name.Equals(command.Name, StringComparison.OrdinalIgnoreCase)))
                {
                    error(new ValidationError("Pattern name is already assigned.", "Name"));
                }

                if (patterns.Any(x => x.Key != command.Id && x.Value.Pattern == command.Pattern))
                {
                    error(new ValidationError("Pattern already exists.", "Pattern"));
                }
            });
        }
    }
}
