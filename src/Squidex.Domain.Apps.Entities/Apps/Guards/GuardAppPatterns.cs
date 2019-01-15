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
    public static class GuardAppPatterns
    {
        public static void CanAdd(AppPatterns patterns, AddPattern command)
        {
            Guard.NotNull(command, nameof(command));

            Validate.It(() => "Cannot add pattern.", e =>
            {
                if (command.PatternId == Guid.Empty)
                {
                   e(Not.Defined("Id"), nameof(command.PatternId));
                }

                if (string.IsNullOrWhiteSpace(command.Name))
                {
                   e(Not.Defined("Name"), nameof(command.Name));
                }

                if (patterns.Values.Any(x => x.Name.Equals(command.Name, StringComparison.OrdinalIgnoreCase)))
                {
                    e("A pattern with the same name already exists.");
                }

                if (string.IsNullOrWhiteSpace(command.Pattern))
                {
                   e(Not.Defined("Pattern"), nameof(command.Pattern));
                }
                else if (!command.Pattern.IsValidRegex())
                {
                    e(Not.Valid("Pattern"), nameof(command.Pattern));
                }

                if (patterns.Values.Any(x => x.Pattern == command.Pattern))
                {
                    e("This pattern already exists but with another name.");
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

            Validate.It(() => "Cannot update pattern.", e =>
            {
                if (string.IsNullOrWhiteSpace(command.Name))
                {
                   e(Not.Defined("Name"), nameof(command.Name));
                }

                if (patterns.Any(x => x.Key != command.PatternId && x.Value.Name.Equals(command.Name, StringComparison.OrdinalIgnoreCase)))
                {
                    e("A pattern with the same name already exists.");
                }

                if (string.IsNullOrWhiteSpace(command.Pattern))
                {
                   e(Not.Defined("Pattern"), nameof(command.Pattern));
                }
                else if (!command.Pattern.IsValidRegex())
                {
                    e(Not.Valid("Pattern"), nameof(command.Pattern));
                }

                if (patterns.Any(x => x.Key != command.PatternId && x.Value.Pattern == command.Pattern))
                {
                    e("This pattern already exists but with another name.");
                }
            });
        }
    }
}
