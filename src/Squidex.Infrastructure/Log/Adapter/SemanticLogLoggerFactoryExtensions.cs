// ==========================================================================
//  SemanticLogLoggerFactoryExtensions.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Squidex.Infrastructure.Log.Adapter
{
    public static class SemanticLogLoggerFactoryExtensions
    {
        public static ILoggingBuilder AddSemanticLog(this ILoggingBuilder builder)
        {
            builder.Services.AddSingleton<ILoggerProvider, SemanticLogLoggerProvider>();

            return builder;
        }
    }
}
