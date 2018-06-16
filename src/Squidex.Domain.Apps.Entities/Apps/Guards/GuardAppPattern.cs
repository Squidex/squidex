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
                if (command.PatternId == Guid.Empty)
                {
                    error(new ValidationError("Id is required.", nameof(command.PatternId)));
                }

                if (string.IsNullOrWhiteSpace(command.Name))
                {
                    error(new ValidationError("Name is required.", nameof(command.Name)));
                }

                if (patterns.Values.Any(x => x.Name.Equals(command.Name, StringComparison.OrdinalIgnoreCase)))
                {
                    error(new ValidationError("An pattern with the same name already exists."));
                }

                if (string.IsNullOrWhiteSpace(command.Pattern))
                {
                    error(new ValidationError("Pattern is required.", nameof(command.Pattern)));
                }
                else if (!command.Pattern.IsValidRegex())
                {
                    error(new ValidationError("Pattern is not a valid regular expression.", nameof(command.Pattern)));
                }

                if (patterns.Values.Any(x => x.Pattern == command.Pattern))
                {
                    error(new ValidationError("This pattern already exists but with another name."));
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
                    error(new ValidationError("Name is required.", nameof(command.Name)));
                }

                if (patterns.Any(x => x.Key != command.PatternId && x.Value.Name.Equals(command.Name, StringComparison.OrdinalIgnoreCase)))
                {
                    error(new ValidationError("An pattern with the same name already exists."));
                }

                if (string.IsNullOrWhiteSpace(command.Pattern))
                {
                    error(new ValidationError("Pattern is required.", nameof(command.Pattern)));
                }
                else if (!command.Pattern.IsValidRegex())
                {
                    error(new ValidationError("Pattern is not a valid regular expression.", nameof(command.Pattern)));
                }

                if (patterns.Any(x => x.Key != command.PatternId && x.Value.Pattern == command.Pattern))
                {
                    error(new ValidationError("This pattern already exists but with another name."));
                }
            });
        }
    }
}
