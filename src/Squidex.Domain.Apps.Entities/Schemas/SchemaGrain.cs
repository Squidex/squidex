// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Collections.Generic;
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
using Squidex.Infrastructure.Log;
using Squidex.Infrastructure.Orleans;
using Squidex.Infrastructure.Reflection;
using Squidex.Infrastructure.States;

namespace Squidex.Domain.Apps.Entities.Schemas
{
    public sealed class SchemaGrain : SquidexDomainObjectGrain<SchemaState>, ISchemaGrain
    {
        private readonly IAppProvider appProvider;
        private readonly FieldRegistry registry;

        public SchemaGrain(IStore<Guid> store, ISemanticLog log, IAppProvider appProvider, FieldRegistry registry)
            : base(store, log)
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
                case AddField addField:
                    return UpdateAsync(addField, c =>
                    {
                        GuardSchemaField.CanAdd(Snapshot.SchemaDef, c);

                        Add(c);

                        long id;

                        if (c.ParentFieldId == null)
                        {
                            id = Snapshot.SchemaDef.FieldsByName[c.Name].Id;
                        }
                        else
                        {
                            id = ((IArrayField)Snapshot.SchemaDef.FieldsById[c.ParentFieldId.Value]).FieldsByName[c.Name].Id;
                        }

                        return EntityCreatedResult.Create(id, Version);
                    });

                case CreateSchema createSchema:
                    return CreateAsync(createSchema, async c =>
                    {
                        await GuardSchema.CanCreate(c, appProvider);

                        Create(c);
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

                case ChangeCategory changeCategory:
                    return UpdateAsync(changeCategory, c =>
                    {
                        GuardSchema.CanChangeCategory(Snapshot.SchemaDef, c);

                        ChangeCategory(c);
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
            var @event = SimpleMapper.Map(command, new SchemaCreated { SchemaId = NamedId.Of(command.SchemaId, command.Name) });

            if (command.Fields != null)
            {
                @event.Fields = new List<SchemaCreatedField>();

                foreach (var commandField in command.Fields)
                {
                    var eventField = SimpleMapper.Map(commandField, new SchemaCreatedField());

                    @event.Fields.Add(eventField);

                    if (commandField.Nested != null)
                    {
                        eventField.Nested = new List<SchemaCreatedNestedField>();

                        foreach (var nestedField in commandField.Nested)
                        {
                            var eventNestedField = SimpleMapper.Map(nestedField, new SchemaCreatedNestedField());

                            eventField.Nested.Add(eventNestedField);
                        }
                    }
                }
            }

            RaiseEvent(@event);
        }

        public void Add(AddField command)
        {
            RaiseEvent(SimpleMapper.Map(command, new FieldAdded { ParentFieldId = GetFieldId(command.ParentFieldId), FieldId = CreateFieldId(command) }));
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
            RaiseEvent(SimpleMapper.Map(command, new SchemaFieldsReordered { ParentFieldId = GetFieldId(command.ParentFieldId) }));
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

        public void ChangeCategory(ChangeCategory command)
        {
            RaiseEvent(SimpleMapper.Map(command, new SchemaCategoryChanged()));
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

            if (fieldCommand.ParentFieldId.HasValue)
            {
                if (Snapshot.SchemaDef.FieldsById.TryGetValue(fieldCommand.ParentFieldId.Value, out var field))
                {
                    @event.ParentFieldId = NamedId.Of(field.Id, field.Name);

                    if (field is IArrayField arrayField && arrayField.FieldsById.TryGetValue(fieldCommand.FieldId, out var nestedField))
                    {
                        @event.FieldId = NamedId.Of(nestedField.Id, nestedField.Name);
                    }
                }
            }
            else
            {
                @event.FieldId = GetFieldId(fieldCommand.FieldId);
            }

            RaiseEvent(@event);
        }

        private NamedId<long> CreateFieldId(AddField command)
        {
            return NamedId.Of(Snapshot.TotalFields + 1L, command.Name);
        }

        private NamedId<long> GetFieldId(long? id)
        {
            if (id.HasValue && Snapshot.SchemaDef.FieldsById.TryGetValue(id.Value, out var field))
            {
                return NamedId.Of(field.Id, field.Name);
            }

            return null;
        }

        private void RaiseEvent(SchemaEvent @event)
        {
            if (@event.SchemaId == null)
            {
                @event.SchemaId = NamedId.Of(Snapshot.Id, Snapshot.Name);
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

        protected override SchemaState OnEvent(Envelope<IEvent> @event)
        {
            return Snapshot.Apply(@event, registry);
        }

        public Task<J<ISchemaEntity>> GetStateAsync()
        {
            return J.AsTask<ISchemaEntity>(Snapshot);
        }
    }
}