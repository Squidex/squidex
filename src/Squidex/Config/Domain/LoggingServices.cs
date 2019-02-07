// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Squidex.Domain.Apps.Entities.Apps;
using Squidex.Infrastructure.Log;
using Squidex.Pipeline;

namespace Squidex.Config.Domain
{
    public static class LoggingServices
    {
        public static void AddMyLoggingServices(this IServiceCollection services, IConfiguration config)
        {
            if (config.GetValue<bool>("logging:human"))
            {
                services.AddSingletonAs(JsonLogWriterFactory.Readable())
                    .As<IObjectWriterFactory>();
            }
            else
            {
                services.AddSingletonAs(JsonLogWriterFactory.Default())
                    .As<IObjectWriterFactory>();
            }

            var loggingFile = config.GetValue<string>("logging:file");

            if (!string.IsNullOrWhiteSpace(loggingFile))
            {
                services.AddSingletonAs(new FileChannel(loggingFile))
                    .As<ILogChannel>();
            }

            var useColors = config.GetValue<bool>("logging:colors");

            services.AddSingletonAs(new ConsoleLogChannel(useColors))
                .As<ILogChannel>();

            services.AddSingletonAs(c => new ApplicationInfoLogAppender(typeof(Program).Assembly, Guid.NewGuid()))
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

            services.AddSingletonAs<NoopLogStore>()
                .As<ILogStore>();
        }
    }
}
