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

namespace Squidex.Domain.Apps.Entities.Apps.Guards
{
    public static class GuardAppPatterns
    {
        public static void CanConfigure(ConfigurePatterns command)
        {
            Guard.NotNull(command, nameof(command));

            Validate.It(() => "Cannot configure patterns.", e =>
            {
                if (command.Patterns?.Length > 0)
                {
                    var patternIndex = 0;
                    var patternPrefix = string.Empty;

                    foreach (var pattern in command.Patterns)
                    {
                        patternIndex++;
                        patternPrefix = $"{nameof(command.Patterns)}[{patternIndex}]";

                        ValidatePattern(pattern, patternPrefix, e);
                    }

                    var validNames = command.Patterns.Select(p => p?.Name).Where(p => !string.IsNullOrWhiteSpace(p));

                    if (validNames.Count() != validNames.Distinct(StringComparer.OrdinalIgnoreCase).Count())
                    {
                        e("Two patterns with the same name exist.", nameof(command.Patterns));
                    }

                    var validPatterns = command.Patterns.Select(p => p?.Pattern).Where(p => !string.IsNullOrWhiteSpace(p));

                    if (validPatterns.Count() != validPatterns.Distinct().Count())
                    {
                        e("Two patterns with the same expression exist.", nameof(command.Patterns));
                    }
                }
            });
        }

        private static void ValidatePattern(UpsertAppPattern pattern, string prefix, AddValidation e)
        {
            if (pattern == null)
            {
                e(Not.Defined("Pattern"), prefix);
            }
            else
            {
                if (string.IsNullOrWhiteSpace(pattern.Name))
                {
                    e(Not.Defined("Name"), $"{prefix}.{nameof(pattern.Name)}");
                }

                if (string.IsNullOrWhiteSpace(pattern.Pattern))
                {
                    e(Not.Defined("Expression"), $"{prefix}.{nameof(pattern.Pattern)}");
                }
                else if (!pattern.Pattern.IsValidRegex())
                {
                    e(Not.Valid("Expression"), $"{prefix}.{nameof(pattern.Pattern)}");
                }
            }
        }
    }
}
