// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
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
                AddContentUpdate(schemaId, schemaType, schemaName, inputType, resultType);
                AddContentPatch(schemaId, schemaType, schemaName, inputType, resultType);
                AddContentPublish(schemaId, schemaType, schemaName);
                AddContentUnpublish(schemaId, schemaType, schemaName);
                AddContentArchive(schemaId, schemaType, schemaName);
                AddContentRestore(schemaId, schemaType, schemaName);
                AddContentDelete(schemaId, schemaType, schemaName);
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
                    new QueryArgument(typeof(BooleanGraphType))
                    {
                        Name = "publish",
                        Description = "Set to true to autopublish content.",
                        DefaultValue = false
                    },
                    new QueryArgument(typeof(NoopGraphType))
                    {
                        Name = "data",
                        Description = $"The data for the {schemaName} content.",
                        DefaultValue = null,
                        ResolvedType = new NonNullGraphType(inputType),
                    },
                    new QueryArgument(typeof(IntGraphType))
                    {
                        Name = "expectedVersion",
                        Description = "The expected version",
                        DefaultValue = EtagVersion.Any
                    }
                },
                ResolvedType = new NonNullGraphType(contentType),
                Resolver = ResolveAsync(async (c, publish) =>
                {
                    var argPublish = c.GetArgument<bool>("publish");

                    var contentData = GetContentData(c);

                    var command = new CreateContent { SchemaId = schemaId, ContentId = Guid.NewGuid(), Data = contentData, Publish = argPublish };
                    var commandContext = await publish(command);

                    var result = commandContext.Result<EntityCreatedResult<NamedContentData>>();
                    var response = ContentEntity.Create(command, result);

                    return (IContentEntity)ContentEntity.Create(command, result);
                }),
                Description = $"Creates an {schemaName} content."
            });
        }

        private void AddContentUpdate(NamedId<Guid> schemaId, string schemaType, string schemaName, ContentDataGraphInputType inputType, IComplexGraphType resultType)
        {
            AddField(new FieldType
            {
                Name = $"update{schemaType}Content",
                Arguments = new QueryArguments
                {
                    new QueryArgument(typeof(NonNullGraphType<GuidGraphType>))
                    {
                        Name = "id",
                        Description = $"The id of the {schemaName} content (GUID)",
                        DefaultValue = string.Empty
                    },
                    new QueryArgument(typeof(NoopGraphType))
                    {
                        Name = "data",
                        Description = $"The data for the {schemaName} content.",
                        DefaultValue = null,
                        ResolvedType = new NonNullGraphType(inputType),
                    },
                    new QueryArgument(typeof(IntGraphType))
                    {
                        Name = "expectedVersion",
                        Description = "The expected version",
                        DefaultValue = EtagVersion.Any
                    }
                },
                ResolvedType = new NonNullGraphType(resultType),
                Resolver = ResolveAsync(async (c, publish) =>
                {
                    var contentId = c.GetArgument<Guid>("id");
                    var contentData = GetContentData(c);

                    var command = new UpdateContent { SchemaId = schemaId, ContentId = contentId, Data = contentData };
                    var commandContext = await publish(command);

                    var result = commandContext.Result<ContentDataChangedResult>();

                    return result;
                }),
                Description = $"Update an {schemaName} content by id."
            });
        }

        private void AddContentPatch(NamedId<Guid> schemaId, string schemaType, string schemaName, ContentDataGraphInputType inputType, IComplexGraphType resultType)
        {
            AddField(new FieldType
            {
                Name = $"patch{schemaType}Content",
                Arguments = new QueryArguments
                {
                    new QueryArgument(typeof(NonNullGraphType<GuidGraphType>))
                    {
                        Name = "id",
                        Description = $"The id of the {schemaName} content (GUID)",
                        DefaultValue = string.Empty
                    },
                    new QueryArgument(typeof(NoopGraphType))
                    {
                        Name = "data",
                        Description = $"The data for the {schemaName} content.",
                        DefaultValue = null,
                        ResolvedType = new NonNullGraphType(inputType),
                    },
                    new QueryArgument(typeof(IntGraphType))
                    {
                        Name = "expectedVersion",
                        Description = "The expected version",
                        DefaultValue = EtagVersion.Any
                    }
                },
                ResolvedType = new NonNullGraphType(resultType),
                Resolver = ResolveAsync(async (c, publish) =>
                {
                    var contentId = c.GetArgument<Guid>("id");
                    var contentData = GetContentData(c);

                    var command = new PatchContent { SchemaId = schemaId, ContentId = contentId, Data = contentData };
                    var commandContext = await publish(command);

                    var result = commandContext.Result<ContentDataChangedResult>();

                    return result;
                }),
                Description = $"Patch a {schemaName} content."
            });
        }

        private void AddContentPublish(NamedId<Guid> schemaId, string schemaType, string schemaName)
        {
            AddField(new FieldType
            {
                Name = $"publish{schemaType}Content",
                Arguments = CreateIdArguments(schemaName),
                ResolvedType = new NonNullGraphType(new CommandVersionGraphType()),
                Resolver = ResolveAsync((c, publish) =>
                {
                    var contentId = c.GetArgument<Guid>("id");

                    var command = new ChangeContentStatus { SchemaId = schemaId, ContentId = contentId, Status = Status.Published };

                    return publish(command);
                }),
                Description = $"Publish a {schemaName} content."
            });
        }

        private void AddContentUnpublish(NamedId<Guid> schemaId, string schemaType, string schemaName)
        {
            AddField(new FieldType
            {
                Name = $"unpublish{schemaType}Content",
                Arguments = CreateIdArguments(schemaName),
                ResolvedType = new NonNullGraphType(new CommandVersionGraphType()),
                Resolver = ResolveAsync((c, publish) =>
                {
                    var contentId = c.GetArgument<Guid>("id");

                    var command = new ChangeContentStatus { SchemaId = schemaId, ContentId = contentId, Status = Status.Draft };

                    return publish(command);
                }),
                Description = $"Unpublish a {schemaName} content."
            });
        }

        private void AddContentArchive(NamedId<Guid> schemaId, string schemaType, string schemaName)
        {
            AddField(new FieldType
            {
                Name = $"archive{schemaType}Content",
                Arguments = CreateIdArguments(schemaName),
                ResolvedType = new NonNullGraphType(new CommandVersionGraphType()),
                Resolver = ResolveAsync((c, publish) =>
                {
                    var contentId = c.GetArgument<Guid>("id");

                    var command = new ChangeContentStatus { SchemaId = schemaId, ContentId = contentId, Status = Status.Archived };

                    return publish(command);
                }),
                Description = $"Archive a {schemaName} content."
            });
        }

        private void AddContentRestore(NamedId<Guid> schemaId, string schemaType, string schemaName)
        {
            AddField(new FieldType
            {
                Name = $"restore{schemaType}Content",
                Arguments = CreateIdArguments(schemaName),
                ResolvedType = new NonNullGraphType(new CommandVersionGraphType()),
                Resolver = ResolveAsync((c, publish) =>
                {
                    var contentId = c.GetArgument<Guid>("id");

                    var command = new ChangeContentStatus { SchemaId = schemaId, ContentId = contentId, Status = Status.Draft };

                    return publish(command);
                }),
                Description = $"Restore a {schemaName} content."
            });
        }

        private void AddContentDelete(NamedId<Guid> schemaId, string schemaType, string schemaName)
        {
            AddField(new FieldType
            {
                Name = $"delete{schemaType}Content",
                Arguments = CreateIdArguments(schemaName),
                ResolvedType = new NonNullGraphType(new CommandVersionGraphType()),
                Resolver = ResolveAsync((c, publish) =>
                {
                    var contentId = c.GetArgument<Guid>("id");

                    var command = new DeleteContent { SchemaId = schemaId, ContentId = contentId };

                    return publish(command);
                }),
                Description = $"Delete an {schemaName} content."
            });
        }

        private static QueryArguments CreateIdArguments(string schemaName)
        {
            return new QueryArguments
            {
                new QueryArgument(typeof(GuidGraphType))
                {
                    Name = "id",
                    Description = $"The id of the {schemaName} content (GUID)",
                    DefaultValue = string.Empty
                },
                new QueryArgument(typeof(IntGraphType))
                {
                    Name = "expectedVersion",
                    Description = "The expected version",
                    DefaultValue = EtagVersion.Any
                }
            };
        }

        private static IFieldResolver ResolveAsync<T>(Func<ResolveFieldContext, Func<SquidexCommand, Task<CommandContext>>, Task<T>> action)
        {
            return new FuncFieldResolver<Task<T>>(c =>
            {
                var e = (GraphQLExecutionContext)c.UserContext;

                return action(c, command =>
                {
                    command.ExpectedVersion = c.GetArgument("expectedVersion", EtagVersion.Any);

                    return e.CommandBus.PublishAsync(command);
                });
            });
        }

        private static NamedContentData GetContentData(ResolveFieldContext c)
        {
            return JObject.FromObject(c.GetArgument<object>("data")).ToObject<NamedContentData>();
        }
    }
}
