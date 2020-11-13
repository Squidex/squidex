// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using GraphQL;
using GraphQL.Instrumentation;
using GraphQL.Types;
using Squidex.Infrastructure;
using Squidex.Log;

namespace Squidex.Domain.Apps.Entities.Contents.GraphQL
{
    public static class Middlewares
    {
        public static Func<ISchema, FieldMiddlewareDelegate, FieldMiddlewareDelegate> Logging(ISemanticLog log)
        {
            Guard.NotNull(log, nameof(log));

            return (_, next) =>
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
                            .WriteProperty("action", "resolveField")
                            .WriteProperty("status", "failed")
                            .WriteProperty("field", context.FieldName));

                        throw;
                    }
                };
            };
        }

        public static Func<ISchema, FieldMiddlewareDelegate, FieldMiddlewareDelegate> Errors()
        {
            return (_, next) =>
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
            };
        }
    }
}
