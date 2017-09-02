// ==========================================================================
//  ContentCommandMiddleware.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Squidex.Domain.Apps.Core;
using Squidex.Domain.Apps.Core.Contents;
using Squidex.Domain.Apps.Core.Schemas;
using Squidex.Domain.Apps.Core.Scripting;
using Squidex.Domain.Apps.Read.Apps;
using Squidex.Domain.Apps.Read.Apps.Services;
using Squidex.Domain.Apps.Read.Assets.Repositories;
using Squidex.Domain.Apps.Read.Contents.Repositories;
using Squidex.Domain.Apps.Read.Schemas;
using Squidex.Domain.Apps.Read.Schemas.Services;
using Squidex.Domain.Apps.Write.Contents.Commands;
using Squidex.Infrastructure;
using Squidex.Infrastructure.CQRS.Commands;
using Squidex.Infrastructure.Dispatching;

// ReSharper disable ConvertToLambdaExpression

namespace Squidex.Domain.Apps.Write.Contents
{
    public class ContentCommandMiddleware : ICommandMiddleware
    {
        private readonly IAggregateHandler handler;
        private readonly IAppProvider appProvider;
        private readonly IAssetRepository assetRepository;
        private readonly IContentRepository contentRepository;
        private readonly ISchemaProvider schemas;
        private readonly IScriptEngine scriptEngine;

        public ContentCommandMiddleware(
            IAggregateHandler handler,
            IAppProvider appProvider,
            IAssetRepository assetRepository,
            ISchemaProvider schemas,
            IScriptEngine scriptEngine,
            IContentRepository contentRepository)
        {
            Guard.NotNull(handler, nameof(handler));
            Guard.NotNull(schemas, nameof(schemas));
            Guard.NotNull(appProvider, nameof(appProvider));
            Guard.NotNull(scriptEngine, nameof(scriptEngine));
            Guard.NotNull(assetRepository, nameof(assetRepository));
            Guard.NotNull(contentRepository, nameof(contentRepository));

            this.handler = handler;
            this.schemas = schemas;
            this.appProvider = appProvider;
            this.scriptEngine = scriptEngine;
            this.assetRepository = assetRepository;
            this.contentRepository = contentRepository;
        }

        protected async Task On(CreateContent command, CommandContext context)
        {
            await handler.CreateAsync<ContentDomainObject>(context, async content =>
            {
                var schemaAndApp = await ResolveSchemaAndAppAsync(command);
                var scriptContext = CreateScriptContext(content, command, command.Data);

                command.Data = scriptEngine.ExecuteAndTransform(scriptContext, schemaAndApp.SchemaEntity.ScriptCreate, "create content", true);
                command.Data.Enrich(schemaAndApp.SchemaEntity.Schema, schemaAndApp.AppEntity.PartitionResolver);

                await ValidateAsync(schemaAndApp, command, () => "Failed to create content", false);

                content.Create(command);

                context.Complete(EntityCreatedResult.Create(command.Data, content.Version));
            });
        }

        protected async Task On(UpdateContent command, CommandContext context)
        {
            await handler.UpdateAsync<ContentDomainObject>(context, async content =>
            {
                var schemaAndApp = await ResolveSchemaAndAppAsync(command);
                var scriptContext = CreateScriptContext(content, command, command.Data);

                command.Data = scriptEngine.ExecuteAndTransform(scriptContext, schemaAndApp.SchemaEntity.ScriptUpdate, "update content", true);

                await ValidateAsync(schemaAndApp, command, () => "Failed to update content", false);

                content.Update(command);

                context.Complete(new ContentDataChangedResult(content.Data, content.Version));
            });
        }

        protected async Task On(PatchContent command, CommandContext context)
        {
            await handler.UpdateAsync<ContentDomainObject>(context, async content =>
            {
                var schemaAndApp = await ResolveSchemaAndAppAsync(command);
                var scriptContext = CreateScriptContext(content, command, command.Data);

                command.Data = scriptEngine.ExecuteAndTransform(scriptContext, schemaAndApp.SchemaEntity.ScriptUpdate, "patch content", true);

                await ValidateAsync(schemaAndApp, command, () => "Failed to patch content", true);

                content.Patch(command);

                context.Complete(new ContentDataChangedResult(content.Data, content.Version));
            });
        }

        protected Task On(PublishContent command, CommandContext context)
        {
            return handler.UpdateAsync<ContentDomainObject>(context, async content =>
            {
                var schemaAndApp = await ResolveSchemaAndAppAsync(command);
                var scriptContext = CreateScriptContext(content, command);

                scriptEngine.Execute(scriptContext, schemaAndApp.SchemaEntity.ScriptPublish, "publish content");

                content.Publish(command);
            });
        }

        protected Task On(UnpublishContent command, CommandContext context)
        {
            return handler.UpdateAsync<ContentDomainObject>(context, async content =>
            {
                var schemaAndApp = await ResolveSchemaAndAppAsync(command);
                var scriptContext = CreateScriptContext(content, command);

                scriptEngine.Execute(scriptContext, schemaAndApp.SchemaEntity.ScriptUnpublish, "unpublish content");

                content.Unpublish(command);
            });
        }

        protected Task On(DeleteContent command, CommandContext context)
        {
            return handler.UpdateAsync<ContentDomainObject>(context, async content =>
            {
                var schemaAndApp = await ResolveSchemaAndAppAsync(command);
                var scriptContext = CreateScriptContext(content, command);

                scriptEngine.Execute(scriptContext, schemaAndApp.SchemaEntity.ScriptDelete, "delete content");

                content.Delete(command);
            });
        }

        public async Task HandleAsync(CommandContext context, Func<Task> next)
        {
            if (!await this.DispatchActionAsync(context.Command, context))
            {
                await next();
            }
        }

        private async Task ValidateAsync((ISchemaEntity Schema, IAppEntity App) schemaAndApp, ContentDataCommand command, Func<string> message, bool partial)
        {
            Guard.Valid(command, nameof(command), message);

            var schemaErrors = new List<ValidationError>();

            var appId = command.AppId.Id;

            var validationContext =
                new ValidationContext(
                    (contentIds, schemaId) =>
                    {
                        return contentRepository.QueryNotFoundAsync(appId, schemaId, contentIds.ToList());
                    },
                    assetIds =>
                    {
                        return assetRepository.QueryNotFoundAsync(appId, assetIds.ToList());
                    });

            if (partial)
            {
                await command.Data.ValidatePartialAsync(validationContext, schemaAndApp.Schema.Schema, schemaAndApp.App.PartitionResolver, schemaErrors);
            }
            else
            {
                await command.Data.ValidateAsync(validationContext, schemaAndApp.Schema.Schema, schemaAndApp.App.PartitionResolver, schemaErrors);
            }

            if (schemaErrors.Count > 0)
            {
                throw new ValidationException(message(), schemaErrors);
            }
        }

        private static ScriptContext CreateScriptContext(ContentDomainObject content, ContentCommand command, NamedContentData data = null)
        {
            return new ScriptContext { ContentId = content.Id, Data = data, OldData = content.Data, User = ScriptUser.Create(command.Principal) };
        }

        private async Task<(ISchemaEntity SchemaEntity, IAppEntity AppEntity)> ResolveSchemaAndAppAsync(SchemaCommand command)
        {
            var taskForApp = appProvider.FindAppByIdAsync(command.AppId.Id);
            var taskForSchema = schemas.FindSchemaByIdAsync(command.SchemaId.Id);

            await Task.WhenAll(taskForApp, taskForSchema);

            return (taskForSchema.Result, taskForApp.Result);
        }
    }
}
