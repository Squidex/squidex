// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Threading.Tasks;
using Squidex.Domain.Apps.Core.Scripting;
using Squidex.Domain.Apps.Entities.Assets.Repositories;
using Squidex.Domain.Apps.Entities.Contents.Commands;
using Squidex.Domain.Apps.Entities.Contents.Guards;
using Squidex.Domain.Apps.Entities.Contents.Repositories;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Commands;
using Squidex.Infrastructure.Dispatching;

namespace Squidex.Domain.Apps.Entities.Contents
{
    public class ContentCommandMiddleware : ICommandMiddleware
    {
        private readonly IAggregateHandler handler;
        private readonly IAppProvider appProvider;
        private readonly IAssetRepository assetRepository;
        private readonly IContentRepository contentRepository;
        private readonly IScriptEngine scriptEngine;

        public ContentCommandMiddleware(
            IAggregateHandler handler,
            IAppProvider appProvider,
            IAssetRepository assetRepository,
            IScriptEngine scriptEngine,
            IContentRepository contentRepository)
        {
            Guard.NotNull(handler, nameof(handler));
            Guard.NotNull(appProvider, nameof(appProvider));
            Guard.NotNull(scriptEngine, nameof(scriptEngine));
            Guard.NotNull(assetRepository, nameof(assetRepository));
            Guard.NotNull(contentRepository, nameof(contentRepository));

            this.handler = handler;
            this.appProvider = appProvider;
            this.scriptEngine = scriptEngine;
            this.assetRepository = assetRepository;
            this.contentRepository = contentRepository;
        }

        protected async Task On(CreateContent command, CommandContext context)
        {
            await handler.CreateAsync<ContentDomainObject>(context, async content =>
            {
                GuardContent.CanCreate(command);

                var operationContext = await CreateContext(command, content, () => "Failed to create content.");

                if (command.Publish)
                {
                    await operationContext.ExecuteScriptAsync(x => x.ScriptChange, "Published");
                }

                await operationContext.ExecuteScriptAndTransformAsync(x => x.ScriptCreate, "Create");
                await operationContext.EnrichAsync();
                await operationContext.ValidateAsync(false);

                content.Create(command);

                context.Complete(EntityCreatedResult.Create(command.Data, content.Version));
            });
        }

        protected async Task On(UpdateContent command, CommandContext context)
        {
            await handler.UpdateAsync<ContentDomainObject>(context, async content =>
            {
                GuardContent.CanUpdate(command);

                var operationContext = await CreateContext(command, content, () => "Failed to update content.");

                await operationContext.ValidateAsync(true);
                await operationContext.ExecuteScriptAndTransformAsync(x => x.ScriptUpdate, "Update");

                content.Update(command);

                context.Complete(new ContentDataChangedResult(content.Snapshot.Data, content.Version));
            });
        }

        protected async Task On(PatchContent command, CommandContext context)
        {
            await handler.UpdateAsync<ContentDomainObject>(context, async content =>
            {
                GuardContent.CanPatch(command);

                var operationContext = await CreateContext(command, content, () => "Failed to patch content.");

                await operationContext.ValidateAsync(true);
                await operationContext.ExecuteScriptAndTransformAsync(x => x.ScriptUpdate, "Patch");

                content.Patch(command);

                context.Complete(new ContentDataChangedResult(content.Snapshot.Data, content.Version));
            });
        }

        protected Task On(ChangeContentStatus command, CommandContext context)
        {
            return handler.UpdateAsync<ContentDomainObject>(context, async content =>
            {
                GuardContent.CanChangeContentStatus(content.Snapshot.Status, command);

                var operationContext = await CreateContext(command, content, () => "Failed to patch content.");

                await operationContext.ExecuteScriptAsync(x => x.ScriptChange, command.Status);

                content.ChangeStatus(command);
            });
        }

        protected Task On(DeleteContent command, CommandContext context)
        {
            return handler.UpdateAsync<ContentDomainObject>(context, async content =>
            {
                GuardContent.CanDelete(command);

                var operationContext = await CreateContext(command, content, () => "Failed to delete content.");

                await operationContext.ExecuteScriptAsync(x => x.ScriptDelete, "Delete");

                content.Delete(command);
            });
        }

        protected Task On(PublishContentAt command, CommandContext context)
        {
            return handler.UpdateAsync<ContentDomainObject>(context, content =>
            {
                GuardContent.CanPublishAt(command);

                content.PublishAt(command);
            });
        }

        public async Task HandleAsync(CommandContext context, Func<Task> next)
        {
            await this.DispatchActionAsync(context.Command, context);
            await next();
        }

        private async Task<ContentOperationContext> CreateContext(ContentCommand command, ContentDomainObject content, Func<string> message)
        {
            var operationContext =
                await ContentOperationContext.CreateAsync(
                    contentRepository,
                    content,
                    command,
                    appProvider,
                    assetRepository,
                    scriptEngine,
                    message);

            return operationContext;
        }
    }
}
