// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using GraphQL;
using GraphQL.Resolvers;
using Squidex.Domain.Apps.Core.Subscriptions;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Commands;
using Squidex.Infrastructure.Security;
using Squidex.Infrastructure.Translations;
using Squidex.Messaging.Subscriptions;
using Squidex.Shared;

namespace Squidex.Domain.Apps.Entities.Contents.GraphQL.Types;

public static class Resolvers
{
    public static IFieldResolver Sync<TSource, T>(Func<TSource, T> resolver)
    {
        return new FuncFieldResolver<TSource, T>(x => resolver(x.Source));
    }

    public static IFieldResolver Sync<TSource, T>(Func<TSource, IResolveFieldContext, GraphQLExecutionContext, T> resolver)
    {
        return new FuncFieldResolver<TSource, T>(x => resolver(x.Source, x, (GraphQLExecutionContext)x.UserContext));
    }

    public static IFieldResolver Async<TSource, T>(Func<TSource, ValueTask<T?>> resolver)
    {
        return new FuncFieldResolver<TSource, T>(x => resolver(x.Source));
    }

    public static IFieldResolver Async<TSource, T>(Func<TSource, IResolveFieldContext, GraphQLExecutionContext, ValueTask<T?>> resolver)
    {
        return new FuncFieldResolver<TSource, T>(x => resolver(x.Source, x, (GraphQLExecutionContext)x.UserContext));
    }

    public static IFieldResolver Command(string permissionId, Func<IResolveFieldContext, ICommand> action)
    {
        return Async<object, object>(async (source, fieldContext, context) =>
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
        return new SourceStreamResolver<object>(fieldContext =>
        {
            var context = (GraphQLExecutionContext)fieldContext.UserContext;

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
