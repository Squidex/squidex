// ==========================================================================
//  GuardAppPattern.cs
//  CivicPlus implementation of Squidex Headless CMS
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
                if (string.IsNullOrWhiteSpace(command.Name))
                {
                    error(new ValidationError("Pattern name can not be empty.", "Name"));
                }

                if (string.IsNullOrWhiteSpace(command.Pattern))
                {
                    error(new ValidationError("Pattern can not be empty.", "Name"));
                }

                if (patterns.ContainsKey(command.Name.ToLower()))
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
                if (!patterns.ContainsKey(command.Name.ToLower()))
                {
                    error(new ValidationError("Pattern not found.", nameof(command.Name)));
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

                if (!patterns.ContainsKey(command.OriginalName.ToLower()))
                {
                    error(new ValidationError("Pattern not found.", nameof(command.OriginalName)));
                }

                if (patterns.Where(x => !x.Key.Equals(command.OriginalName, StringComparison.OrdinalIgnoreCase))
                    .ToDictionary(x => x.Key, y => y.Value).ContainsKey(command.Name.ToLower()))
                {
                    error(new ValidationError("Pattern name is already assigned.", "Name"));
                }

                if (patterns.Where(x => !x.Key.Equals(command.OriginalName, StringComparison.OrdinalIgnoreCase))
                    .ToDictionary(x => x.Key, y => y.Value).Values
                    .Any(x => x.Pattern == command.Pattern))
                {
                    error(new ValidationError("Pattern already exists.", "Pattern"));
                }
            });
        }
    }
}
