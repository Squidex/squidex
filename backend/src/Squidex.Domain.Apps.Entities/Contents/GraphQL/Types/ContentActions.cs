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
using Squidex.Domain.Apps.Entities.Contents.GraphQL.Types.Utils;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Commands;
using Squidex.Infrastructure.Json.Objects;
using Squidex.Infrastructure.Validation;

namespace Squidex.Domain.Apps.Entities.Contents.GraphQL.Types
{
    public static class ContentActions
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

            public static readonly ValueResolver Resolver = (value, c) =>
            {
                if (c.Arguments.TryGetValue("path", out var p) && p is string path)
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
                }
            };

            public static readonly IFieldResolver Resolver = new FuncFieldResolver<object?>(c =>
            {
                var contentId = c.GetArgument<DomainId>("id");

                return ((GraphQLExecutionContext)c.UserContext).FindContentAsync(contentId);
            });
        }

        public static class QueryOrReferencing
        {
            private static QueryArguments? arguments;

            public static QueryArguments Arguments(int pageSize)
            {
                return arguments ??= new QueryArguments
                {
                    new QueryArgument(AllTypes.None)
                    {
                        Name = "top",
                        Description = $"Optional number of contents to take (Default: {pageSize}).",
                        DefaultValue = pageSize,
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
            }

            public static IFieldResolver Query(DomainId schemaId)
            {
                var schemaIdValue = schemaId.ToString();

                return new FuncFieldResolver<object?>(c =>
                {
                    var query = c.BuildODataQuery();

                    return ((GraphQLExecutionContext)c.UserContext).QueryContentsAsync(schemaIdValue, query);
                });
            }

            public static IFieldResolver Referencing(DomainId schemaId)
            {
                var schemaIdValue = schemaId.ToString();

                return new FuncFieldResolver<IContentEntity, object?>(c =>
                {
                    var query = c.BuildODataQuery();

                    var contentId = c.Source.Id;

                    return ((GraphQLExecutionContext)c.UserContext).QueryReferencingContentsAsync(schemaIdValue, query, c.Source.Id);
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
                    new QueryArgument(AllTypes.None)
                    {
                        Name = "publish",
                        Description = "Set to true to autopublish content.",
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
                    new QueryArgument(AllTypes.None)
                    {
                        Name = "id",
                        Description = "The id of the content (usually GUID)",
                        DefaultValue = null,
                        ResolvedType = AllTypes.NonNullString
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
                    new QueryArgument(AllTypes.None)
                    {
                        Name = "id",
                        Description = "The id of the content (usually GUID)",
                        DefaultValue = string.Empty,
                        ResolvedType = AllTypes.NonNullString
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
                    new QueryArgument(AllTypes.None)
                    {
                        Name = "id",
                        Description = "The id of the content (usually GUID)",
                        DefaultValue = string.Empty,
                        ResolvedType = AllTypes.NonNullString
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
                new QueryArgument(AllTypes.None)
                {
                    Name = "id",
                    Description = "The id of the content (usually GUID)",
                    DefaultValue = null,
                    ResolvedType = AllTypes.NonNullString
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
                new QueryArgument(AllTypes.None)
                {
                    Name = "id",
                    Description = "The id of the content (usually GUID)",
                    DefaultValue = null,
                    ResolvedType = AllTypes.NonNullString
                },
                new QueryArgument(AllTypes.None)
                {
                    Name = "expectedVersion",
                    Description = "The expected version",
                    DefaultValue = EtagVersion.Any,
                    ResolvedType = AllTypes.Int
                }
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

        private static NamedContentData GetContentData(IResolveFieldContext c)
        {
            var source = c.GetArgument<IDictionary<string, object>>("data");

            return source.ToNamedContentData((IComplexGraphType)c.FieldDefinition.Arguments.Find("data").Flatten());
        }

        private static IFieldResolver ResolveAsync<T>(NamedId<DomainId> appId, NamedId<DomainId> schemaId, Func<IResolveFieldContext, ContentCommand> action)
        {
            return new FuncFieldResolver<Task<T>>(async c =>
            {
                var e = (GraphQLExecutionContext)c.UserContext;

                try
                {
                    var command = action(c);

                    command.AppId = appId;
                    command.SchemaId = schemaId;
                    command.ExpectedVersion = c.GetArgument("expectedVersion", EtagVersion.Any);

                    var commandContext = await e.CommandBus.PublishAsync(command);

                    return commandContext.Result<T>();
                }
                catch (ValidationException ex)
                {
                    c.Errors.Add(new ExecutionError(ex.Message));

                    throw;
                }
                catch (DomainException ex)
                {
                    c.Errors.Add(new ExecutionError(ex.Message));

                    throw;
                }
            });
        }
    }
}
