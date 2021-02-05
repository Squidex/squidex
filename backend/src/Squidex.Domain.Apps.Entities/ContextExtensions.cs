// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Collections.Generic;
using System.Linq;

namespace Squidex.Domain.Apps.Entities
{
    public static class ContextExtensions
    {
        private const string HeaderNoTotal = "X-NoTotal";

        public static bool ShouldSkipTotal(this Context context)
        {
            return context.Headers.ContainsKey(HeaderNoTotal);
        }

        public static ICloneBuilder WithoutTotal(this ICloneBuilder builder, bool value = true)
        {
            return builder.WithBoolean(HeaderNoTotal, value);
        }

        public static ICloneBuilder WithBoolean(this ICloneBuilder builder, string key, bool value)
        {
            if (value)
            {
                builder.SetHeader(key, "1");
            }
            else
            {
                builder.Remove(key);
            }

            return builder;
        }

        public static ICloneBuilder WithStrings(this ICloneBuilder builder, string key, IEnumerable<string>? values)
        {
            if (values?.Any() == true)
            {
                builder.SetHeader(key, string.Join(",", values));
            }
            else
            {
                builder.Remove(key);
            }

            return builder;
        }
    }
}
