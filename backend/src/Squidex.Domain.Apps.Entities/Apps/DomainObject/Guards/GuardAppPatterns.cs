// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Linq;
using Squidex.Domain.Apps.Entities.Apps.Commands;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Translations;
using Squidex.Infrastructure.Validation;

namespace Squidex.Domain.Apps.Entities.Apps.DomainObject.Guards
{
    public static class GuardAppPatterns
    {
        public static void CanAdd(AddPattern command, IAppEntity app)
        {
            Guard.NotNull(command, nameof(command));

            var patterns = app.Patterns;

            Validate.It(e =>
            {
                if (command.PatternId == DomainId.Empty)
                {
                    e(Not.Defined(nameof(command.PatternId)), nameof(command.PatternId));
                }

                if (string.IsNullOrWhiteSpace(command.Name))
                {
                    e(Not.Defined(nameof(command.Name)), nameof(command.Name));
                }

                if (patterns.Values.Any(x => x.Name.Equals(command.Name, StringComparison.OrdinalIgnoreCase)))
                {
                    e(T.Get("apps.patterns.nameAlreadyExists"));
                }

                if (string.IsNullOrWhiteSpace(command.Pattern))
                {
                    e(Not.Defined(nameof(command.Pattern)), nameof(command.Pattern));
                }
                else if (!command.Pattern.IsValidRegex())
                {
                    e(Not.Valid(nameof(command.Pattern)), nameof(command.Pattern));
                }

                if (patterns.Values.Any(x => x.Pattern == command.Pattern))
                {
                    e(T.Get("apps.patterns.patternAlreadyExists"));
                }
            });
        }

        public static void CanDelete(DeletePattern command, IAppEntity app)
        {
            Guard.NotNull(command, nameof(command));

            var patterns = app.Patterns;

            if (!patterns.ContainsKey(command.PatternId))
            {
                throw new DomainObjectNotFoundException(command.PatternId.ToString());
            }
        }

        public static void CanUpdate(UpdatePattern command, IAppEntity app)
        {
            Guard.NotNull(command, nameof(command));

            var patterns = app.Patterns;

            if (!patterns.ContainsKey(command.PatternId))
            {
                throw new DomainObjectNotFoundException(command.PatternId.ToString());
            }

            Validate.It(e =>
            {
                if (string.IsNullOrWhiteSpace(command.Name))
                {
                    e(Not.Defined(nameof(command.Name)), nameof(command.Name));
                }

                if (patterns.Any(x => x.Key != command.PatternId && x.Value.Name.Equals(command.Name, StringComparison.OrdinalIgnoreCase)))
                {
                    e(T.Get("apps.patterns.nameAlreadyExists"));
                }

                if (string.IsNullOrWhiteSpace(command.Pattern))
                {
                    e(Not.Defined(nameof(command.Pattern)), nameof(command.Pattern));
                }
                else if (!command.Pattern.IsValidRegex())
                {
                    e(Not.Valid(nameof(command.Pattern)), nameof(command.Pattern));
                }

                if (patterns.Any(x => x.Key != command.PatternId && x.Value.Pattern == command.Pattern))
                {
                    e(T.Get("apps.patterns.patternAlreadyExists"));
                }
            });
        }
    }
}
