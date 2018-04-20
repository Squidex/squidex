// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using Microsoft.Extensions.Logging;

namespace Squidex.Config
{
    public static class Logging
    {
        public static void AddOrleansFilter(this ILoggingBuilder builder)
        {
            builder.AddFilter((category, level) =>
            {
                if (category.StartsWith("Orleans.", StringComparison.OrdinalIgnoreCase))
                {
                    return level >= LogLevel.Warning;
                }

                if (category.StartsWith("Runtime.", StringComparison.OrdinalIgnoreCase))
                {
                    return level >= LogLevel.Warning;
                }

                return level >= LogLevel.Information;
            });
        }
    }
}
