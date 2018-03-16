// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Log;
using Squidex.Pipeline;

#pragma warning disable RECS0092 // Convert field to readonly

namespace Squidex.Config.Domain
{
    public static class LoggingServices
    {
        private static ILogChannel console = new ConsoleLogChannel();
        private static ILogChannel file;

        public static void AddMyLoggingServices(this IServiceCollection services, IConfiguration config)
        {
            if (config.GetValue<bool>("logging:human"))
            {
                services.AddSingletonAs(c => new Func<IObjectWriter>(() => new JsonLogWriter(Formatting.Indented, true)));
            }
            else
            {
                services.AddSingletonAs(c => new Func<IObjectWriter>(() => new JsonLogWriter()));
            }

            var loggingFile = config.GetValue<string>("logging:file");

            if (!string.IsNullOrWhiteSpace(loggingFile))
            {
                services.AddSingletonAs(file ?? (file = new FileChannel(loggingFile)))
                    .As<ILogChannel>()
                    .As<IInitializable>();
            }

            services.AddSingletonAs(console)
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
        }
    }
}
