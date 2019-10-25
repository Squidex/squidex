// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
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

        public static ILoggerFactory AddSemanticLog(this ILoggerFactory factory, ISemanticLog log)
        {
            factory.AddProvider(new SemanticLogLoggerProvider(log));

            return factory;
        }
    }
}
