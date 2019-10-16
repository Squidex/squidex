// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

#define LOG_ALL_IDENTITY_SERVER_NONE

using System;
using Microsoft.Extensions.Logging;

namespace Squidex.Config
{
    public static class Logging
    {
        public static void AddFilters(this ILoggingBuilder builder)
        {
            builder.AddFilter((category, level) =>
            {
                if (level < LogLevel.Information)
                {
                    return false;
                }

                if (category.StartsWith("Orleans.", StringComparison.OrdinalIgnoreCase))
                {
                    var subCategory = category.AsSpan().Slice(8);

                    if (subCategory.StartsWith("Runtime."))
                    {
                        var subCategory2 = subCategory.Slice(8);

                        if (subCategory.StartsWith("NoOpHostEnvironmentStatistics", StringComparison.OrdinalIgnoreCase))
                        {
                            return level >= LogLevel.Error;
                        }

                        if (subCategory.StartsWith("SafeTimer", StringComparison.OrdinalIgnoreCase))
                        {
                            return level >= LogLevel.Error;
                        }
                    }

                    return level >= LogLevel.Warning;
                }

                if (category.StartsWith("Runtime.", StringComparison.OrdinalIgnoreCase))
                {
                    return level >= LogLevel.Warning;
                }

                if (category.StartsWith("Microsoft.AspNetCore.", StringComparison.OrdinalIgnoreCase))
                {
                    return level >= LogLevel.Warning;
                }
#if LOG_ALL_IDENTITY_SERVER
                if (category.StartsWith("IdentityServer4.", StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
#endif
                return true;
            });
        }
    }
}
