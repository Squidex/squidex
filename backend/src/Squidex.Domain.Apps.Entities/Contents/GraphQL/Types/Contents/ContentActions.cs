﻿// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Collections.Generic;
using GraphQL;
using GraphQL.Resolvers;
using GraphQL.Types;
using NodaTime;
using Squidex.Domain.Apps.Core.Contents;
using Squidex.Domain.Apps.Entities.Contents.Commands;
using Squidex.Domain.Apps.Entities.Contents.GraphQL.Types.Primitives;
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
                new QueryArgument(AllTypes.None)
                {
                    Name = "path",
                    Description = "The path to the json value",
                    DefaultValue = null,
                    ResolvedType = AllTypes.String
                }
            };

            public static readonly ValueResolver Resolver = (value, fieldContext, context) =>
            {
                if (fieldContext.Arguments.TryGetValue("path", out var p) && p is string path)
                {
                    value.TryGetByPath(path, out var result);

                    return result!;
                }

                return value;
            };
        }

        public static readonly QueryArguments JsonPath = new QueryArguments
        {
            new QueryArgument(AllTypes.None)
            {
                Name = "path",
                Description = "The path to the json value",
                DefaultValue = null,
                ResolvedType = AllTypes.String
            }
        };

        public static class Find
        {
            public static readonly QueryArguments Arguments = new QueryArguments
            {
                new QueryArgument(AllTypes.None)
                {
                    Name = "id",
                    Description = "The id of the content (usually GUID).",
                    DefaultValue = null,
                    ResolvedType = AllTypes.NonNullDomainId
                },
                new QueryArgument(AllTypes.None)
                {
                    Name = "version",
                    Description = "The optional version of the content to retrieve an older instance (not cached).",
                    DefaultValue = null,
                    ResolvedType = AllTypes.Int
                }
            };

            public static readonly IFieldResolver Resolver = Resolvers.Async<object, object?>(async (_, fieldContext, context) =>
            {
                var contentId = fieldContext.GetArgument<DomainId>("id");

                var version = fieldContext.GetArgument<int?>("version");

                if (version >= 0)
                {
                    return await context.FindContentAsync(fieldContext.FieldDefinition.SchemaId(), contentId, version.Value);
                }
                else
                {
                    return await context.FindContentAsync(contentId);
                }
            });
        }

        public static class QueryOrReferencing
        {
            public static readonly QueryArguments Arguments = new QueryArguments
            {
                new QueryArgument(AllTypes.None)
                {
                    Name = "top",
                    Description = "Optional number of contents to take.",
                    DefaultValue = null,
                    ResolvedType = AllTypes.Int
                },
                new QueryArgument(AllTypes.None)
                {
                    Name = "skip",
                    Description = "Optional number of contents to skip.",
                    DefaultValue = 0,
                    ResolvedType = AllTypes.Int
                },
                new QueryArgument(AllTypes.None)
                {
                    Name = "filter",
                    Description = "Optional OData filter.",
                    DefaultValue = null,
                    ResolvedType = AllTypes.String
                },
                new QueryArgument(AllTypes.None)
                {
                    Name = "orderby",
                    Description = "Optional OData order definition.",
                    DefaultValue = null,
                    ResolvedType = AllTypes.String
                },
                new QueryArgument(AllTypes.None)
                {
                    Name = "search",
                    Description = "Optional OData full text search.",
                    DefaultValue = null,
                    ResolvedType = AllTypes.String
                }
            };

            public static readonly IFieldResolver Query = Resolvers.Async<object, object>(async (_, fieldContext, context) =>
            {
                var query = fieldContext.BuildODataQuery();

                var q = Q.Empty.WithODataQuery(query).WithoutTotal();

                return await context.QueryContentsAsync(fieldContext.FieldDefinition.SchemaId(), q);
            });

            public static readonly IFieldResolver QueryWithTotal = Resolvers.Async<object, object>(async (_, fieldContext, context) =>
            {
                var query = fieldContext.BuildODataQuery();

                var q = Q.Empty.WithODataQuery(query);

                return await context.QueryContentsAsync(fieldContext.FieldDefinition.SchemaId(), q);
            });

            public static readonly IFieldResolver Referencing = Resolvers.Async<IContentEntity, object?>(async (source, fieldContext, context) =>
            {
                var query = fieldContext.BuildODataQuery();

                var q = Q.Empty.WithODataQuery(query).WithReference(source.Id).WithoutTotal();

                return await context.QueryContentsAsync(fieldContext.FieldDefinition.SchemaId(), q);
            });

            public static readonly IFieldResolver ReferencingWithTotal = Resolvers.Async<IContentEntity, object?>(async (source, fieldContext, context) =>
            {
                var query = fieldContext.BuildODataQuery();

                var q = Q.Empty.WithODataQuery(query).WithReference(source.Id);

                return await context.QueryContentsAsync(fieldContext.FieldDefinition.SchemaId(), q);
            });
        }

        public static class Create
        {
            public static QueryArguments Arguments(IGraphType inputType)
            {
                return new QueryArguments
                {
                    new QueryArgument(AllTypes.None)
                    {
                        Name = "data",
                        Description = "The data for the content.",
                        DefaultValue = null,
                        ResolvedType = new NonNullGraphType(inputType)
                    },
                    new QueryArgument(AllTypes.None)
                    {
                        Name = "publish",
                        Description = "Set to true to autopublish content on create.",
                        DefaultValue = false,
                        ResolvedType = AllTypes.Boolean
                    },
                    new QueryArgument(AllTypes.None)
                    {
                        Name = "id",
                        Description = "The optional custom content id.",
                        DefaultValue = null,
                        ResolvedType = AllTypes.String
                    }
                };
            }

            public static readonly IFieldResolver Resolver = ResolveAsync(Permissions.AppContentsCreate, c =>
            {
                var publish = c.GetArgument<bool>("publish");
                var contentData = GetContentData(c);
                var contentId = c.GetArgument<string?>("id");

                var command = new CreateContent { Data = contentData, Publish = publish };

                if (!string.IsNullOrWhiteSpace(contentId))
                {
                    var id = DomainId.Create(contentId);

                    command.ContentId = id;
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
                    new QueryArgument(AllTypes.None)
                    {
                        Name = "id",
                        Description = "The id of the content (usually GUID).",
                        DefaultValue = null,
                        ResolvedType = AllTypes.NonNullDomainId
                    },
                    new QueryArgument(AllTypes.None)
                    {
                        Name = "data",
                        Description = "The data for the content.",
                        DefaultValue = null,
                        ResolvedType = new NonNullGraphType(inputType)
                    },
                    new QueryArgument(AllTypes.None)
                    {
                        Name = "publish",
                        Description = "Set to true to autopublish content on create.",
                        DefaultValue = false,
                        ResolvedType = AllTypes.Boolean
                    },
                    new QueryArgument(AllTypes.None)
                    {
                        Name = "expectedVersion",
                        Description = "The expected version",
                        DefaultValue = EtagVersion.Any,
                        ResolvedType = AllTypes.Int
                    }
                };
            }

            public static readonly IFieldResolver Resolver = ResolveAsync(Permissions.AppContentsUpsert, c =>
            {
                var publish = c.GetArgument<bool>("publish");

                var contentData = GetContentData(c);
                var contentId = c.GetArgument<string>("id");

                var id = DomainId.Create(contentId);

                return new UpsertContent { ContentId = id, Data = contentData, Publish = publish };
            });
        }

        public static class Update
        {
            public static QueryArguments Arguments(IGraphType inputType)
            {
                return new QueryArguments
                {
                    new QueryArgument(AllTypes.None)
                    {
                        Name = "id",
                        Description = "The optional custom content id.",
                        DefaultValue = null,
                        ResolvedType = AllTypes.String
                    },
                    new QueryArgument(AllTypes.None)
                    {
                        Name = "data",
                        Description = "The data for the content.",
                        DefaultValue = null,
                        ResolvedType = new NonNullGraphType(inputType)
                    },
                    new QueryArgument(AllTypes.None)
                    {
                        Name = "expectedVersion",
                        Description = "The expected version",
                        DefaultValue = EtagVersion.Any,
                        ResolvedType = AllTypes.Int
                    }
                };
            }

            public static readonly IFieldResolver Resolver = ResolveAsync(Permissions.AppContentsUpdateOwn, c =>
            {
                var contentId = c.GetArgument<DomainId>("id");
                var contentData = GetContentData(c);

                return new UpdateContent { ContentId = contentId, Data = contentData };
            });
        }

        public static class Patch
        {
            public static QueryArguments Arguments(IGraphType inputType)
            {
                return new QueryArguments
                {
                    new QueryArgument(AllTypes.None)
                    {
                        Name = "id",
                        Description = "The optional custom content id.",
                        DefaultValue = null,
                        ResolvedType = AllTypes.String
                    },
                    new QueryArgument(AllTypes.None)
                    {
                        Name = "data",
                        Description = "The data for the content.",
                        DefaultValue = null,
                        ResolvedType = new NonNullGraphType(inputType)
                    },
                    new QueryArgument(AllTypes.None)
                    {
                        Name = "expectedVersion",
                        Description = "The expected version",
                        DefaultValue = EtagVersion.Any,
                        ResolvedType = AllTypes.Int
                    }
                };
            }

            public static readonly IFieldResolver Resolver = ResolveAsync(Permissions.AppContentsUpdateOwn, c =>
            {
                var contentId = c.GetArgument<DomainId>("id");
                var contentData = GetContentData(c);

                return new PatchContent { ContentId = contentId, Data = contentData };
            });
        }

        public static class ChangeStatus
        {
            public static readonly QueryArguments Arguments = new QueryArguments
            {
                new QueryArgument(AllTypes.None)
                {
                    Name = "id",
                    Description = "The id of the content (usually GUID).",
                    DefaultValue = null,
                    ResolvedType = AllTypes.NonNullDomainId
                },
                new QueryArgument(AllTypes.None)
                {
                    Name = "status",
                    Description = "The new status",
                    DefaultValue = null,
                    ResolvedType = AllTypes.NonNullString
                },
                new QueryArgument(AllTypes.None)
                {
                    Name = "dueTime",
                    Description = "When to change the status",
                    DefaultValue = null,
                    ResolvedType = AllTypes.Date
                },
                new QueryArgument(AllTypes.None)
                {
                    Name = "expectedVersion",
                    Description = "The expected version",
                    DefaultValue = EtagVersion.Any,
                    ResolvedType = AllTypes.Int
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
                new QueryArgument(AllTypes.None)
                {
                    Name = "id",
                    Description = "The id of the content (usually GUID).",
                    DefaultValue = null,
                    ResolvedType = AllTypes.NonNullDomainId
                },
                new QueryArgument(AllTypes.None)
                {
                    Name = "expectedVersion",
                    Description = "The expected version",
                    DefaultValue = EtagVersion.Any,
                    ResolvedType = AllTypes.Int
                }
            };

            public static readonly IFieldResolver Resolver = ResolveAsync(Permissions.AppContentsDeleteOwn, c =>
            {
                var contentId = c.GetArgument<DomainId>("id");

                return new DeleteContent { ContentId = contentId };
            });
        }

        private static ContentData GetContentData(IResolveFieldContext c)
        {
            var source = c.GetArgument<IDictionary<string, object>>("data");

            return source.ToContentData((IComplexGraphType)c.FieldDefinition.Arguments.Find("data").Flatten());
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
