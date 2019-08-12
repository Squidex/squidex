// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using GraphQL;
using GraphQL.Instrumentation;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Log;

namespace Squidex.Domain.Apps.Entities.Contents.GraphQL
{
    public static class Middlewares
    {
        public static Func<FieldMiddlewareDelegate, FieldMiddlewareDelegate> Logging(ISemanticLog log)
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

                        throw;
                    }
                };
            });
        }

        public static Func<FieldMiddlewareDelegate, FieldMiddlewareDelegate> Errors()
        {
            return new Func<FieldMiddlewareDelegate, FieldMiddlewareDelegate>(next =>
            {
                return async context =>
                {
                    try
                    {
                        return await next(context);
                    }
                    catch (DomainException ex)
                    {
                        throw new ExecutionError(ex.Message);
                    }
                };
            });
        }
    }
}
