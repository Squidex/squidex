// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

#define LOG_ALL_IDENTITY_SERVER_NONE

using System;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Squidex.Domain.Apps.Entities.Apps;
using Squidex.Infrastructure.Log;
using Squidex.Log;
using Squidex.Web.Pipeline;

namespace Squidex.Config.Domain
{
    public static class LoggingServices
    {
        public static void ConfigureForSquidex(this ILoggingBuilder builder, IConfiguration config)
        {
            builder.ClearProviders();

            builder.AddConfiguration(config.GetSection("logging"));

            builder.AddSemanticLog();
            builder.AddFilters();

            builder.Services.AddServices(config);
        }

        private static void AddServices(this IServiceCollection services, IConfiguration config)
        {
            services.Configure<RequestLogOptions>(
                config.GetSection("logging"));

            services.Configure<RequestLogStoreOptions>(
                config.GetSection("logging"));

            services.Configure<SemanticLogOptions>(
                config.GetSection("logging"));

            if (config.GetValue<bool>("logging:human"))
            {
                services.AddSingletonAs(_ => JsonLogWriterFactory.Readable())
                    .As<IObjectWriterFactory>();
            }
            else
            {
                services.AddSingletonAs(_ => JsonLogWriterFactory.Default())
                    .As<IObjectWriterFactory>();
            }

            var loggingFile = config.GetValue<string>("logging:file");

            if (!string.IsNullOrWhiteSpace(loggingFile))
            {
                services.AddSingletonAs(_ => new FileChannel(loggingFile))
                    .As<ILogChannel>();
            }

            var useColors = config.GetValue<bool>("logging:colors");

            services.AddSingletonAs(_ => new ConsoleLogChannel(useColors))
                .As<ILogChannel>();

            services.AddSingletonAs(_ => new ApplicationInfoLogAppender(typeof(LoggingServices).Assembly, Guid.NewGuid()))
                .As<ILogAppender>();

            services.AddSingletonAs<ActionContextLogAppender>()
                .As<ILogAppender>();

            services.AddSingletonAs<TimestampLogAppender>()
                .As<ILogAppender>();

            services.AddSingletonAs<DebugLogChannel>()
                .As<ILogChannel>();

            services.AddSingletonAs<SemanticLog>()
                .As<ISemanticLog>();

            services.AddSingletonAs<DefaultAppLogStore>()
                .As<IAppLogStore>();

            services.AddSingletonAs<BackgroundRequestLogStore>()
                .AsOptional<IRequestLogStore>();
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

                if (category.StartsWith("Orleans.Runtime.NoOpHostEnvironmentStatistics", StringComparison.OrdinalIgnoreCase))
                {
                    return level >= LogLevel.Error;
                }

                if (category.StartsWith("Orleans.Runtime.SafeTimer", StringComparison.OrdinalIgnoreCase))
                {
                    return level >= LogLevel.Error;
                }

                if (category.StartsWith("Orleans.Runtime.Scheduler", StringComparison.OrdinalIgnoreCase))
                {
                    return level >= LogLevel.Warning;
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
                    return level >= LogLevel.Warning;
                }

                return true;
            });
        }
    }
}
