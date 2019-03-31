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
        public static void AddFilter(this ILoggingBuilder builder)
        {
            builder.AddFilter((category, level) =>
            {
                if (category.StartsWith("Orleans.Runtime.NoOpHostEnvironmentStatistics", StringComparison.OrdinalIgnoreCase))
                {
                    return level >= LogLevel.Error;
                }

                if (category.StartsWith("Orleans.Runtime.Scheduler", StringComparison.OrdinalIgnoreCase))
                {
                    return level >= LogLevel.Error;
                }

                if (category.StartsWith("Orleans.", StringComparison.OrdinalIgnoreCase))
                {
                    return level >= LogLevel.Warning;
                }

                if (category.StartsWith("Runtime.", StringComparison.OrdinalIgnoreCase))
                {
                    return level >= LogLevel.Warning;
                }

                if (category.StartsWith("Microsoft.AspNetCore.", StringComparison.OrdinalIgnoreCase))
                {
                    return level > LogLevel.Information;
                }
#if LOG_ALL_IDENTITY_SERVER
                if (category.StartsWith("IdentityServer4.", StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
#endif
                return level >= LogLevel.Information;
            });
        }
    }
}
