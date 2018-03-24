// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Squidex.Domain.Apps.Core.Schemas;
using Squidex.Domain.Apps.Entities.Schemas.Commands;
using Squidex.Domain.Apps.Entities.Schemas.Guards;
using Squidex.Domain.Apps.Entities.Schemas.State;
using Squidex.Domain.Apps.Events;
using Squidex.Domain.Apps.Events.Schemas;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Commands;
using Squidex.Infrastructure.EventSourcing;
using Squidex.Infrastructure.Orleans;
using Squidex.Infrastructure.Reflection;
using Squidex.Infrastructure.States;

namespace Squidex.Domain.Apps.Entities.Schemas
{
    public class SchemaGrain : SquidexDomainObjectGrain<SchemaState>, ISchemaGrain
    {
        private readonly IAppProvider appProvider;
        private readonly FieldRegistry registry;

        public SchemaGrain(IStore<Guid> store, IAppProvider appProvider, FieldRegistry registry)
            : base(store)
        {
            Guard.NotNull(appProvider, nameof(appProvider));
            Guard.NotNull(registry, nameof(registry));

            this.appProvider = appProvider;

            this.registry = registry;
        }

        protected override Task<object> ExecuteAsync(IAggregateCommand command)
        {
            VerifyNotDeleted();

            switch (command)
            {
                case CreateSchema createSchema:
                    return CreateAsync(createSchema, async c =>
                    {
                        await GuardSchema.CanCreate(c, appProvider);

                        Create(c);
                    });

                case AddField addField:
                    return UpdateReturnAsync(addField, c =>
                    {
                        GuardSchemaField.CanAdd(Snapshot.SchemaDef, c);

                        Add(c);

                        return EntityCreatedResult.Create(Snapshot.SchemaDef.FieldsById.Values.First(x => x.Name == addField.Name).Id, NewVersion);
                    });

                case DeleteField deleteField:
                    return UpdateAsync(deleteField, c =>
                    {
                        GuardSchemaField.CanDelete(Snapshot.SchemaDef, deleteField);

                        DeleteField(c);
                    });

                case LockField lockField:
                    return UpdateAsync(lockField, c =>
                    {
                        GuardSchemaField.CanLock(Snapshot.SchemaDef, lockField);

                        LockField(c);
                    });

                case HideField hideField:
                    return UpdateAsync(hideField, c =>
                    {
                        GuardSchemaField.CanHide(Snapshot.SchemaDef, c);

                        HideField(c);
                    });

                case ShowField showField:
                    return UpdateAsync(showField, c =>
                    {
                        GuardSchemaField.CanShow(Snapshot.SchemaDef, c);

                        ShowField(c);
                    });

                case DisableField disableField:
                    return UpdateAsync(disableField, c =>
                    {
                        GuardSchemaField.CanDisable(Snapshot.SchemaDef, c);

                        DisableField(c);
                    });

                case EnableField enableField:
                    return UpdateAsync(enableField, c =>
                    {
                        GuardSchemaField.CanEnable(Snapshot.SchemaDef, c);

                        EnableField(c);
                    });

                case UpdateField updateField:
                    return UpdateAsync(updateField, c =>
                    {
                        GuardSchemaField.CanUpdate(Snapshot.SchemaDef, c);

                        UpdateField(c);
                    });

                case ReorderFields reorderFields:
                    return UpdateAsync(reorderFields, c =>
                    {
                        GuardSchema.CanReorder(Snapshot.SchemaDef, c);

                        Reorder(c);
                    });

                case UpdateSchema updateSchema:
                    return UpdateAsync(updateSchema, c =>
                    {
                        GuardSchema.CanUpdate(Snapshot.SchemaDef, c);

                        Update(c);
                    });

                case PublishSchema publishSchema:
                    return UpdateAsync(publishSchema, c =>
                    {
                        GuardSchema.CanPublish(Snapshot.SchemaDef, c);

                        Publish(c);
                    });

                case UnpublishSchema unpublishSchema:
                    return UpdateAsync(unpublishSchema, c =>
                    {
                        GuardSchema.CanUnpublish(Snapshot.SchemaDef, c);

                        Unpublish(c);
                    });

                case ConfigureScripts configureScripts:
                    return UpdateAsync(configureScripts, c =>
                    {
                        GuardSchema.CanConfigureScripts(Snapshot.SchemaDef, c);

                        ConfigureScripts(c);
                    });

                case DeleteSchema deleteSchema:
                    return UpdateAsync(deleteSchema, c =>
                    {
                        GuardSchema.CanDelete(Snapshot.SchemaDef, c);

                        Delete(c);
                    });

                default:
                    throw new NotSupportedException();
            }
        }

        public void Create(CreateSchema command)
        {
            var @event = SimpleMapper.Map(command, new SchemaCreated { SchemaId = new NamedId<Guid>(command.SchemaId, command.Name) });

            if (command.Fields != null)
            {
                @event.Fields = new List<SchemaCreatedField>();

                foreach (var commandField in command.Fields)
                {
                    var eventField = SimpleMapper.Map(commandField, new SchemaCreatedField());

                    @event.Fields.Add(eventField);
                }
            }

            RaiseEvent(@event);
        }

        public void Add(AddField command)
        {
            RaiseEvent(SimpleMapper.Map(command, new FieldAdded { FieldId = new NamedId<long>(Snapshot.TotalFields + 1, command.Name) }));
        }

        public void UpdateField(UpdateField command)
        {
            RaiseEvent(command, SimpleMapper.Map(command, new FieldUpdated()));
        }

        public void LockField(LockField command)
        {
            RaiseEvent(command, new FieldLocked());
        }

        public void HideField(HideField command)
        {
            RaiseEvent(command, new FieldHidden());
        }

        public void ShowField(ShowField command)
        {
            RaiseEvent(command, new FieldShown());
        }

        public void DisableField(DisableField command)
        {
            RaiseEvent(command, new FieldDisabled());
        }

        public void EnableField(EnableField command)
        {
            RaiseEvent(command, new FieldEnabled());
        }

        public void DeleteField(DeleteField command)
        {
            RaiseEvent(command, new FieldDeleted());
        }

        public void Reorder(ReorderFields command)
        {
            RaiseEvent(SimpleMapper.Map(command, new SchemaFieldsReordered()));
        }

        public void Publish(PublishSchema command)
        {
            RaiseEvent(SimpleMapper.Map(command, new SchemaPublished()));
        }

        public void Unpublish(UnpublishSchema command)
        {
            RaiseEvent(SimpleMapper.Map(command, new SchemaUnpublished()));
        }

        public void ConfigureScripts(ConfigureScripts command)
        {
            RaiseEvent(SimpleMapper.Map(command, new ScriptsConfigured()));
        }

        public void Delete(DeleteSchema command)
        {
            RaiseEvent(SimpleMapper.Map(command, new SchemaDeleted()));
        }

        public void Update(UpdateSchema command)
        {
            RaiseEvent(SimpleMapper.Map(command, new SchemaUpdated()));
        }

        private void RaiseEvent(FieldCommand fieldCommand, FieldEvent @event)
        {
            SimpleMapper.Map(fieldCommand, @event);

            if (Snapshot.SchemaDef.FieldsById.TryGetValue(fieldCommand.FieldId, out var field))
            {
                @event.FieldId = new NamedId<long>(field.Id, field.Name);
            }

            RaiseEvent(@event);
        }

        private void RaiseEvent(SchemaEvent @event)
        {
            if (@event.SchemaId == null)
            {
                @event.SchemaId = new NamedId<Guid>(Snapshot.Id, Snapshot.Name);
            }

            if (@event.AppId == null)
            {
                @event.AppId = Snapshot.AppId;
            }

            RaiseEvent(Envelope.Create(@event));
        }

        private void VerifyNotDeleted()
        {
            if (Snapshot.IsDeleted)
            {
                throw new DomainException("Schema has already been deleted.");
            }
        }

        public override void ApplyEvent(Envelope<IEvent> @event)
        {
            ApplySnapshot(Snapshot.Apply(@event, registry));
        }

        public Task<J<ISchemaEntity>> GetStateAsync()
        {
            return Task.FromResult(new J<ISchemaEntity>(Snapshot));
        }
    }
}