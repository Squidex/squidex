// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using GraphQL.Instrumentation;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Log;

namespace Squidex.Domain.Apps.Entities.Contents.GraphQL
{
    public static class LoggingMiddleware
    {
        public static Func<FieldMiddlewareDelegate, FieldMiddlewareDelegate> Create(ISemanticLog log)
        {
            Guard.NotNull(log, nameof(log));

            return new Func<FieldMiddlewareDelegate, FieldMiddlewareDelegate>(next =>
            {
                return async context =>
                {
                    try
                    {
                        return await next(context);
                    }
                    catch (Exception ex)
                    {
                        log.LogWarning(ex, w => w
                            .WriteProperty("action", "reolveField")
                            .WriteProperty("status", "failed")
                            .WriteProperty("field", context.FieldName));

                        throw ex;
                    }
                };
            });
        }
    }
}
