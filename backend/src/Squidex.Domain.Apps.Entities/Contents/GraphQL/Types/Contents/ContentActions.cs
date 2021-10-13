// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using GraphQL;
using GraphQL.Resolvers;
using GraphQL.Types;
using NodaTime;
using Squidex.Domain.Apps.Core;
using Squidex.Domain.Apps.Core.Contents;
using Squidex.Domain.Apps.Entities.Contents.Commands;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Json.Objects;
using Squidex.Infrastructure.Translations;
using Squidex.Shared;

namespace Squidex.Domain.Apps.Entities.Contents.GraphQL.Types.Contents
{
    internal static class ContentActions
    {
        public static class Json
        {
            public static readonly QueryArguments Arguments = new QueryArguments
            {
                new QueryArgument(AllTypes.String)
                {
                    Name = "path",
                    Description = FieldDescriptions.JsonPath,
                    DefaultValue = null
                }
            };

            public static readonly ValueResolver Resolver = (value, fieldContext, context) =>
            {
                if (fieldContext.Arguments.TryGetValue("path", out var v) && v.Value is string path)
                {
                    value.TryGetByPath(path, out var result);

                    return result!;
                }

                return value;
            };
        }

        public static readonly QueryArguments JsonPath = new QueryArguments
        {
            new QueryArgument(AllTypes.String)
            {
                Name = "path",
                Description = FieldDescriptions.JsonPath,
                DefaultValue = null
            }
        };

        public static class Find
        {
            public static readonly QueryArguments Arguments = new QueryArguments
            {
                new QueryArgument(AllTypes.NonNullString)
                {
                    Name = "id",
                    Description = FieldDescriptions.EntityId,
                    DefaultValue = null
                },
                new QueryArgument(AllTypes.Int)
                {
                    Name = "version",
                    Description = FieldDescriptions.QueryVersion,
                    DefaultValue = null
                }
            };

            public static readonly IFieldResolver Resolver = Resolvers.Async<object, object?>(async (_, fieldContext, context) =>
            {
                var contentId = fieldContext.GetArgument<DomainId>("id");
                var contentSchema = fieldContext.FieldDefinition.SchemaId();

                var version = fieldContext.GetArgument<int?>("version");

                if (version >= 0)
                {
                    return await context.FindContentAsync(contentSchema, contentId, version.Value,
                        fieldContext.CancellationToken);
                }
                else
                {
                    return await context.FindContentAsync(DomainId.Create(contentSchema), contentId,
                        fieldContext.CancellationToken);
                }
            });
        }

        public static class QueryOrReferencing
        {
            public static readonly QueryArguments Arguments = new QueryArguments
            {
                new QueryArgument(AllTypes.Int)
                {
                    Name = "top",
                    Description = FieldDescriptions.QueryTop,
                    DefaultValue = null
                },
                new QueryArgument(AllTypes.Int)
                {
                    Name = "skip",
                    Description = FieldDescriptions.QuerySkip,
                    DefaultValue = 0
                },
                new QueryArgument(AllTypes.String)
                {
                    Name = "filter",
                    Description = FieldDescriptions.QueryFilter,
                    DefaultValue = null
                },
                new QueryArgument(AllTypes.String)
                {
                    Name = "orderby",
                    Description = FieldDescriptions.QueryOrderBy,
                    DefaultValue = null
                },
                new QueryArgument(AllTypes.String)
                {
                    Name = "search",
                    Description = FieldDescriptions.QuerySearch,
                    DefaultValue = null
                }
            };

            public static readonly IFieldResolver Query = Resolvers.Async<object, object>(async (_, fieldContext, context) =>
            {
                var query = fieldContext.BuildODataQuery();

                var q = Q.Empty.WithODataQuery(query).WithoutTotal();

                return await context.QueryContentsAsync(fieldContext.FieldDefinition.SchemaId(), q,
                    fieldContext.CancellationToken);
            });

            public static readonly IFieldResolver QueryWithTotal = Resolvers.Async<object, object>(async (_, fieldContext, context) =>
            {
                var query = fieldContext.BuildODataQuery();

                var q = Q.Empty.WithODataQuery(query);

                return await context.QueryContentsAsync(fieldContext.FieldDefinition.SchemaId(), q,
                    fieldContext.CancellationToken);
            });

            public static readonly IFieldResolver Referencing = Resolvers.Async<IContentEntity, object?>(async (source, fieldContext, context) =>
            {
                var query = fieldContext.BuildODataQuery();

                var q = Q.Empty.WithODataQuery(query).WithReference(source.Id).WithoutTotal();

                return await context.QueryContentsAsync(fieldContext.FieldDefinition.SchemaId(), q,
                    fieldContext.CancellationToken);
            });

            public static readonly IFieldResolver ReferencingWithTotal = Resolvers.Async<IContentEntity, object?>(async (source, fieldContext, context) =>
            {
                var query = fieldContext.BuildODataQuery();

                var q = Q.Empty.WithODataQuery(query).WithReference(source.Id);

                return await context.QueryContentsAsync(fieldContext.FieldDefinition.SchemaId(), q,
                    fieldContext.CancellationToken);
            });
        }

        public static class Create
        {
            public static QueryArguments Arguments(IGraphType inputType)
            {
                return new QueryArguments
                {
                    new QueryArgument(new NonNullGraphType(inputType))
                    {
                        Name = "data",
                        Description = FieldDescriptions.ContentRequestData,
                        DefaultValue = null
                    },
                    new QueryArgument(AllTypes.Boolean)
                    {
                        Name = "publish",
                        Description = FieldDescriptions.ContentRequestPublish,
                        DefaultValue = false
                    },
                    new QueryArgument(AllTypes.String)
                    {
                        Name = "status",
                        Description = FieldDescriptions.ContentRequestOptionalStatus,
                        DefaultValue = null
                    },
                    new QueryArgument(AllTypes.String)
                    {
                        Name = "id",
                        Description = FieldDescriptions.ContentRequestOptionalId,
                        DefaultValue = null
                    }
                };
            }

            public static readonly IFieldResolver Resolver = ResolveAsync(Permissions.AppContentsCreate, c =>
            {
                var contentId = c.GetArgument<string?>("id");
                var contentData = c.GetArgument<ContentData>("data");
                var contentStatus = c.GetArgument<string?>("status");

                var command = new CreateContent { Data = contentData };

                if (!string.IsNullOrWhiteSpace(contentId))
                {
                    command.ContentId = DomainId.Create(contentId);
                }

                if (!string.IsNullOrWhiteSpace(contentStatus))
                {
                    command.Status = new Status(contentStatus);
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
                return new QueryArguments
                {
                    new QueryArgument(AllTypes.NonNullString)
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
                    new QueryArgument(AllTypes.Boolean)
                    {
                        Name = "publish",
                        Description = FieldDescriptions.ContentRequestPublish,
                        DefaultValue = false
                    },
                    new QueryArgument(AllTypes.String)
                    {
                        Name = "status",
                        Description = FieldDescriptions.ContentRequestOptionalStatus,
                        DefaultValue = null
                    },
                    new QueryArgument(AllTypes.Int)
                    {
                        Name = "expectedVersion",
                        Description = FieldDescriptions.EntityExpectedVersion,
                        DefaultValue = EtagVersion.Any
                    }
                };
            }

            public static readonly IFieldResolver Resolver = ResolveAsync(Permissions.AppContentsUpsert, c =>
            {
                var contentId = c.GetArgument<string>("id");
                var contentData = c.GetArgument<ContentData>("data");
                var contentStatus = c.GetArgument<string?>("status");

                var id = DomainId.Create(contentId);

                var command = new UpsertContent { ContentId = id, Data = contentData };

                if (!string.IsNullOrWhiteSpace(contentStatus))
                {
                    command.Status = new Status(contentStatus);
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
                return new QueryArguments
                {
                    new QueryArgument(AllTypes.String)
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
                    new QueryArgument(AllTypes.Int)
                    {
                        Name = "expectedVersion",
                        Description = FieldDescriptions.EntityExpectedVersion,
                        DefaultValue = EtagVersion.Any
                    }
                };
            }

            public static readonly IFieldResolver Resolver = ResolveAsync(Permissions.AppContentsUpdateOwn, c =>
            {
                var contentId = c.GetArgument<DomainId>("id");
                var contentData = c.GetArgument<ContentData>("data");

                return new UpdateContent { ContentId = contentId, Data = contentData };
            });
        }

        public static class Patch
        {
            public static QueryArguments Arguments(IGraphType inputType)
            {
                return new QueryArguments
                {
                    new QueryArgument(AllTypes.String)
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
                    new QueryArgument(AllTypes.Int)
                    {
                        Name = "expectedVersion",
                        Description = FieldDescriptions.EntityExpectedVersion,
                        DefaultValue = EtagVersion.Any
                    }
                };
            }

            public static readonly IFieldResolver Resolver = ResolveAsync(Permissions.AppContentsUpdateOwn, c =>
            {
                var contentId = c.GetArgument<DomainId>("id");
                var contentData = c.GetArgument<ContentData>("data");

                return new PatchContent { ContentId = contentId, Data = contentData };
            });
        }

        public static class ChangeStatus
        {
            public static readonly QueryArguments Arguments = new QueryArguments
            {
                new QueryArgument(AllTypes.NonNullString)
                {
                    Name = "id",
                    Description = FieldDescriptions.EntityId,
                    DefaultValue = null
                },
                new QueryArgument(AllTypes.NonNullString)
                {
                    Name = "status",
                    Description = FieldDescriptions.ContentRequestStatus,
                    DefaultValue = null
                },
                new QueryArgument(AllTypes.DateTime)
                {
                    Name = "dueTime",
                    Description = FieldDescriptions.ContentRequestDueTime,
                    DefaultValue = null
                },
                new QueryArgument(AllTypes.Int)
                {
                    Name = "expectedVersion",
                    Description = FieldDescriptions.EntityExpectedVersion,
                    DefaultValue = EtagVersion.Any
                }
            };

            public static readonly IFieldResolver Resolver = ResolveAsync(Permissions.AppContentsChangeStatusOwn, c =>
            {
                var contentId = c.GetArgument<DomainId>("id");
                var contentStatus = c.GetArgument<Status>("status");
                var contentDueTime = c.GetArgument<Instant?>("dueTime");

                return new ChangeContentStatus { ContentId = contentId, Status = contentStatus, DueTime = contentDueTime };
            });
        }

        public static class Delete
        {
            public static readonly QueryArguments Arguments = new QueryArguments
            {
                new QueryArgument(AllTypes.NonNullString)
                {
                    Name = "id",
                    Description = "The id of the content (usually GUID).",
                    DefaultValue = null
                },
                new QueryArgument(AllTypes.Int)
                {
                    Name = "expectedVersion",
                    Description = FieldDescriptions.EntityExpectedVersion,
                    DefaultValue = EtagVersion.Any
                }
            };

            public static readonly IFieldResolver Resolver = ResolveAsync(Permissions.AppContentsDeleteOwn, c =>
            {
                var contentId = c.GetArgument<DomainId>("id");

                return new DeleteContent { ContentId = contentId };
            });
        }

        private static IFieldResolver ResolveAsync(string permissionId, Func<IResolveFieldContext, ContentCommand> action)
        {
            return Resolvers.Async<object, object>(async (source, fieldContext, context) =>
            {
                var schemaId = fieldContext.FieldDefinition.SchemaNamedId();

                CheckPermission(permissionId, context, schemaId);

                var contentCommand = action(fieldContext);

                contentCommand.SchemaId = schemaId;
                contentCommand.ExpectedVersion = fieldContext.GetArgument("expectedVersion", EtagVersion.Any);

                var commandContext = await context.CommandBus.PublishAsync(contentCommand);

                return commandContext.PlainResult!;
            });
        }

        private static void CheckPermission(string permissionId, GraphQLExecutionContext context, NamedId<DomainId> schemaId)
        {
            if (!context.Context.Allows(permissionId, schemaId.Name))
            {
                throw new DomainForbiddenException(T.Get("common.errorNoPermission"));
            }
        }
    }
}
