// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Reactive.Linq;
using GraphQL;
using GraphQL.Resolvers;
using GraphQL.Types;
using NodaTime;
using Squidex.Domain.Apps.Core;
using Squidex.Domain.Apps.Core.Contents;
using Squidex.Domain.Apps.Core.Rules.EnrichedEvents;
using Squidex.Domain.Apps.Core.Subscriptions;
using Squidex.Domain.Apps.Entities.Contents.Commands;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Translations;
using Squidex.Messaging.Subscriptions;
using Squidex.Shared;

namespace Squidex.Domain.Apps.Entities.Contents.GraphQL.Types.Contents;

internal static class ContentActions
{
    public static class Json
    {
        public static readonly QueryArguments Arguments =
        [
            new QueryArgument(Scalars.String)
            {
                Name = "path",
                Description = FieldDescriptions.JsonPath,
                DefaultValue = null
            },
        ];

        public static readonly ValueResolver<object> Resolver = (value, fieldContext, context) =>
        {
            if (fieldContext.Arguments != null &&
                fieldContext.Arguments.TryGetValue("path", out var contextValue) &&
                contextValue.Value is string path)
            {
                value.TryGetByPath(path, out var result);

                return result!;
            }

            return value;
        };
    }

    public static readonly QueryArguments JsonPath =
    [
        new QueryArgument(Scalars.String)
        {
            Name = "path",
            Description = FieldDescriptions.JsonPath,
            DefaultValue = null
        },
    ];

    public static class Find
    {
        public static readonly QueryArguments Arguments =
        [
            new QueryArgument(Scalars.NonNullString)
            {
                Name = "id",
                Description = FieldDescriptions.EntityId,
                DefaultValue = null
            },
            new QueryArgument(Scalars.Int)
            {
                Name = "version",
                Description = FieldDescriptions.QueryVersion,
                DefaultValue = null
            },
        ];

        public static readonly IFieldResolver Resolver = Resolvers.Sync<object, object?>((_, fieldContext, context) =>
        {
            var contentId = fieldContext.GetArgument<DomainId>("id");

            var contentSchemaId = fieldContext.FieldDefinition.SchemaId();
            var contentVersion = fieldContext.GetArgument<int?>("version");

            if (contentVersion >= 0)
            {
                return context.GetContent(contentSchemaId, contentId, contentVersion.Value);
            }
            else
            {
                return context.GetContent(contentSchemaId, contentId,
                    fieldContext.FieldNames(),
                    fieldContext.CacheDuration());
            }
        });
    }

    public static class FindSingleton
    {
        public static readonly QueryArguments Arguments =
        [
            new QueryArgument(Scalars.Int)
            {
                Name = "version",
                Description = FieldDescriptions.QueryVersion,
                DefaultValue = null
            },
        ];

        public static readonly IFieldResolver Resolver = Resolvers.Sync<object, object?>((_, fieldContext, context) =>
        {
            var contentSchemaId = fieldContext.FieldDefinition.SchemaId();
            var contentVersion = fieldContext.GetArgument<int?>("version");

            if (contentVersion >= 0)
            {
                return context.GetContent(contentSchemaId, contentSchemaId, contentVersion.Value);
            }
            else
            {
                return context.GetContent(contentSchemaId, contentSchemaId,
                    fieldContext.FieldNames(),
                    fieldContext.CacheDuration());
            }
        });
    }

    public static class QueryByIds
    {
        public static readonly QueryArguments Arguments =
        [
            new QueryArgument(Scalars.NonNullStrings)
            {
                Name = "ids",
                Description = FieldDescriptions.EntityIds,
                DefaultValue = null,
            },
        ];

        public static readonly IFieldResolver Resolver = Resolvers.Sync<object, object?>((_, fieldContext, context) =>
        {
            var ids = fieldContext.GetArgument<DomainId[]>("ids").ToList();

            return context.GetContents(ids,
                fieldContext.FieldNames(),
                fieldContext.CacheDuration());
        });
    }

    public static class QueryOrReferencing
    {
        public static readonly QueryArguments Arguments =
        [
            new QueryArgument(Scalars.Int)
            {
                Name = "top",
                Description = FieldDescriptions.QueryTop,
                DefaultValue = null
            },
            new QueryArgument(Scalars.Int)
            {
                Name = "skip",
                Description = FieldDescriptions.QuerySkip,
                DefaultValue = 0
            },
            new QueryArgument(Scalars.String)
            {
                Name = "filter",
                Description = FieldDescriptions.QueryFilter,
                DefaultValue = null
            },
            new QueryArgument(Scalars.String)
            {
                Name = "orderby",
                Description = FieldDescriptions.QueryOrderBy,
                DefaultValue = null
            },
            new QueryArgument(Scalars.String)
            {
                Name = "search",
                Description = FieldDescriptions.QuerySearch,
                DefaultValue = null
            },
        ];

        public static readonly IFieldResolver Query = Resolvers.Async<object, object>(async (_, fieldContext, context) =>
        {
            var query = fieldContext.BuildODataQuery();

            var q = Q.Empty
                .WithODataQuery(query)
                .WithoutTotal()
                .WithFields(fieldContext.FieldNames());

            return await context.QueryContentsAsync(fieldContext.FieldDefinition.SchemaId().ToString(), q,
                fieldContext.CancellationToken);
        });

        public static readonly IFieldResolver QueryWithTotal = Resolvers.Async<object, object>(async (_, fieldContext, context) =>
        {
            var query = fieldContext.BuildODataQuery();

            var q = Q.Empty
                .WithODataQuery(query)
                .WithFields(fieldContext.FieldNames());

            return await context.QueryContentsAsync(fieldContext.FieldDefinition.SchemaId().ToString(), q,
                fieldContext.CancellationToken);
        });

        public static readonly IFieldResolver Referencing = Resolvers.Async<Content, object?>(async (source, fieldContext, context) =>
        {
            var query = fieldContext.BuildODataQuery();

            var q = Q.Empty
                .WithODataQuery(query)
                .WithReference(source.Id)
                .WithoutTotal()
                .WithFields(fieldContext.FieldNames());

            return await context.QueryContentsAsync(fieldContext.FieldDefinition.SchemaId().ToString(), q,
                fieldContext.CancellationToken);
        });

        public static readonly IFieldResolver ReferencingWithTotal = Resolvers.Async<Content, object?>(async (source, fieldContext, context) =>
        {
            var query = fieldContext.BuildODataQuery();

            var q = Q.Empty
                .WithODataQuery(query)
                .WithReference(source.Id)
                .WithFields(fieldContext.FieldNames());

            return await context.QueryContentsAsync(fieldContext.FieldDefinition.SchemaId().ToString(), q,
                fieldContext.CancellationToken);
        });

        public static readonly IFieldResolver References = Resolvers.Async<Content, object?>(async (source, fieldContext, context) =>
        {
            var query = fieldContext.BuildODataQuery();

            var q = Q.Empty
                .WithODataQuery(query)
                .WithReferencing(source.Id)
                .WithoutTotal()
                .WithFields(fieldContext.FieldNames());

            return await context.QueryContentsAsync(fieldContext.FieldDefinition.SchemaId().ToString(), q,
                fieldContext.CancellationToken);
        });

        public static readonly IFieldResolver ReferencesWithTotal = Resolvers.Async<Content, object?>(async (source, fieldContext, context) =>
        {
            var query = fieldContext.BuildODataQuery();

            var q = Q.Empty
                .WithODataQuery(query)
                .WithReferencing(source.Id)
                .WithFields(fieldContext.FieldNames());

            return await context.QueryContentsAsync(fieldContext.FieldDefinition.SchemaId().ToString(), q,
                fieldContext.CancellationToken);
        });
    }

    public static class Create
    {
        public static QueryArguments Arguments(IGraphType inputType)
        {
            return
            [
                new QueryArgument(new NonNullGraphType(inputType))
                {
                    Name = "data",
                    Description = FieldDescriptions.ContentRequestData,
                    DefaultValue = null
                },
                new QueryArgument(Scalars.Boolean)
                {
                    Name = "publish",
                    Description = FieldDescriptions.ContentRequestPublish,
                    DefaultValue = false
                },
                new QueryArgument(Scalars.String)
                {
                    Name = "status",
                    Description = FieldDescriptions.ContentRequestOptionalStatus,
                    DefaultValue = null
                },
                new QueryArgument(Scalars.String)
                {
                    Name = "id",
                    Description = FieldDescriptions.ContentRequestOptionalId,
                    DefaultValue = null
                },
            ];
        }

        public static readonly IFieldResolver Resolver = ContentCommand(PermissionIds.AppContentsCreate, c =>
        {
            var command = new CreateContent
            {
                // The data is converted from input args.
                Data = c.GetArgument<ContentData>("data")
            };

            var status = c.GetArgument<string?>("status");

            if (!string.IsNullOrWhiteSpace(status))
            {
                command.Status = new Status(status);
            }
            else if (c.GetArgument<bool>("publish"))
            {
                command.Status = Status.Published;
            }

            return command;
        });
    }

    public static class Upsert
    {
        public static QueryArguments Arguments(IGraphType inputType)
        {
            return
            [
                new QueryArgument(Scalars.NonNullString)
                {
                    Name = "id",
                    Description = FieldDescriptions.EntityId,
                    DefaultValue = null
                },
                new QueryArgument(new NonNullGraphType(inputType))
                {
                    Name = "data",
                    Description = FieldDescriptions.ContentRequestData,
                    DefaultValue = null
                },
                new QueryArgument(Scalars.Boolean)
                {
                    Name = "publish",
                    Description = FieldDescriptions.ContentRequestPublish,
                    DefaultValue = false
                },
                new QueryArgument(Scalars.Boolean)
                {
                    Name = "patch",
                    Description = FieldDescriptions.ContentRequestPatch,
                    DefaultValue = false
                },
                new QueryArgument(Scalars.String)
                {
                    Name = "status",
                    Description = FieldDescriptions.ContentRequestOptionalStatus,
                    DefaultValue = null
                },
                new QueryArgument(Scalars.Int)
                {
                    Name = "expectedVersion",
                    Description = FieldDescriptions.EntityExpectedVersion,
                    DefaultValue = EtagVersion.Any
                },
            ];
        }

        public static readonly IFieldResolver Resolver = ContentCommand(PermissionIds.AppContentsUpsert, c =>
        {
            var command = new UpsertContent
            {
                // The data is converted from input args.
                Data = c.GetArgument<ContentData>("data"),

                // True, to make a path, if the content exits.
                Patch = c.GetArgument<bool>("patch"),
            };

            var status = c.GetArgument<string?>("status");

            if (!string.IsNullOrWhiteSpace(status))
            {
                command.Status = new Status(status);
            }
            else if (c.GetArgument<bool>("publish"))
            {
                command.Status = Status.Published;
            }

            return command;
        });
    }

    public static class Update
    {
        public static QueryArguments Arguments(IGraphType inputType)
        {
            return
            [
                new QueryArgument(Scalars.String)
                {
                    Name = "id",
                    Description = FieldDescriptions.EntityId,
                    DefaultValue = null
                },
                new QueryArgument(new NonNullGraphType(inputType))
                {
                    Name = "data",
                    Description = FieldDescriptions.ContentRequestData,
                    DefaultValue = null
                },
                new QueryArgument(Scalars.Int)
                {
                    Name = "expectedVersion",
                    Description = FieldDescriptions.EntityExpectedVersion,
                    DefaultValue = EtagVersion.Any
                },
            ];
        }

        public static readonly IFieldResolver Resolver = ContentCommand(PermissionIds.AppContentsUpdateOwn, c =>
        {
            return new PatchContent
            {
                // The data is converted from input args.
                Data = c.GetArgument<ContentData>("data")!
            };
        });
    }

    public static class Patch
    {
        public static QueryArguments Arguments(IGraphType inputType)
        {
            return
            [
                new QueryArgument(Scalars.String)
                {
                    Name = "id",
                    Description = FieldDescriptions.EntityId,
                    DefaultValue = null
                },
                new QueryArgument(new NonNullGraphType(inputType))
                {
                    Name = "data",
                    Description = FieldDescriptions.ContentRequestData,
                    DefaultValue = null
                },
                new QueryArgument(Scalars.Int)
                {
                    Name = "expectedVersion",
                    Description = FieldDescriptions.EntityExpectedVersion,
                    DefaultValue = EtagVersion.Any
                },
            ];
        }

        public static readonly IFieldResolver Resolver = ContentCommand(PermissionIds.AppContentsUpdateOwn, c =>
        {
            return new PatchContent
            {
                // The data is converted from input args.
                Data = c.GetArgument<ContentData>("data")!
            };
        });
    }

    public static class ChangeStatus
    {
        public static readonly QueryArguments Arguments =
        [
            new QueryArgument(Scalars.NonNullString)
            {
                Name = "id",
                Description = FieldDescriptions.EntityId,
                DefaultValue = null
            },
            new QueryArgument(Scalars.NonNullString)
            {
                Name = "status",
                Description = FieldDescriptions.ContentRequestStatus,
                DefaultValue = null
            },
            new QueryArgument(Scalars.DateTime)
            {
                Name = "dueTime",
                Description = FieldDescriptions.ContentRequestDueTime,
                DefaultValue = null
            },
            new QueryArgument(Scalars.Int)
            {
                Name = "expectedVersion",
                Description = FieldDescriptions.EntityExpectedVersion,
                DefaultValue = EtagVersion.Any
            },
        ];

        public static readonly IFieldResolver Resolver = ContentCommand(PermissionIds.AppContentsChangeStatusOwn, c =>
        {
            return new ChangeContentStatus
            {
                // Main parameter to set the status.
                Status = c.GetArgument<Status>("status"),

                // This is an optional field to delay the status change.
                DueTime = c.GetArgument<Instant?>("dueTime"),
            };
        });
    }

    public static class Delete
    {
        public static readonly QueryArguments Arguments =
        [
            new QueryArgument(Scalars.NonNullString)
            {
                Name = "id",
                Description = "The ID of the content (usually GUID).",
                DefaultValue = null
            },
            new QueryArgument(Scalars.Int)
            {
                Name = "expectedVersion",
                Description = FieldDescriptions.EntityExpectedVersion,
                DefaultValue = EtagVersion.Any
            },
        ];

        public static readonly IFieldResolver Resolver = ContentCommand(PermissionIds.AppContentsDeleteOwn, c =>
        {
            return new DeleteContent();
        });
    }

    public static class Subscription
    {
        public static readonly QueryArguments Arguments =
        [
            new QueryArgument(Scalars.EnrichedContentEventType)
            {
                Name = "type",
                Description = FieldDescriptions.EventType,
                DefaultValue = null
            },
            new QueryArgument(Scalars.String)
            {
                Name = "schemaName",
                Description = FieldDescriptions.ContentSchemaName,
                DefaultValue = null
            },
        ];

        public static readonly ISourceStreamResolver Resolver = new SourceStreamResolver<object>(async fieldContext =>
        {
            var context = (GraphQLExecutionContext)fieldContext.UserContext;

            var app = context.Context.App;

            if (!context.Context.UserPermissions.Includes(PermissionIds.ForApp(PermissionIds.AppContentsRead, app.Name)))
            {
                throw new DomainForbiddenException(T.Get("common.errorNoPermission"));
            }

            var key = $"content-{app.Id}";

            var subscription = new ContentSubscription
            {
                Permissions = context.Context.UserPermissions,

                // Primary filter for the event types.
                Type = fieldContext.GetArgument<EnrichedContentEventType?>("type"),

                // The name of the schema is used instead of the ID for a simpler API.
                SchemaName = fieldContext.GetArgument<string?>("schemaName"),
            };

            var observable =
                await context.Resolve<ISubscriptionService>()
                    .SubscribeAsync(key, subscription, fieldContext.CancellationToken);

            return observable.OfType<EnrichedContentEvent>();
        });
    }

    private static IFieldResolver ContentCommand(string permissionId, Func<IResolveFieldContext, ContentCommand> creator)
    {
        return Resolvers.Command(permissionId, c =>
        {
            var command = creator(c);

            var contentId = c.GetArgument<string?>("id");

            if (!string.IsNullOrWhiteSpace(contentId))
            {
                // Same parameter for all commands.
                command.ContentId = DomainId.Create(contentId);
            }

            return command;
        });
    }
}
