// ==========================================================================
//  WebhookCommandMiddleware.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System;
using System.Linq;
using System.Threading.Tasks;
using Squidex.Domain.Apps.Read.Schemas.Services;
using Squidex.Domain.Apps.Write.Webhooks.Commands;
using Squidex.Infrastructure;
using Squidex.Infrastructure.CQRS.Commands;
using Squidex.Infrastructure.Dispatching;

namespace Squidex.Domain.Apps.Write.Webhooks
{
    public class WebhookCommandMiddleware : ICommandMiddleware
    {
        private readonly IAggregateHandler handler;
        private readonly ISchemaProvider schemas;

        public WebhookCommandMiddleware(IAggregateHandler handler, ISchemaProvider schemas)
        {
            Guard.NotNull(handler, nameof(handler));
            Guard.NotNull(schemas, nameof(schemas));

            this.handler = handler;
            this.schemas = schemas;
        }

        protected async Task On(CreateWebhook command, CommandContext context)
        {
            await ValidateAsync(command, () => "Failed to create webhook");

            await handler.CreateAsync<WebhookDomainObject>(context, c => c.Create(command));
        }

        protected async Task On(UpdateWebhook command, CommandContext context)
        {
            await ValidateAsync(command, () => "Failed to update content");

            await handler.UpdateAsync<WebhookDomainObject>(context, c => c.Update(command));
        }

        protected Task On(DeleteWebhook command, CommandContext context)
        {
            return handler.UpdateAsync<WebhookDomainObject>(context, c => c.Delete(command));
        }

        public async Task HandleAsync(CommandContext context, Func<Task> next)
        {
            if (!await this.DispatchActionAsync(context.Command, context))
            {
                await next();
            }
        }

        private async Task ValidateAsync(WebhookEditCommand command, Func<string> message)
        {
            var results = await Task.WhenAll(
                command.Schemas.Select(async schema =>
                    await schemas.FindSchemaByIdAsync(schema.SchemaId) == null
                        ? new ValidationError($"Schema {schema.SchemaId} does not exist.")
                        : null));

            var errors = results.Where(x => x != null).ToArray();

            if (errors.Length > 0)
            {
                throw new ValidationException(message(), errors);
            }
        }
    }
}
