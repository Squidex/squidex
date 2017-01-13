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
using Squidex.Read.Apps.Services;
using Squidex.Read.Schemas.Services;
using Squidex.Write.Contents.Commands;

namespace Squidex.Write.Contents
{
    public class ContentCommandHandler : ICommandHandler
    {
        private readonly IAggregateHandler handler;
        private readonly IAppProvider appProvider;
        private readonly ISchemaProvider schemaProvider;

        public ContentCommandHandler(
            IAggregateHandler handler,
            IAppProvider appProvider, 
            ISchemaProvider schemaProvider)
        {
            Guard.NotNull(handler, nameof(handler));
            Guard.NotNull(appProvider, nameof(appProvider));
            Guard.NotNull(schemaProvider, nameof(schemaProvider));

            this.handler = handler;
            this.appProvider = appProvider;
            this.schemaProvider = schemaProvider;
        }

        protected async Task On(CreateContent command, CommandContext context)
        {
            await ValidateAsync(command, () => "Failed to create content");

            await handler.CreateAsync<ContentDomainObject>(command, s =>
            {
                s.Create(command);

                context.Succeed(command.AggregateId);
            });
        }

        protected async Task On(UpdateContent command, CommandContext context)
        {
            await ValidateAsync(command, () => "Failed to update content");

            await handler.UpdateAsync<ContentDomainObject>(command, s => s.Update(command));
        }

        protected Task On(DeleteContent command, CommandContext context)
        {
            return handler.UpdateAsync<ContentDomainObject>(command, s => s.Delete(command));
        }

        public Task<bool> HandleAsync(CommandContext context)
        {
            return context.IsHandled ? Task.FromResult(false) : this.DispatchActionAsync(context.Command, context);
        }

        private async Task ValidateAsync(ContentDataCommand command, Func<string> message)
        {
            Guard.Valid(command, nameof(command), message);

            var taskForApp = appProvider.FindAppByIdAsync(command.AppId);
            var taskForSchema = schemaProvider.FindSchemaByIdAsync(command.SchemaId);

            await Task.WhenAll(taskForApp, taskForSchema);

            var errors = new List<ValidationError>();

            await taskForSchema.Result.Schema.ValidateAsync(command.Data, errors, new HashSet<Language>(taskForApp.Result.Languages));

            if (errors.Count > 0)
            {
                throw new ValidationException(message(), errors);
            }
        }
    }
}
