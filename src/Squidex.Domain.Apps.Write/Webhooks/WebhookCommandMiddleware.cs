// ==========================================================================
//  WebhookCommandMiddleware.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System;
using System.Threading.Tasks;
using Squidex.Domain.Apps.Read.Schemas.Services;
using Squidex.Domain.Apps.Write.Webhooks.Commands;
using Squidex.Domain.Apps.Write.Webhooks.Guards;
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
            await handler.CreateAsync<WebhookDomainObject>(context, async w =>
            {
                await GuardWebhook.CanCreate(command, schemas);

                w.Create(command);
            });
        }

        protected async Task On(UpdateWebhook command, CommandContext context)
        {
            await handler.UpdateAsync<WebhookDomainObject>(context, async c =>
            {
                await GuardWebhook.CanUpdate(command, schemas);

                c.Update(command);
            });
        }

        protected Task On(DeleteWebhook command, CommandContext context)
        {
            return handler.UpdateAsync<WebhookDomainObject>(context, c =>
            {
                GuardWebhook.CanDelete(command);

                c.Delete(command);
            });
        }

        public async Task HandleAsync(CommandContext context, Func<Task> next)
        {
            if (!await this.DispatchActionAsync(context.Command, context))
            {
                await next();
            }
        }
    }
}
