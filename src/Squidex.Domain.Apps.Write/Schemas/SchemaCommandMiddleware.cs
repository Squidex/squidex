﻿// ==========================================================================
//  SchemaCommandMiddleware.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System;
using System.Linq;
using System.Threading.Tasks;
using Squidex.Domain.Apps.Read.Schemas.Services;
using Squidex.Domain.Apps.Write.Schemas.Commands;
using Squidex.Infrastructure;
using Squidex.Infrastructure.CQRS.Commands;
using Squidex.Infrastructure.Dispatching;

namespace Squidex.Domain.Apps.Write.Schemas
{
    public class SchemaCommandMiddleware : ICommandMiddleware
    {
        private readonly ISchemaProvider schemas;
        private readonly IAggregateHandler handler;

        public SchemaCommandMiddleware(IAggregateHandler handler, ISchemaProvider schemas)
        {
            Guard.NotNull(handler, nameof(handler));
            Guard.NotNull(schemas, nameof(schemas));

            this.handler = handler;
            this.schemas = schemas;
        }

        protected async Task On(CreateSchema command, CommandContext context)
        {
            if (await schemas.FindSchemaByNameAsync(command.AppId.Id, command.Name) != null)
            {
                var error =
                    new ValidationError($"A schema with name '{command.Name}' already exists", "Name",
                        nameof(CreateSchema.Name));

                throw new ValidationException("Cannot create a new schema.", error);
            }

            await handler.CreateAsync<SchemaDomainObject>(context, s =>
            {
                s.Create(command);

                context.Complete(EntityCreatedResult.Create(s.Id, s.Version));
            });
        }

        protected Task On(AddField command, CommandContext context)
        {
            return handler.UpdateAsync<SchemaDomainObject>(context, s =>
            {
                s.Add(command);

                context.Complete(EntityCreatedResult.Create(s.Schema.FieldsById.Values.First(x => x.Name == command.Name).Id, s.Version));
            });
        }

        protected Task On(DeleteSchema command, CommandContext context)
        {
            return handler.UpdateAsync<SchemaDomainObject>(context, s => s.Delete(command));
        }

        protected Task On(DeleteField command, CommandContext context)
        {
            return handler.UpdateAsync<SchemaDomainObject>(context, s => s.DeleteField(command));
        }

        protected Task On(LockField command, CommandContext context)
        {
            return handler.UpdateAsync<SchemaDomainObject>(context, s => s.LockField(command));
        }

        protected Task On(HideField command, CommandContext context)
        {
            return handler.UpdateAsync<SchemaDomainObject>(context, s => s.HideField(command));
        }

        protected Task On(ShowField command, CommandContext context)
        {
            return handler.UpdateAsync<SchemaDomainObject>(context, s => s.ShowField(command));
        }

        protected Task On(DisableField command, CommandContext context)
        {
            return handler.UpdateAsync<SchemaDomainObject>(context, s => s.DisableField(command));
        }

        protected Task On(EnableField command, CommandContext context)
        {
            return handler.UpdateAsync<SchemaDomainObject>(context, s => s.EnableField(command));
        }

        protected Task On(ReorderFields command, CommandContext context)
        {
            return handler.UpdateAsync<SchemaDomainObject>(context, s => s.Reorder(command));
        }

        protected Task On(UpdateSchema command, CommandContext context)
        {
            return handler.UpdateAsync<SchemaDomainObject>(context, s => s.Update(command));
        }

        protected Task On(UpdateField command, CommandContext context)
        {
            return handler.UpdateAsync<SchemaDomainObject>(context, s => s.UpdateField(command));
        }

        protected Task On(PublishSchema command, CommandContext context)
        {
            return handler.UpdateAsync<SchemaDomainObject>(context, s => s.Publish(command));
        }

        protected Task On(UnpublishSchema command, CommandContext context)
        {
            return handler.UpdateAsync<SchemaDomainObject>(context, s => s.Unpublish(command));
        }

        protected Task On(ConfigureScripts command, CommandContext context)
        {
            return handler.UpdateAsync<SchemaDomainObject>(context, s => s.ConfigureScripts(command));
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
