// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Threading.Tasks;
using GraphQL;
using GraphQL.Resolvers;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Validation;
using Squidex.Log;

namespace Squidex.Domain.Apps.Entities.Contents.GraphQL.Types
{
    public static class Resolvers
    {
        public static IFieldResolver Sync<TSource, T>(Func<TSource, T> resolver)
        {
            return new SyncResolver<TSource, T>((source, context, execution) => resolver(source));
        }

        public static IFieldResolver Sync<TSource, T>(Func<TSource, IResolveFieldContext, GraphQLExecutionContext, T> resolver)
        {
            return new SyncResolver<TSource, T>(resolver);
        }

        public static IFieldResolver Async<TSource, T>(Func<TSource, Task<T>> resolver)
        {
            return new AsyncResolver<TSource, T>((source, context, execution) => resolver(source));
        }

        public static IFieldResolver Async<TSource, T>(Func<TSource, IResolveFieldContext, GraphQLExecutionContext, Task<T>> resolver)
        {
            return new AsyncResolver<TSource, T>(resolver);
        }

        private sealed class SyncResolver<TSource, T> : IFieldResolver<T>, IFieldResolver
        {
            private readonly Func<TSource, IResolveFieldContext, GraphQLExecutionContext, T> resolver;

            public SyncResolver(Func<TSource, IResolveFieldContext, GraphQLExecutionContext, T> resolver)
            {
                this.resolver = resolver;
            }

            public T Resolve(IResolveFieldContext context)
            {
                var executionContext = (GraphQLExecutionContext)context.UserContext;

                try
                {
                    return resolver((TSource)context.Source, context, executionContext);
                }
                catch (ValidationException ex)
                {
                    throw new ExecutionError(ex.Message);
                }
                catch (DomainException ex)
                {
                    throw new ExecutionError(ex.Message);
                }
                catch (Exception ex)
                {
                    executionContext.Log.LogWarning(ex, w => w
                        .WriteProperty("action", "resolveField")
                        .WriteProperty("status", "failed")
                        .WriteProperty("field", context.FieldDefinition.Name));

                    throw;
                }
            }

            object IFieldResolver.Resolve(IResolveFieldContext context)
            {
                return Resolve(context)!;
            }
        }

        private sealed class AsyncResolver<TSource, T> : IFieldResolver<Task<T>>, IFieldResolver
        {
            private readonly Func<TSource, IResolveFieldContext, GraphQLExecutionContext, Task<T>> resolver;

            public AsyncResolver(Func<TSource, IResolveFieldContext, GraphQLExecutionContext, Task<T>> resolver)
            {
                this.resolver = resolver;
            }

            public async Task<T> Resolve(IResolveFieldContext context)
            {
                var executionContext = (GraphQLExecutionContext)context.UserContext;

                try
                {
                    return await resolver((TSource)context.Source, context, executionContext);
                }
                catch (ValidationException ex)
                {
                    throw new ExecutionError(ex.Message);
                }
                catch (DomainException ex)
                {
                    throw new ExecutionError(ex.Message);
                }
                catch (Exception ex)
                {
                    executionContext.Log.LogWarning(ex, w => w
                        .WriteProperty("action", "resolveField")
                        .WriteProperty("status", "failed")
                        .WriteProperty("field", context.FieldDefinition.Name));

                    throw;
                }
            }

            object IFieldResolver.Resolve(IResolveFieldContext context)
            {
                return Resolve(context)!;
            }
        }
    }
}
