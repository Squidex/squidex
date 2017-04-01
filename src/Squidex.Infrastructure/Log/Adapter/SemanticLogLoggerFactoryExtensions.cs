// ==========================================================================
//  SemanticLogLoggerFactoryExtensions.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using Microsoft.Extensions.Logging;

namespace Squidex.Infrastructure.Log.Adapter
{
    public static class SemanticLogLoggerFactoryExtensions
    {
        public static ILoggerFactory AddSemanticLog(this ILoggerFactory factory, ISemanticLog semanticLog)
        {
            factory.AddProvider(new SemanticLogLoggerProvider(semanticLog));

            return factory;
        }
    }
}
