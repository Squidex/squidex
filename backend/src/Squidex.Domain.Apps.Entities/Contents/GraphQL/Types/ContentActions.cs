﻿// ==========================================================================
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

            public static readonly ValueResolver Resolver = new ValueResolver((value, c) =>
            {
                if (c.Arguments.TryGetValue("path", out var p) && p is string path)
                {
                    value.TryGetByPath(path, out var result);

                    return result!;
                }

                return value;
            });
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
                    Description = "The id of the content (GUID).",
                    DefaultValue = string.Empty,
                    ResolvedType = AllTypes.NonNullGuid
                }
            };

            public static readonly IFieldResolver Resolver = new FuncFieldResolver<object?>(c =>
            {
                var id = c.GetArgument<Guid>("id");

                return ((GraphQLExecutionContext)c.UserContext).FindContentAsync(id);
            });
        }

        public static class Query
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
                        DefaultValue = string.Empty,
                        ResolvedType = AllTypes.String
                    },
                    new QueryArgument(AllTypes.None)
                    {
                        Name = "orderby",
                        Description = "Optional OData order definition.",
                        DefaultValue = string.Empty,
                        ResolvedType = AllTypes.String
                    },
                    new QueryArgument(AllTypes.None)
                    {
                        Name = "search",
                        Description = "Optional OData full text search.",
                        DefaultValue = string.Empty,
                        ResolvedType = AllTypes.String
                    },
                };
            }

            public static IFieldResolver Resolver(Guid schemaId)
            {
                var schemaIdValue = schemaId.ToString();

                return new FuncFieldResolver<object?>(c =>
                {
                    var query = c.BuildODataQuery();

                    return ((GraphQLExecutionContext)c.UserContext).QueryContentsAsync(schemaIdValue, query);
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
                        ResolvedType = new NonNullGraphType(inputType),
                    },
                    new QueryArgument(AllTypes.None)
                    {
                        Name = "publish",
                        Description = "Set to true to autopublish content.",
                        DefaultValue = false,
                        ResolvedType = AllTypes.Boolean
                    }
                };
            }

            public static IFieldResolver Resolver(NamedId<Guid> schemaId)
            {
                return ResolveAsync<IEnrichedContentEntity>(c =>
                {
                    var contentPublish = c.GetArgument<bool>("publish");
                    var contentData = GetContentData(c);

                    return new CreateContent { SchemaId = schemaId, Data = contentData, Publish = contentPublish };
                });
            }
        }

        public static class UpdateOrPatch
        {
            public static QueryArguments Arguments(IGraphType inputType)
            {
                return new QueryArguments
                {
                    new QueryArgument(AllTypes.None)
                    {
                        Name = "id",
                        Description = "The id of the content (GUID)",
                        DefaultValue = string.Empty,
                        ResolvedType = AllTypes.NonNullGuid
                    },
                    new QueryArgument(AllTypes.None)
                    {
                        Name = "data",
                        Description = "The data for the content.",
                        DefaultValue = null,
                        ResolvedType = new NonNullGraphType(inputType),
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

            public static readonly IFieldResolver Update = ResolveAsync<IEnrichedContentEntity>(c =>
            {
                var contentId = c.GetArgument<Guid>("id");
                var contentData = GetContentData(c);

                return new UpdateContent { ContentId = contentId, Data = contentData };
            });

            public static readonly IFieldResolver Patch = ResolveAsync<IEnrichedContentEntity>(c =>
            {
                var contentId = c.GetArgument<Guid>("id");
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
                    Description = "The id of the content (GUID)",
                    DefaultValue = string.Empty,
                    ResolvedType = AllTypes.NonNullGuid
                },
                new QueryArgument(AllTypes.None)
                {
                    Name = "status",
                    Description = "The new status",
                    DefaultValue = string.Empty,
                    ResolvedType = AllTypes.NonNullString
                },
                new QueryArgument(AllTypes.None)
                {
                    Name = "dueTime",
                    Description = "When to change the status",
                    DefaultValue = EtagVersion.Any,
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

            public static readonly IFieldResolver Resolver = ResolveAsync<IEnrichedContentEntity>(c =>
            {
                var contentId = c.GetArgument<Guid>("id");
                var contentStatus = new Status(c.GetArgument<string>("status"));
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
                    Description = "The id of the content (GUID)",
                    DefaultValue = string.Empty,
                    ResolvedType = AllTypes.NonNullGuid
                },
                new QueryArgument(AllTypes.None)
                {
                    Name = "expectedVersion",
                    Description = "The expected version",
                    DefaultValue = EtagVersion.Any,
                    ResolvedType = AllTypes.Int
                }
            };

            public static readonly IFieldResolver Resolver = ResolveAsync<EntitySavedResult>(c =>
            {
                var contentId = c.GetArgument<Guid>("id");

                return new DeleteContent { ContentId = contentId };
            });
        }

        private static NamedContentData GetContentData(IResolveFieldContext c)
        {
            var source = c.GetArgument<IDictionary<string, object>>("data");

            return source.ToNamedContentData((IComplexGraphType)c.FieldDefinition.Arguments.Find("data").Flatten());
        }

        private static IFieldResolver ResolveAsync<T>(Func<IResolveFieldContext, SquidexCommand> action)
        {
            return new FuncFieldResolver<Task<T>>(async c =>
            {
                var e = (GraphQLExecutionContext)c.UserContext;

                try
                {
                    var command = action(c);

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
