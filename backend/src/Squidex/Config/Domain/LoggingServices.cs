// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

#define LOG_ALL_IDENTITY_SERVER_NONE

using Squidex.Infrastructure.Log;
using Squidex.Log;
using Squidex.Web.Pipeline;

namespace Squidex.Config.Domain;

public static class LoggingServices
{
    public static void ConfigureForSquidex(this ILoggingBuilder builder, IConfiguration config)
    {
        builder.ClearProviders();

        // Also adds semantic logging.
        builder.ConfigureSemanticLog(config);

        builder.AddConfiguration(config.GetSection("logging"));
        builder.AddFilters();

        builder.Services.AddServices(config);
    }

    private static void AddServices(this IServiceCollection services, IConfiguration config)
    {
        services.Configure<RequestLogOptions>(config,
            "logging");

        services.Configure<RequestLogStoreOptions>(config,
            "logging");

        services.AddSingletonAs(_ => new ApplicationInfoLogAppender(typeof(LoggingServices).Assembly, Guid.NewGuid()))
            .As<ILogAppender>();

        services.AddSingletonAs<ActionContextLogAppender>()
            .As<ILogAppender>();
    }

    private static void AddFilters(this ILoggingBuilder builder)
    {
        builder.AddFilter((category, level) =>
        {
#if LOG_ALL_IDENTITY_SERVER
            if (category.StartsWith("Microsoft.AspNetCore.", StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }

            if (category.StartsWith("IdentityServer4.", StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
#endif
            if (level < LogLevel.Information)
            {
                return false;
            }

            if (category == null)
            {
                return true;
            }

            if (category.StartsWith("OpenIddict", StringComparison.OrdinalIgnoreCase))
            {
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

            return true;
        });
    }
}
