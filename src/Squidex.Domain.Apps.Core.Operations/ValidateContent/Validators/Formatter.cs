// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;

namespace Squidex.Domain.Apps.Core.ValidateContent.Validators
{
    public static class Formatter
    {
        private static readonly string IV = InvariantPartitioning.Instance.Master.Key;

        public static AddError Combine(string field, AddError formatter)
        {
            return (f, m) =>
            {
                if (!string.IsNullOrWhiteSpace(f))
                {
                    formatter($"{field}.{f}", m);
                }
                else
                {
                    formatter(field, m);
                }
            };
        }

        public static AddError CombineForLanguage(string field, AddError formatter)
        {
            return (f, m) =>
            {
                if (!string.IsNullOrWhiteSpace(f) && !string.Equals(f, IV, StringComparison.OrdinalIgnoreCase))
                {
                    formatter($"{field}.{f}", m);
                }
                else
                {
                    formatter(field, m);
                }
            };
        }
    }
}
