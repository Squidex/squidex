// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using GraphQL;
using GraphQL.Execution;
using GraphQL.Resolvers;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Commands;
using Squidex.Infrastructure.Security;
using Squidex.Infrastructure.Translations;

namespace Squidex.Domain.Apps.Entities.Contents.GraphQL.Types;

public static class Resolvers
{
    public static IFieldResolver Sync<TSource, T>(Func<TSource, T> resolver)
    {
        return new FuncFieldResolver<TSource, T>(c => resolver(c.Source));
    }

    public static IFieldResolver Sync<TSource, T>(Func<TSource, IResolveFieldContext, GraphQLExecutionContext, T> resolver)
    {
        return new FuncFieldResolver<TSource, T>(c => resolver(c.Source, c, (GraphQLExecutionContext)c.UserContext));
    }

    public static IFieldResolver Async<TSource, T>(Func<TSource, ValueTask<T?>> resolver)
    {
        return new FuncFieldResolver<TSource, T>(c => resolver(c.Source));
    }

    public static IFieldResolver Async<TSource, T>(Func<TSource, IResolveFieldContext, GraphQLExecutionContext, ValueTask<T?>> resolver)
    {
        return new FuncFieldResolver<TSource, T>(async c =>
        {
            using (var activity = Telemetry.Activities.StartActivity($"gql/{c.FieldDefinition.Name}"))
            {
                activity?.SetTag("/gql/fieldName", c.FieldDefinition.Name);
                activity?.SetTag("/gql/fieldType", c.FieldDefinition.ResolvedType?.ToString());

                if (activity != null && c.Arguments != null)
                {
                    foreach (var (key, value) in c.Arguments)
                    {
                        if (value.Source == ArgumentSource.Literal)
                        {
                            activity.SetTag($"arg/{key}", value);
                        }
                    }
                }

                return await resolver(c.Source, c, (GraphQLExecutionContext)c.UserContext);
            }
        });
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
                await context.Resolve<ICommandBus>().PublishAsync(command,
                    fieldContext.CancellationToken);

            return commandContext.PlainResult!;
        });
    }
}
