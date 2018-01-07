// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================
using System;
using System.Linq;
using Squidex.Domain.Apps.Core.Apps;
using Squidex.Domain.Apps.Entities.Apps.Commands;
using Squidex.Infrastructure;

namespace Squidex.Domain.Apps.Entities.Apps.Guards
{
    public static class GuardAppPattern
    {
        public static void CanAdd(AppPatterns patterns, AddPattern command)
        {
            Guard.NotNull(command, nameof(command));

            Validate.It(() => "Cannot add pattern.", error =>
            {
                if (string.IsNullOrWhiteSpace(command.Name))
                {
                    error(new ValidationError("Pattern name can not be empty.", nameof(command.Name)));
                }

                if (patterns.Values.Any(x => x.Name.Equals(command.Name, StringComparison.OrdinalIgnoreCase)))
                {
                    error(new ValidationError("Pattern name is already assigned.", nameof(command.Name)));
                }

                if (string.IsNullOrWhiteSpace(command.Pattern))
                {
                    error(new ValidationError("Pattern can not be empty.", nameof(command.Pattern)));
                }
                else if (!command.Pattern.IsValidRegex())
                {
                    error(new ValidationError("Pattern is not a valid regular expression.", nameof(command.Pattern)));
                }

                if (patterns.Values.Any(x => x.Pattern == command.Pattern))
                {
                    error(new ValidationError("Pattern already exists.", nameof(command.Pattern)));
                }
            });
        }

        public static void CanDelete(AppPatterns patterns, DeletePattern command)
        {
            Guard.NotNull(command, nameof(command));

            if (!patterns.ContainsKey(command.PatternId))
            {
                throw new DomainObjectNotFoundException(command.PatternId.ToString(), typeof(AppPattern));
            }
        }

        public static void CanUpdate(AppPatterns patterns, UpdatePattern command)
        {
            Guard.NotNull(command, nameof(command));

            if (!patterns.ContainsKey(command.PatternId))
            {
                throw new DomainObjectNotFoundException(command.PatternId.ToString(), typeof(AppPattern));
            }

            Validate.It(() => "Cannot update pattern.", error =>
            {
                if (string.IsNullOrWhiteSpace(command.Name))
                {
                    error(new ValidationError("Pattern name can not be empty.", nameof(command.Name)));
                }

                if (patterns.Any(x => x.Key != command.PatternId && x.Value.Name.Equals(command.Name, StringComparison.OrdinalIgnoreCase)))
                {
                    error(new ValidationError("Pattern name is already assigned.", nameof(command.Name)));
                }

                if (string.IsNullOrWhiteSpace(command.Pattern))
                {
                    error(new ValidationError("Pattern can not be empty.", nameof(command.Pattern)));
                }
                else if (!command.Pattern.IsValidRegex())
                {
                    error(new ValidationError("Pattern is not a valid regular expression.", nameof(command.Pattern)));
                }

                if (patterns.Any(x => x.Key != command.PatternId && x.Value.Pattern == command.Pattern))
                {
                    error(new ValidationError("Pattern already exists.", nameof(command.Pattern)));
                }
            });
        }
    }
}
