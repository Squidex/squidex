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
        builder.ConfigureSemanticLog(config);

        builder.Services.Configure<RequestLogOptions>(config,
            "logging");

        builder.Services.Configure<RequestLogStoreOptions>(config,
            "logging");

        builder.Services.AddSingletonAs(_ => new ApplicationInfoLogAppender(typeof(LoggingServices).Assembly, Guid.NewGuid()))
            .As<ILogAppender>();

        builder.Services.AddSingletonAs<ActionContextLogAppender>()
            .As<ILogAppender>();
    }
}
