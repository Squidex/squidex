// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using GraphQL;
using GraphQL.Resolvers;
using GraphQL.Types;
using Squidex.Domain.Apps.Core.Contents;
using Squidex.Domain.Apps.Core.ConvertContent;
using Squidex.Domain.Apps.Core.Schemas;
using Squidex.Domain.Apps.Entities.Contents.Commands;
using Squidex.Domain.Apps.Entities.Contents.GraphQL.Types.Utils;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Commands;
using Squidex.Infrastructure.Json.Objects;

namespace Squidex.Domain.Apps.Entities.Contents.GraphQL.Types
{
    public static class ContentResolvers
    {
        public static IFieldResolver NestedValue(ValueResolver valueResolver, string key)
        {
            return new FuncFieldResolver<JsonObject, object?>(c =>
            {
                if (c.Source.TryGetValue(key, out var value))
                {
                    return valueResolver(value, c);
                }

                return null;
            });
        }

        public static IFieldResolver Partition(ValueResolver valueResolver, string key)
        {
            return new FuncFieldResolver<ContentFieldData, object?>(c =>
            {
                if (c.Source.TryGetValue(key, out var value) && value != null)
                {
                    return valueResolver(value, c);
                }

                return null;
            });
        }

        public static IFieldResolver FlatPartition(ValueResolver valueResolver, string key)
        {
            return new FuncFieldResolver<FlatContentData, object?>(c =>
            {
                if (c.Source.TryGetValue(key, out var value) && value != null)
                {
                    return valueResolver(value, c);
                }

                return null;
            });
        }

        public static IFieldResolver Field(RootField field)
        {
            var fieldName = field.Name;

            return new FuncFieldResolver<NamedContentData, IReadOnlyDictionary<string, IJsonValue>?>(c =>
            {
                return c.Source?.GetOrDefault(fieldName);
            });
        }

        public static IFieldResolver QueryContents(Guid schemaId)
        {
            var schemaIdValue = schemaId.ToString();

            return ResolveRoot((_, c, context) =>
            {
                return context.QueryContentsAsync(schemaIdValue, c.BuildODataQuery());
            });
        }

        public static readonly IFieldResolver FindContent = ResolveRoot((_, c, context) =>
        {
            var id = c.GetArgument<Guid>("id");

            return context.FindContentAsync(id);
        });

        public static readonly IFieldResolver Url = Resolve((content, _, context) =>
        {
            var appId = content.AppId;

            return context.UrlGenerator.ContentUI(appId, content.SchemaId, content.Id);
        });

        public static readonly IFieldResolver FlatData = Resolve((content, c, context) =>
        {
            var language = context.Context.App.LanguagesConfig.Master;

            return content.Data.ToFlatten(language);
        });

        public static readonly IFieldResolver Data = Resolve(x => x.Data);
        public static readonly IFieldResolver Status = Resolve(x => x.Status.Name.ToUpperInvariant());
        public static readonly IFieldResolver StatusColor = Resolve(x => x.StatusColor);
        public static readonly IFieldResolver ListTotal = ResolveList(x => x.Total);
        public static readonly IFieldResolver ListItems = ResolveList(x => x);

        private static IFieldResolver Resolve<T>(Func<IEnrichedContentEntity, IResolveFieldContext, GraphQLExecutionContext, T> action)
        {
            return new FuncFieldResolver<IEnrichedContentEntity, object?>(c => action(c.Source, c, (GraphQLExecutionContext)c.UserContext));
        }

        private static IFieldResolver Resolve<T>(Func<IEnrichedContentEntity, T> action)
        {
            return new FuncFieldResolver<IEnrichedContentEntity, object?>(c => action(c.Source));
        }

        private static IFieldResolver ResolveList<T>(Func<IResultList<IEnrichedContentEntity>, T> action)
        {
            return new FuncFieldResolver<IResultList<IEnrichedContentEntity>, object?>(c => action(c.Source));
        }

        private static IFieldResolver ResolveRoot<T>(Func<AppQueriesGraphType, IResolveFieldContext, GraphQLExecutionContext, T> action)
        {
            return new FuncFieldResolver<AppQueriesGraphType, object?>(c => action(c.Source, c, (GraphQLExecutionContext)c.UserContext));
        }

        public static IFieldResolver Create(NamedId<Guid> schemaId)
        {
            return ResolveAsync(async (c, publish) =>
            {
                var argPublish = c.GetArgument<bool>("publish");

                var contentData = GetContentData(c);

                var command = new CreateContent { SchemaId = schemaId, Data = contentData, Publish = argPublish };
                var commandContext = await publish(command);

                return commandContext.Result<IEnrichedContentEntity>();
            });
        }

        public static readonly IFieldResolver Update = ResolveAsync(async (c, publish) =>
        {
            var contentId = c.GetArgument<Guid>("id");
            var contentData = GetContentData(c);

            var command = new UpdateContent { ContentId = contentId, Data = contentData };
            var commandContext = await publish(command);

            return commandContext.Result<IEnrichedContentEntity>();
        });

        public static readonly IFieldResolver Patch = ResolveAsync(async (c, publish) =>
        {
            var contentId = c.GetArgument<Guid>("id");
            var contentData = GetContentData(c);

            var command = new PatchContent { ContentId = contentId, Data = contentData };
            var commandContext = await publish(command);

            return commandContext.Result<IEnrichedContentEntity>();
        });

        public static readonly IFieldResolver ChangeStatus = ResolveAsync(async (c, publish) =>
        {
            var contentId = c.GetArgument<Guid>("id");
            var contentStatus = c.GetArgument<string>("status");

            var command = new ChangeContentStatus { ContentId = contentId, Status = new Status(contentStatus) };
            var commandContext = await publish(command);

            return commandContext.Result<IEnrichedContentEntity>();
        });

        public static readonly IFieldResolver Delete = ResolveAsync(async (c, publish) =>
        {
            var contentId = c.GetArgument<Guid>("id");

            var command = new DeleteContent { ContentId = contentId };
            var commandContext = await publish(command);

            return commandContext.Result<EntitySavedResult>();
        });

        private static NamedContentData GetContentData(IResolveFieldContext c)
        {
            var source = c.GetArgument<IDictionary<string, object>>("data");

            return source.ToNamedContentData((IComplexGraphType)c.FieldDefinition.Arguments.Find("data").Flatten());
        }

        private static IFieldResolver ResolveAsync<T>(Func<IResolveFieldContext, Func<SquidexCommand, Task<CommandContext>>, Task<T>> action)
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
    }
}
