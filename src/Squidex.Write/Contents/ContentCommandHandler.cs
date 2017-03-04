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
using Squidex.Infrastructure;
using Squidex.Infrastructure.CQRS.Commands;
using Squidex.Infrastructure.Dispatching;
using Squidex.Infrastructure.Tasks;
using Squidex.Read.Apps.Services;
using Squidex.Read.Schemas.Services;
using Squidex.Write.Contents.Commands;

namespace Squidex.Write.Contents
{
    public class ContentCommandHandler : ICommandHandler
    {
        private readonly IAggregateHandler handler;
        private readonly IAppProvider appProvider;
        private readonly ISchemaProvider schemas;

        public ContentCommandHandler(
            IAggregateHandler handler,
            IAppProvider appProvider, 
            ISchemaProvider schemas)
        {
            Guard.NotNull(handler, nameof(handler));
            Guard.NotNull(schemas, nameof(schemas));
            Guard.NotNull(appProvider, nameof(appProvider));

            this.handler = handler;
            this.schemas = schemas;

            this.appProvider = appProvider;
        }

        protected async Task On(CreateContent command, CommandContext context)
        {
            await ValidateAsync(command, () => "Failed to create content");

            await handler.CreateAsync<ContentDomainObject>(context, c => c.Create(command));
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

        private async Task ValidateAsync(ContentDataCommand command, Func<string> message)
        {
            Guard.Valid(command, nameof(command), message);

            var taskForApp = 
                appProvider.FindAppByIdAsync(command.AppId.Id);

            var taskForSchema = 
                schemas.FindSchemaByIdAsync(command.SchemaId.Id);

            await Task.WhenAll(taskForApp, taskForSchema);

            var languages = new HashSet<Language>(taskForApp.Result.Languages);

            var schemaObject = taskForSchema.Result.Schema;
            var schemaErrors = new List<ValidationError>();

            await schemaObject.ValidateAsync(command.Data, schemaErrors, languages);

            schemaObject.Enrich(command.Data, languages);

            if (schemaErrors.Count > 0)
            {
                throw new ValidationException(message(), schemaErrors);
            }
        }
    }
}
