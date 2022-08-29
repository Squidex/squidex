// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using GraphQL;
using GraphQL.Resolvers;
using Microsoft.Extensions.Logging;
using Squidex.Domain.Apps.Core.Subscriptions;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Commands;
using Squidex.Infrastructure.Security;
using Squidex.Infrastructure.Translations;
using Squidex.Infrastructure.Validation;
using Squidex.Messaging.Subscriptions;
using Squidex.Shared;

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

        private abstract class BaseResolver<T, TOut> where T : TOut
        {
            protected async ValueTask<TOut> ResolveWithErrorHandlingAsync(IResolveFieldContext context)
            {
                var executionContext = (GraphQLExecutionContext)context.UserContext!;
                try
                {
                    return await ResolveCoreAsync(context, executionContext);
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

            protected abstract ValueTask<T> ResolveCoreAsync(IResolveFieldContext context, GraphQLExecutionContext executionContext);
        }

        private sealed class SyncResolver<TSource, T> : BaseResolver<T, object?>, IFieldResolver
        {
            private readonly Func<TSource, IResolveFieldContext, GraphQLExecutionContext, T> resolver;

            public SyncResolver(Func<TSource, IResolveFieldContext, GraphQLExecutionContext, T> resolver)
            {
                this.resolver = resolver;
            }

            protected override ValueTask<T> ResolveCoreAsync(IResolveFieldContext context, GraphQLExecutionContext executionContext)
            {
                return new ValueTask<T>(resolver((TSource)context.Source!, context, executionContext));
            }

            public ValueTask<object?> ResolveAsync(IResolveFieldContext context)
            {
                return ResolveWithErrorHandlingAsync(context);
            }
        }

        private sealed class AsyncResolver<TSource, T> : BaseResolver<T, object?>, IFieldResolver
        {
            private readonly Func<TSource, IResolveFieldContext, GraphQLExecutionContext, Task<T>> resolver;

            public AsyncResolver(Func<TSource, IResolveFieldContext, GraphQLExecutionContext, Task<T>> resolver)
            {
                this.resolver = resolver;
            }

            protected override async ValueTask<T> ResolveCoreAsync(IResolveFieldContext context, GraphQLExecutionContext executionContext)
            {
                return await resolver((TSource)context.Source!, context, executionContext);
            }

            public ValueTask<object?> ResolveAsync(IResolveFieldContext context)
            {
                return ResolveWithErrorHandlingAsync(context);
            }
        }

        private sealed class SyncStreamResolver : BaseResolver<IObservable<object?>, IObservable<object?>>, ISourceStreamResolver
        {
            private readonly Func<IResolveFieldContext, GraphQLExecutionContext, IObservable<object?>> resolver;

            public SyncStreamResolver(Func<IResolveFieldContext, GraphQLExecutionContext, IObservable<object?>> resolver)
            {
                this.resolver = resolver;
            }

            protected override ValueTask<IObservable<object?>> ResolveCoreAsync(IResolveFieldContext context, GraphQLExecutionContext executionContext)
            {
                return new ValueTask<IObservable<object?>>(resolver(context, executionContext));
            }

            public ValueTask<IObservable<object?>> ResolveAsync(IResolveFieldContext context)
            {
                return ResolveWithErrorHandlingAsync(context);
            }
        }

        public static IFieldResolver Command(string permissionId, Func<IResolveFieldContext, ICommand> action)
        {
            return new AsyncResolver<object, object>(async (source, fieldContext, context) =>
            {
                var schemaId = fieldContext.FieldDefinition.SchemaNamedId();

                if (!context.Context.Allows(permissionId, schemaId?.Name ?? Permission.Any))
                {
                    throw new DomainForbiddenException(T.Get("common.errorNoPermission"));
                }

                var command = action(fieldContext);

                // The app identifier is set from the http context.
                if (command is ISchemaCommand schemaCommand && schemaId != null)
                {
                    schemaCommand.SchemaId = schemaId;
                }

                command.ExpectedVersion = fieldContext.GetArgument("expectedVersion", EtagVersion.Any);

                var commandContext =
                    await context.Resolve<ICommandBus>()
                        .PublishAsync(command, fieldContext.CancellationToken);

                return commandContext.PlainResult!;
            });
        }

        public static ISourceStreamResolver Stream(string permissionId, Func<IResolveFieldContext, AppSubscription> action)
        {
            return new SyncStreamResolver((fieldContext, context) =>
            {
                if (!context.Context.UserPermissions.Includes(PermissionIds.ForApp(permissionId, context.Context.App.Name)))
                {
                    throw new DomainForbiddenException(T.Get("common.errorNoPermission"));
                }

                var subscription = action(fieldContext);

                // The app id is taken from the URL so we cannot get events from other apps.
                subscription.AppId = context.Context.App.Id;

                // We also check the subscriptions on the source server.
                subscription.Permissions = context.Context.UserPermissions;

                return context.Resolve<ISubscriptionService>().Subscribe<object>(subscription);
            });
        }
    }
}
