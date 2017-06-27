// ==========================================================================
//  ContentCommandHandler.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Squidex.Core;
using Squidex.Core.Schemas;
using Squidex.Infrastructure;
using Squidex.Infrastructure.CQRS.Commands;
using Squidex.Infrastructure.Dispatching;
using Squidex.Infrastructure.Tasks;
using Squidex.Read.Apps.Services;
using Squidex.Read.Assets.Repositories;
using Squidex.Read.Contents.Repositories;
using Squidex.Read.Schemas.Services;
using Squidex.Write.Contents.Commands;

namespace Squidex.Write.Contents
{
    public class ContentCommandHandler : ICommandHandler
    {
        private readonly IAggregateHandler handler;
        private readonly IAppProvider appProvider;
        private readonly IAssetRepository assetRepository;
        private readonly IContentRepository contentRepository;
        private readonly ISchemaProvider schemas;

        public ContentCommandHandler(
            IAggregateHandler handler,
            IAppProvider appProvider, 
            IAssetRepository assetRepository, 
            ISchemaProvider schemas, 
            IContentRepository contentRepository)
        {
            Guard.NotNull(handler, nameof(handler));
            Guard.NotNull(schemas, nameof(schemas));
            Guard.NotNull(handler, nameof(handler));
            Guard.NotNull(appProvider, nameof(appProvider));
            Guard.NotNull(assetRepository, nameof(assetRepository));
            Guard.NotNull(contentRepository, nameof(contentRepository));

            this.handler = handler;
            this.schemas = schemas;
            this.appProvider = appProvider;
            this.assetRepository = assetRepository;
            this.contentRepository = contentRepository;
        }

        protected async Task On(CreateContent command, CommandContext context)
        {
            await ValidateAsync(command, () => "Failed to create content", true);

            await handler.CreateAsync<ContentDomainObject>(context, c =>
            {
                c.Create(command);

                context.Succeed(EntityCreatedResult.Create(command.Data, c.Version));
            });
        }

        protected async Task On(UpdateContent command, CommandContext context)
        {
            await ValidateAsync(command, () => "Failed to update content");

            await handler.UpdateAsync<ContentDomainObject>(context, c => c.Update(command));
        }

        protected async Task On(PatchContent command, CommandContext context)
        {
            await ValidateAsync(command, () => "Failed to patch content");

            await handler.UpdateAsync<ContentDomainObject>(context, c => c.Patch(command));
        }

        protected Task On(PublishContent command, CommandContext context)
        {
            return handler.UpdateAsync<ContentDomainObject>(context, c => c.Publish(command));
        }

        protected Task On(UnpublishContent command, CommandContext context)
        {
            return handler.UpdateAsync<ContentDomainObject>(context, c => c.Unpublish(command));
        }

        protected Task On(DeleteContent command, CommandContext context)
        {
            return handler.UpdateAsync<ContentDomainObject>(context, c => c.Delete(command));
        }

        public Task<bool> HandleAsync(CommandContext context)
        {
            return context.IsHandled ? TaskHelper.False : this.DispatchActionAsync(context.Command, context);
        }

        private async Task ValidateAsync(ContentDataCommand command, Func<string> message, bool enrich = false)
        {
            Guard.Valid(command, nameof(command), message);

            var taskForApp = appProvider.FindAppByIdAsync(command.AppId.Id);
            var taskForSchema = schemas.FindSchemaByIdAsync(command.SchemaId.Id);

            await Task.WhenAll(taskForApp, taskForSchema);
            
            var schemaObject = taskForSchema.Result.Schema;
            var schemaErrors = new List<ValidationError>();

            var appId = command.AppId.Id;

            var validationContext = new ValidationContext((x, y) => contentRepository.ExistsAsync(appId, x, y), x => assetRepository.ExistsAsync(appId, x));

            await command.Data.ValidateAsync(validationContext, schemaObject, taskForApp.Result.PartitionResolver, schemaErrors);

            if (schemaErrors.Count > 0)
            {
                throw new ValidationException(message(), schemaErrors);
            }

            if (enrich)
            {
                command.Data.Enrich(schemaObject, taskForApp.Result.PartitionResolver);
            }
        }
    }
}
