// ==========================================================================
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
using Squidex.Domain.Apps.Write.Schemas.Guards;
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

        protected Task On(CreateSchema command, CommandContext context)
        {
            return handler.CreateAsync<SchemaDomainObject>(context, async s =>
            {
                await GuardSchema.CanCreate(command, schemas);

                s.Create(command);

                context.Complete(EntityCreatedResult.Create(s.Id, s.Version));
            });
        }

        protected Task On(AddField command, CommandContext context)
        {
            return handler.UpdateAsync<SchemaDomainObject>(context, s =>
            {
                GuardSchemaField.CanAdd(s.Schema, command);

                s.Add(command);

                context.Complete(EntityCreatedResult.Create(s.Schema.FieldsById.Values.First(x => x.Name == command.Name).Id, s.Version));
            });
        }

        protected Task On(DeleteField command, CommandContext context)
        {
            return handler.UpdateAsync<SchemaDomainObject>(context, s =>
            {
                GuardSchemaField.CanDelete(s.Schema, command);

                s.DeleteField(command);
            });
        }

        protected Task On(LockField command, CommandContext context)
        {
            return handler.UpdateAsync<SchemaDomainObject>(context, s =>
            {
                GuardSchemaField.CanLock(s.Schema, command);

                s.LockField(command);
            });
        }

        protected Task On(HideField command, CommandContext context)
        {
            return handler.UpdateAsync<SchemaDomainObject>(context, s =>
            {
                GuardSchemaField.CanHide(s.Schema, command);

                s.HideField(command);
            });
        }

        protected Task On(ShowField command, CommandContext context)
        {
            return handler.UpdateAsync<SchemaDomainObject>(context, s =>
            {
                GuardSchemaField.CanShow(s.Schema, command);

                s.ShowField(command);
            });
        }

        protected Task On(DisableField command, CommandContext context)
        {
            return handler.UpdateAsync<SchemaDomainObject>(context, s =>
            {
                GuardSchemaField.CanDisable(s.Schema, command);

                s.DisableField(command);
            });
        }

        protected Task On(EnableField command, CommandContext context)
        {
            return handler.UpdateAsync<SchemaDomainObject>(context, s =>
            {
                GuardSchemaField.CanEnable(s.Schema, command);

                s.EnableField(command);
            });
        }

        protected Task On(UpdateField command, CommandContext context)
        {
            return handler.UpdateAsync<SchemaDomainObject>(context, s =>
            {
                GuardSchemaField.CanUpdate(s.Schema, command);

                s.UpdateField(command);
            });
        }

        protected Task On(ReorderFields command, CommandContext context)
        {
            return handler.UpdateAsync<SchemaDomainObject>(context, s =>
            {
                GuardSchema.CanReorder(s.Schema, command);

                s.Reorder(command);
            });
        }

        protected Task On(UpdateSchema command, CommandContext context)
        {
            return handler.UpdateAsync<SchemaDomainObject>(context, s =>
            {
                GuardSchema.CanUpdate(s.Schema, command);

                s.Update(command);
            });
        }

        protected Task On(PublishSchema command, CommandContext context)
        {
            return handler.UpdateAsync<SchemaDomainObject>(context, s =>
            {
                GuardSchema.CanPublish(s.Schema, command);

                s.Publish(command);
            });
        }

        protected Task On(UnpublishSchema command, CommandContext context)
        {
            return handler.UpdateAsync<SchemaDomainObject>(context, s =>
            {
                GuardSchema.CanUnpublish(s.Schema, command);

                s.Unpublish(command);
            });
        }

        protected Task On(ConfigureScripts command, CommandContext context)
        {
            return handler.UpdateAsync<SchemaDomainObject>(context, s =>
            {
                GuardSchema.CanConfigureScripts(s.Schema, command);

                s.ConfigureScripts(command);
            });
        }

        protected Task On(DeleteSchema command, CommandContext context)
        {
            return handler.UpdateAsync<SchemaDomainObject>(context, s =>
            {
                GuardSchema.CanDelete(s.Schema, command);

                s.Delete(command);
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
