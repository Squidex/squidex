// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using GraphQL;
using GraphQL.Resolvers;
using GraphQL.Types;
using NodaTime;
using Squidex.Domain.Apps.Core.Contents;
using Squidex.Domain.Apps.Entities.Contents.Commands;
using Squidex.Domain.Apps.Entities.Contents.GraphQL.Types.Primitives;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Commands;
using Squidex.Infrastructure.Json.Objects;
using Squidex.Infrastructure.Validation;

namespace Squidex.Domain.Apps.Entities.Contents.GraphQL.Types.Contents
{
    public static class ContentActions
    {
        private static readonly QueryArgument Id = new QueryArgument(AllTypes.None)
        {
            Name = "id",
            Description = "The id of the content (usually GUID).",
            DefaultValue = null,
            ResolvedType = AllTypes.NonNullDomainId
        };

        private static readonly QueryArgument NewId = new QueryArgument(AllTypes.None)
        {
            Name = "id",
            Description = "The optional custom content id.",
            DefaultValue = null,
            ResolvedType = AllTypes.String
        };

        private static readonly QueryArgument ExpectedVersion = new QueryArgument(AllTypes.None)
        {
            Name = "expectedVersion",
            Description = "The expected version",
            DefaultValue = EtagVersion.Any,
            ResolvedType = AllTypes.Int
        };

        private static readonly QueryArgument Publish = new QueryArgument(AllTypes.None)
        {
            Name = "publish",
            Description = "Set to true to autopublish content on create.",
            DefaultValue = false,
            ResolvedType = AllTypes.Boolean
        };

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
                Id,
                new QueryArgument(AllTypes.None)
                {
                    Name = "version",
                    Description = "The optional version of the content to retrieve an older instance (not cached).",
                    DefaultValue = null,
                    ResolvedType = AllTypes.Int
                }
            };

            public static IFieldResolver Resolver(DomainId schemaId)
            {
                var schemaIdValue = schemaId.ToString();

                return Resolvers.Async<object, object?>(async (_, fieldContext, context) =>
                {
                    var contentId = fieldContext.GetArgument<DomainId>("id");

                    var version = fieldContext.GetArgument<int?>("version");

                    if (version >= 0)
                    {
                        return await context.FindContentAsync(schemaIdValue, contentId, version.Value);
                    }
                    else
                    {
                        return await context.FindContentAsync(contentId);
                    }
                });
            }
        }

        public static class QueryOrReferencing
        {
            public static readonly QueryArguments Arguments = new QueryArguments
            {
                new QueryArgument(AllTypes.None)
                {
                    Name = "top",
                    Description = $"Optional number of contents to take.",
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

            public static IFieldResolver Query(DomainId schemaId)
            {
                var schemaIdValue = schemaId.ToString();

                return Resolvers.Async<object, object>(async (_, fieldContext, context) =>
                {
                    var query = fieldContext.BuildODataQuery();

                    return await context.QueryContentsAsync(schemaIdValue, query);
                });
            }

            public static IFieldResolver Referencing(DomainId schemaId)
            {
                var schemaIdValue = schemaId.ToString();

                return Resolvers.Async<IContentEntity, object?>(async (source, fieldContext, context) =>
                {
                    var query = fieldContext.BuildODataQuery();

                    var contentId = source.Id;

                    return await context.QueryReferencingContentsAsync(schemaIdValue, query, source.Id);
                });
            }
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
                    Publish, NewId
                };
            }

            public static IFieldResolver Resolver(NamedId<DomainId> appId, NamedId<DomainId> schemaId)
            {
                return ResolveAsync<IEnrichedContentEntity>(appId, schemaId, c =>
                {
                    var contentPublish = c.GetArgument<bool>("publish");
                    var contentData = GetContentData(c);
                    var contentId = c.GetArgument<string?>("id");

                    var command = new CreateContent { Data = contentData, Publish = contentPublish };

                    if (!string.IsNullOrWhiteSpace(contentId))
                    {
                        var id = DomainId.Create(contentId);

                        command.ContentId = id;
                    }

                    return command;
                });
            }
        }

        public static class Upsert
        {
            public static QueryArguments Arguments(IGraphType inputType)
            {
                return new QueryArguments
                {
                    Id,
                    new QueryArgument(AllTypes.None)
                    {
                        Name = "data",
                        Description = "The data for the content.",
                        DefaultValue = null,
                        ResolvedType = new NonNullGraphType(inputType)
                    },
                    Publish,
                    ExpectedVersion
                };
            }

            public static IFieldResolver Resolver(NamedId<DomainId> appId, NamedId<DomainId> schemaId)
            {
                return ResolveAsync<IEnrichedContentEntity>(appId, schemaId, c =>
                {
                    var contentPublish = c.GetArgument<bool>("publish");
                    var contentData = GetContentData(c);
                    var contentId = c.GetArgument<string>("id");

                    var id = DomainId.Create(contentId);

                    return new UpsertContent { ContentId = id, Data = contentData, Publish = contentPublish };
                });
            }
        }

        public static class Update
        {
            public static QueryArguments Arguments(IGraphType inputType)
            {
                return new QueryArguments
                {
                    Id,
                    new QueryArgument(AllTypes.None)
                    {
                        Name = "data",
                        Description = "The data for the content.",
                        DefaultValue = null,
                        ResolvedType = new NonNullGraphType(inputType)
                    },
                    ExpectedVersion
                };
            }

            public static IFieldResolver Resolver(NamedId<DomainId> appId, NamedId<DomainId> schemaId)
            {
                return ResolveAsync<IEnrichedContentEntity>(appId, schemaId, c =>
                {
                    var contentId = c.GetArgument<DomainId>("id");
                    var contentData = GetContentData(c);

                    return new UpdateContent { ContentId = contentId, Data = contentData };
                });
            }
        }

        public static class Patch
        {
            public static QueryArguments Arguments(IGraphType inputType)
            {
                return new QueryArguments
                {
                    Id,
                    new QueryArgument(AllTypes.None)
                    {
                        Name = "data",
                        Description = "The data for the content.",
                        DefaultValue = null,
                        ResolvedType = new NonNullGraphType(inputType)
                    },
                    ExpectedVersion
                };
            }

            public static IFieldResolver Resolver(NamedId<DomainId> appId, NamedId<DomainId> schemaId)
            {
                return ResolveAsync<IEnrichedContentEntity>(appId, schemaId, c =>
                {
                    var contentId = c.GetArgument<DomainId>("id");
                    var contentData = GetContentData(c);

                    return new PatchContent { ContentId = contentId, Data = contentData };
                });
            }
        }

        public static class ChangeStatus
        {
            public static readonly QueryArguments Arguments = new QueryArguments
            {
                Id,
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
                ExpectedVersion
            };

            public static IFieldResolver Resolver(NamedId<DomainId> appId, NamedId<DomainId> schemaId)
            {
                return ResolveAsync<IEnrichedContentEntity>(appId, schemaId, c =>
                {
                    var contentId = c.GetArgument<DomainId>("id");
                    var contentStatus = new Status(c.GetArgument<string>("status"));
                    var contentDueTime = c.GetArgument<Instant?>("dueTime");

                    return new ChangeContentStatus { ContentId = contentId, Status = contentStatus, DueTime = contentDueTime };
                });
            }
        }

        public static class Delete
        {
            public static readonly QueryArguments Arguments = new QueryArguments
            {
                Id,
                ExpectedVersion
            };

            public static IFieldResolver Resolver(NamedId<DomainId> appId, NamedId<DomainId> schemaId)
            {
                return ResolveAsync<EntitySavedResult>(appId, schemaId, c =>
                {
                    var contentId = c.GetArgument<DomainId>("id");

                    return new DeleteContent { ContentId = contentId };
                });
            }
        }

        private static ContentData GetContentData(IResolveFieldContext c)
        {
            var source = c.GetArgument<IDictionary<string, object>>("data");

            return source.ToContentData((IComplexGraphType)c.FieldDefinition.Arguments.Find("data").Flatten());
        }

        private static IFieldResolver ResolveAsync<T>(NamedId<DomainId> appId, NamedId<DomainId> schemaId, Func<IResolveFieldContext, ContentCommand> action)
        {
            return Resolvers.Async<object, T>(async (source, fieldContext, context) =>
            {
                try
                {
                    var command = action(fieldContext);

                    command.AppId = appId;
                    command.SchemaId = schemaId;
                    command.ExpectedVersion = fieldContext.GetArgument("expectedVersion", EtagVersion.Any);

                    var commandContext = await context.CommandBus.PublishAsync(command);

                    return commandContext.Result<T>();
                }
                catch (ValidationException ex)
                {
                    fieldContext.Errors.Add(new ExecutionError(ex.Message));

                    throw;
                }
                catch (DomainException ex)
                {
                    fieldContext.Errors.Add(new ExecutionError(ex.Message));

                    throw;
                }
            });
        }
    }
}
