// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Linq;
using System.Threading.Tasks;
using Squidex.Domain.Apps.Entities.Schemas.Commands;
using Squidex.Domain.Apps.Entities.Schemas.Guards;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Commands;
using Squidex.Infrastructure.Dispatching;

namespace Squidex.Domain.Apps.Entities.Schemas
{
    public class SchemaCommandMiddleware : ICommandMiddleware
    {
        private readonly IAppProvider appProvider;
        private readonly IAggregateHandler handler;

        public SchemaCommandMiddleware(IAggregateHandler handler, IAppProvider appProvider)
        {
            Guard.NotNull(handler, nameof(handler));
            Guard.NotNull(appProvider, nameof(appProvider));

            this.handler = handler;

            this.appProvider = appProvider;
        }

        protected Task On(CreateSchema command, CommandContext context)
        {
            return handler.CreateSyncedAsync<SchemaDomainObject>(context, async s =>
            {
                await GuardSchema.CanCreate(command, appProvider);

                s.Create(command);

                context.Complete(EntityCreatedResult.Create(command.SchemaId, s.Version));
            });
        }

        protected Task On(AddField command, CommandContext context)
        {
            return handler.UpdateSyncedAsync<SchemaDomainObject>(context, s =>
            {
                GuardSchemaField.CanAdd(s.Snapshot.SchemaDef, command);

                s.Add(command);

                context.Complete(EntityCreatedResult.Create(s.Snapshot.SchemaDef.FieldsById.Values.First(x => x.Name == command.Name).Id, s.Version));
            });
        }

        protected Task On(DeleteField command, CommandContext context)
        {
            return handler.UpdateSyncedAsync<SchemaDomainObject>(context, s =>
            {
                GuardSchemaField.CanDelete(s.Snapshot.SchemaDef, command);

                s.DeleteField(command);
            });
        }

        protected Task On(LockField command, CommandContext context)
        {
            return handler.UpdateSyncedAsync<SchemaDomainObject>(context, s =>
            {
                GuardSchemaField.CanLock(s.Snapshot.SchemaDef, command);

                s.LockField(command);
            });
        }

        protected Task On(HideField command, CommandContext context)
        {
            return handler.UpdateSyncedAsync<SchemaDomainObject>(context, s =>
            {
                GuardSchemaField.CanHide(s.Snapshot.SchemaDef, command);

                s.HideField(command);
            });
        }

        protected Task On(ShowField command, CommandContext context)
        {
            return handler.UpdateSyncedAsync<SchemaDomainObject>(context, s =>
            {
                GuardSchemaField.CanShow(s.Snapshot.SchemaDef, command);

                s.ShowField(command);
            });
        }

        protected Task On(DisableField command, CommandContext context)
        {
            return handler.UpdateSyncedAsync<SchemaDomainObject>(context, s =>
            {
                GuardSchemaField.CanDisable(s.Snapshot.SchemaDef, command);

                s.DisableField(command);
            });
        }

        protected Task On(EnableField command, CommandContext context)
        {
            return handler.UpdateSyncedAsync<SchemaDomainObject>(context, s =>
            {
                GuardSchemaField.CanEnable(s.Snapshot.SchemaDef, command);

                s.EnableField(command);
            });
        }

        protected Task On(UpdateField command, CommandContext context)
        {
            return handler.UpdateSyncedAsync<SchemaDomainObject>(context, s =>
            {
                GuardSchemaField.CanUpdate(s.Snapshot.SchemaDef, command);

                s.UpdateField(command);
            });
        }

        protected Task On(ReorderFields command, CommandContext context)
        {
            return handler.UpdateSyncedAsync<SchemaDomainObject>(context, s =>
            {
                GuardSchema.CanReorder(s.Snapshot.SchemaDef, command);

                s.Reorder(command);
            });
        }

        protected Task On(UpdateSchema command, CommandContext context)
        {
            return handler.UpdateSyncedAsync<SchemaDomainObject>(context, s =>
            {
                GuardSchema.CanUpdate(s.Snapshot.SchemaDef, command);

                s.Update(command);
            });
        }

        protected Task On(PublishSchema command, CommandContext context)
        {
            return handler.UpdateSyncedAsync<SchemaDomainObject>(context, s =>
            {
                GuardSchema.CanPublish(s.Snapshot.SchemaDef, command);

                s.Publish(command);
            });
        }

        protected Task On(UnpublishSchema command, CommandContext context)
        {
            return handler.UpdateSyncedAsync<SchemaDomainObject>(context, s =>
            {
                GuardSchema.CanUnpublish(s.Snapshot.SchemaDef, command);

                s.Unpublish(command);
            });
        }

        protected Task On(ConfigureScripts command, CommandContext context)
        {
            return handler.UpdateSyncedAsync<SchemaDomainObject>(context, s =>
            {
                GuardSchema.CanConfigureScripts(s.Snapshot.SchemaDef, command);

                s.ConfigureScripts(command);
            });
        }

        protected Task On(DeleteSchema command, CommandContext context)
        {
            return handler.UpdateSyncedAsync<SchemaDomainObject>(context, s =>
            {
                GuardSchema.CanDelete(s.Snapshot.SchemaDef, command);

                s.Delete(command);
            });
        }

        public async Task HandleAsync(CommandContext context, Func<Task> next)
        {
            await this.DispatchActionAsync(context.Command, context);
            await next();
        }
    }
}
