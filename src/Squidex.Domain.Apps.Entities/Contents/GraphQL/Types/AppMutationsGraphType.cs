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
using Newtonsoft.Json.Linq;
using Squidex.Domain.Apps.Core.Contents;
using Squidex.Domain.Apps.Entities.Contents.Commands;
using Squidex.Domain.Apps.Entities.Schemas;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Commands;

namespace Squidex.Domain.Apps.Entities.Contents.GraphQL.Types
{
    public sealed class AppMutationsGraphType : ObjectGraphType
    {
        public AppMutationsGraphType(IGraphModel model, IEnumerable<ISchemaEntity> schemas)
        {
            foreach (var schema in schemas)
            {
                var schemaId = schema.NamedId();
                var schemaType = schema.TypeName();
                var schemaName = schema.DisplayName();

                var contentType = model.GetContentType(schema.Id);
                var contentDataType = model.GetContentDataType(schema.Id);

                var resultType = new ContentDataChangedResultGraphType(schemaType, schemaName, contentDataType);

                var inputType = new ContentDataGraphInputType(model, schema);

                AddContentCreate(schemaId, schemaType, schemaName, inputType, contentDataType, contentType);
                AddContentUpdate(schemaType, schemaName, inputType, resultType);
                AddContentPatch(schemaType, schemaName, inputType, resultType);
                AddContentPublish(schemaType, schemaName);
                AddContentUnpublish(schemaType, schemaName);
                AddContentArchive(schemaType, schemaName);
                AddContentRestore(schemaType, schemaName);
                AddContentDelete(schemaType, schemaName);
            }

            Description = "The app mutations.";
        }

        private void AddContentCreate(NamedId<Guid> schemaId, string schemaType, string schemaName, ContentDataGraphInputType inputType, IComplexGraphType contentDataType, IComplexGraphType contentType)
        {
            AddField(new FieldType
            {
                Name = $"create{schemaType}Content",
                Arguments = new QueryArguments
                {
                    new QueryArgument(AllTypes.None)
                    {
                        Name = "data",
                        Description = $"The data for the {schemaName} content.",
                        DefaultValue = null,
                        ResolvedType = new NonNullGraphType(inputType),
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
                        Name = "expectedVersion",
                        Description = "The expected version",
                        DefaultValue = EtagVersion.Any,
                        ResolvedType = AllTypes.Int
                    }
                },
                ResolvedType = new NonNullGraphType(contentType),
                Resolver = ResolveAsync(async (c, publish) =>
                {
                    var argPublish = c.GetArgument<bool>("publish");

                    var contentData = GetContentData(c);

                    var command = new CreateContent { SchemaId = schemaId, Data = contentData, Publish = argPublish };
                    var commandContext = await publish(command);

                    var result = commandContext.Result<EntityCreatedResult<NamedContentData>>();
                    var response = ContentEntity.Create(command, result);

                    return (IContentEntity)ContentEntity.Create(command, result);
                }),
                Description = $"Creates an {schemaName} content."
            });
        }

        private void AddContentUpdate(string schemaType, string schemaName, ContentDataGraphInputType inputType, IComplexGraphType resultType)
        {
            AddField(new FieldType
            {
                Name = $"update{schemaType}Content",
                Arguments = new QueryArguments
                {
                    new QueryArgument(AllTypes.None)
                    {
                        Name = "id",
                        Description = $"The id of the {schemaName} content (GUID)",
                        DefaultValue = string.Empty,
                        ResolvedType = AllTypes.NonNullGuid
                    },
                    new QueryArgument(AllTypes.None)
                    {
                        Name = "data",
                        Description = $"The data for the {schemaName} content.",
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
                },
                ResolvedType = new NonNullGraphType(resultType),
                Resolver = ResolveAsync(async (c, publish) =>
                {
                    var contentId = c.GetArgument<Guid>("id");
                    var contentData = GetContentData(c);

                    var command = new UpdateContent { ContentId = contentId, Data = contentData };
                    var commandContext = await publish(command);

                    var result = commandContext.Result<ContentDataChangedResult>();

                    return result;
                }),
                Description = $"Update an {schemaName} content by id."
            });
        }

        private void AddContentPatch(string schemaType, string schemaName, ContentDataGraphInputType inputType, IComplexGraphType resultType)
        {
            AddField(new FieldType
            {
                Name = $"patch{schemaType}Content",
                Arguments = new QueryArguments
                {
                    new QueryArgument(AllTypes.None)
                    {
                        Name = "id",
                        Description = $"The id of the {schemaName} content (GUID)",
                        DefaultValue = string.Empty,
                        ResolvedType = AllTypes.NonNullGuid
                    },
                    new QueryArgument(AllTypes.None)
                    {
                        Name = "data",
                        Description = $"The data for the {schemaName} content.",
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
                },
                ResolvedType = new NonNullGraphType(resultType),
                Resolver = ResolveAsync(async (c, publish) =>
                {
                    var contentId = c.GetArgument<Guid>("id");
                    var contentData = GetContentData(c);

                    var command = new PatchContent { ContentId = contentId, Data = contentData };
                    var commandContext = await publish(command);

                    var result = commandContext.Result<ContentDataChangedResult>();

                    return result;
                }),
                Description = $"Patch a {schemaName} content."
            });
        }

        private void AddContentPublish(string schemaType, string schemaName)
        {
            AddField(new FieldType
            {
                Name = $"publish{schemaType}Content",
                Arguments = CreateIdArguments(schemaName),
                ResolvedType = AllTypes.CommandVersion,
                Resolver = ResolveAsync((c, publish) =>
                {
                    var contentId = c.GetArgument<Guid>("id");

                    var command = new ChangeContentStatus { ContentId = contentId, Status = Status.Published };

                    return publish(command);
                }),
                Description = $"Publish a {schemaName} content."
            });
        }

        private void AddContentUnpublish(string schemaType, string schemaName)
        {
            AddField(new FieldType
            {
                Name = $"unpublish{schemaType}Content",
                Arguments = CreateIdArguments(schemaName),
                ResolvedType = AllTypes.CommandVersion,
                Resolver = ResolveAsync((c, publish) =>
                {
                    var contentId = c.GetArgument<Guid>("id");

                    var command = new ChangeContentStatus { ContentId = contentId, Status = Status.Draft };

                    return publish(command);
                }),
                Description = $"Unpublish a {schemaName} content."
            });
        }

        private void AddContentArchive(string schemaType, string schemaName)
        {
            AddField(new FieldType
            {
                Name = $"archive{schemaType}Content",
                Arguments = CreateIdArguments(schemaName),
                ResolvedType = AllTypes.CommandVersion,
                Resolver = ResolveAsync((c, publish) =>
                {
                    var contentId = c.GetArgument<Guid>("id");

                    var command = new ChangeContentStatus { ContentId = contentId, Status = Status.Archived };

                    return publish(command);
                }),
                Description = $"Archive a {schemaName} content."
            });
        }

        private void AddContentRestore(string schemaType, string schemaName)
        {
            AddField(new FieldType
            {
                Name = $"restore{schemaType}Content",
                Arguments = CreateIdArguments(schemaName),
                ResolvedType = AllTypes.CommandVersion,
                Resolver = ResolveAsync((c, publish) =>
                {
                    var contentId = c.GetArgument<Guid>("id");

                    var command = new ChangeContentStatus { ContentId = contentId, Status = Status.Draft };

                    return publish(command);
                }),
                Description = $"Restore a {schemaName} content."
            });
        }

        private void AddContentDelete(string schemaType, string schemaName)
        {
            AddField(new FieldType
            {
                Name = $"delete{schemaType}Content",
                Arguments = CreateIdArguments(schemaName),
                ResolvedType = AllTypes.CommandVersion,
                Resolver = ResolveAsync((c, publish) =>
                {
                    var contentId = c.GetArgument<Guid>("id");

                    var command = new DeleteContent { ContentId = contentId };

                    return publish(command);
                }),
                Description = $"Delete an {schemaName} content."
            });
        }

        private static QueryArguments CreateIdArguments(string schemaName)
        {
            return new QueryArguments
            {
                new QueryArgument(AllTypes.None)
                {
                    Name = "id",
                    Description = $"The id of the {schemaName} content (GUID)",
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
        }

        private static IFieldResolver ResolveAsync<T>(Func<ResolveFieldContext, Func<SquidexCommand, Task<CommandContext>>, Task<T>> action)
        {
            return new FuncFieldResolver<Task<T>>(async c =>
            {
                var e = (GraphQLExecutionContext)c.UserContext;

                try
                {
                    return await action(c, command =>
                    {
                        command.ExpectedVersion = c.GetArgument("expectedVersion", EtagVersion.Any);

                        return e.CommandBus.PublishAsync(command);
                    });
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

        private static NamedContentData GetContentData(ResolveFieldContext c)
        {
            return JObject.FromObject(c.GetArgument<object>("data")).ToObject<NamedContentData>();
        }
    }
}
