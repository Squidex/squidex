// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using GraphQL;
using GraphQL.Resolvers;
using Microsoft.Extensions.Logging;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Validation;

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

        private sealed class SyncResolver<TSource, T> : IFieldResolver
        {
            private readonly Func<TSource, IResolveFieldContext, GraphQLExecutionContext, T> resolver;

            public SyncResolver(Func<TSource, IResolveFieldContext, GraphQLExecutionContext, T> resolver)
            {
                this.resolver = resolver;
            }

            public ValueTask<object?> ResolveAsync(IResolveFieldContext context)
            {
                var executionContext = (GraphQLExecutionContext)context.UserContext!;

                try
                {
                    var result = resolver((TSource)context.Source!, context, executionContext);

                    return new ValueTask<object?>(result);
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
                    var logFactory = executionContext.Resolve<ILoggerFactory>();

                    logFactory.CreateLogger("GraphQL").LogError(ex, "Failed to resolve field {field}.", context.FieldDefinition.Name);
                    throw;
                }
            }
        }

        private sealed class AsyncResolver<TSource, T> : IFieldResolver
        {
            private readonly Func<TSource, IResolveFieldContext, GraphQLExecutionContext, Task<T>> resolver;

            public AsyncResolver(Func<TSource, IResolveFieldContext, GraphQLExecutionContext, Task<T>> resolver)
            {
                this.resolver = resolver;
            }

            public async ValueTask<object?> ResolveAsync(IResolveFieldContext context)
            {
                var executionContext = (GraphQLExecutionContext)context.UserContext!;

                try
                {
                    var result = await resolver((TSource)context.Source!, context, executionContext);

                    return result;
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
                    var logFactory = executionContext.Resolve<ILoggerFactory>();

                    logFactory.CreateLogger("GraphQL").LogError(ex, "Failed to resolve field {field}.", context.FieldDefinition.Name);
                    throw;
                }
            }
        }
    }
}
