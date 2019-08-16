// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Threading.Tasks;
using Squidex.Domain.Apps.Core.EventSynchronization;
using Squidex.Domain.Apps.Core.Schemas;
using Squidex.Domain.Apps.Entities.Schemas.Commands;
using Squidex.Domain.Apps.Entities.Schemas.Guards;
using Squidex.Domain.Apps.Entities.Schemas.State;
using Squidex.Domain.Apps.Events;
using Squidex.Domain.Apps.Events.Schemas;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Commands;
using Squidex.Infrastructure.EventSourcing;
using Squidex.Infrastructure.Json;
using Squidex.Infrastructure.Log;
using Squidex.Infrastructure.Orleans;
using Squidex.Infrastructure.Reflection;
using Squidex.Infrastructure.States;

namespace Squidex.Domain.Apps.Entities.Schemas
{
    public sealed class SchemaGrain : SquidexDomainObjectGrain<SchemaState>, ISchemaGrain
    {
        private readonly IAppProvider appProvider;
        private readonly IJsonSerializer serializer;

        public SchemaGrain(IStore<Guid> store, ISemanticLog log, IAppProvider appProvider, IJsonSerializer serializer)
            : base(store, log)
        {
            Guard.NotNull(appProvider, nameof(appProvider));
            Guard.NotNull(serializer, nameof(serializer));

            this.appProvider = appProvider;

            this.serializer = serializer;
        }

        protected override Task<object> ExecuteAsync(IAggregateCommand command)
        {
            VerifyNotDeleted();

            switch (command)
            {
                case AddField addField:
                    return UpdateReturn(addField, c =>
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

                        return Snapshot;
                    });

                case CreateSchema createSchema:
                    return CreateReturnAsync(createSchema, async c =>
                    {
                        await GuardSchema.CanCreate(c, appProvider);

                        Create(c);

                        return Snapshot;
                    });

                case SynchronizeSchema synchronizeSchema:
                    return UpdateReturn(synchronizeSchema, c =>
                    {
                        GuardSchema.CanSynchronize(c);

                        Synchronize(c);

                        return Snapshot;
                    });

                case DeleteField deleteField:
                    return UpdateReturn(deleteField, c =>
                    {
                        GuardSchemaField.CanDelete(Snapshot.SchemaDef, deleteField);

                        DeleteField(c);

                        return Snapshot;
                    });

                case LockField lockField:
                    return UpdateReturn(lockField, c =>
                    {
                        GuardSchemaField.CanLock(Snapshot.SchemaDef, lockField);

                        LockField(c);

                        return Snapshot;
                    });

                case HideField hideField:
                    return UpdateReturn(hideField, c =>
                    {
                        GuardSchemaField.CanHide(Snapshot.SchemaDef, c);

                        HideField(c);

                        return Snapshot;
                    });

                case ShowField showField:
                    return UpdateReturn(showField, c =>
                    {
                        GuardSchemaField.CanShow(Snapshot.SchemaDef, c);

                        ShowField(c);

                        return Snapshot;
                    });

                case DisableField disableField:
                    return UpdateReturn(disableField, c =>
                    {
                        GuardSchemaField.CanDisable(Snapshot.SchemaDef, c);

                        DisableField(c);

                        return Snapshot;
                    });

                case EnableField enableField:
                    return UpdateReturn(enableField, c =>
                    {
                        GuardSchemaField.CanEnable(Snapshot.SchemaDef, c);

                        EnableField(c);

                        return Snapshot;
                    });

                case UpdateField updateField:
                    return UpdateReturn(updateField, c =>
                    {
                        GuardSchemaField.CanUpdate(Snapshot.SchemaDef, c);

                        UpdateField(c);

                        return Snapshot;
                    });

                case ReorderFields reorderFields:
                    return UpdateReturn(reorderFields, c =>
                    {
                        GuardSchema.CanReorder(Snapshot.SchemaDef, c);

                        Reorder(c);

                        return Snapshot;
                    });

                case UpdateSchema updateSchema:
                    return UpdateReturn(updateSchema, c =>
                    {
                        GuardSchema.CanUpdate(Snapshot.SchemaDef, c);

                        Update(c);

                        return Snapshot;
                    });

                case PublishSchema publishSchema:
                    return UpdateReturn(publishSchema, c =>
                    {
                        GuardSchema.CanPublish(Snapshot.SchemaDef, c);

                        Publish(c);

                        return Snapshot;
                    });

                case UnpublishSchema unpublishSchema:
                    return UpdateReturn(unpublishSchema, c =>
                    {
                        GuardSchema.CanUnpublish(Snapshot.SchemaDef, c);

                        Unpublish(c);

                        return Snapshot;
                    });

                case ConfigureScripts configureScripts:
                    return UpdateReturn(configureScripts, c =>
                    {
                        GuardSchema.CanConfigureScripts(Snapshot.SchemaDef, c);

                        ConfigureScripts(c);

                        return Snapshot;
                    });

                case ChangeCategory changeCategory:
                    return UpdateReturn(changeCategory, c =>
                    {
                        GuardSchema.CanChangeCategory(Snapshot.SchemaDef, c);

                        ChangeCategory(c);

                        return Snapshot;
                    });

                case ConfigurePreviewUrls configurePreviewUrls:
                    return UpdateReturn(configurePreviewUrls, c =>
                    {
                        GuardSchema.CanConfigurePreviewUrls(c);

                        ConfigurePreviewUrls(c);

                        return Snapshot;
                    });

                case DeleteSchema deleteSchema:
                    return Update(deleteSchema, c =>
                    {
                        GuardSchema.CanDelete(Snapshot.SchemaDef, c);

                        Delete(c);
                    });

                default:
                    throw new NotSupportedException();
            }
        }

        public void Synchronize(SynchronizeSchema command)
        {
            var options = new SchemaSynchronizationOptions
            {
                NoFieldDeletion = command.NoFieldDeletion,
                NoFieldRecreation = command.NoFieldRecreation
            };

            var schemaSource = Snapshot.SchemaDef;
            var schemaTarget = command.ToSchema(schemaSource.Name, schemaSource.IsSingleton);

            var events = schemaSource.Synchronize(schemaTarget, serializer, () => Snapshot.SchemaFieldsTotal + 1, options);

            foreach (var @event in events)
            {
                RaiseEvent(SimpleMapper.Map(command, (SchemaEvent)@event));
            }
        }

        public void Create(CreateSchema command)
        {
            RaiseEvent(command, new SchemaCreated { SchemaId = NamedId.Of(command.SchemaId, command.Name), Schema = command.ToSchema() });
        }

        public void Add(AddField command)
        {
            RaiseEvent(command, new FieldAdded { FieldId = CreateFieldId(command) });
        }

        public void UpdateField(UpdateField command)
        {
            RaiseEvent(command, new FieldUpdated());
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
            RaiseEvent(command, new SchemaFieldsReordered());
        }

        public void Publish(PublishSchema command)
        {
            RaiseEvent(command, new SchemaPublished());
        }

        public void Unpublish(UnpublishSchema command)
        {
            RaiseEvent(command, new SchemaUnpublished());
        }

        public void ConfigureScripts(ConfigureScripts command)
        {
            RaiseEvent(command, new SchemaScriptsConfigured());
        }

        public void ChangeCategory(ChangeCategory command)
        {
            RaiseEvent(command, new SchemaCategoryChanged());
        }

        public void ConfigurePreviewUrls(ConfigurePreviewUrls command)
        {
            RaiseEvent(command, new SchemaPreviewUrlsConfigured());
        }

        public void Update(UpdateSchema command)
        {
            RaiseEvent(command, new SchemaUpdated());
        }

        public void Delete(DeleteSchema command)
        {
            RaiseEvent(command, new SchemaDeleted());
        }

        private void RaiseEvent<TCommand, TEvent>(TCommand command, TEvent @event) where TCommand : SchemaCommand where TEvent : SchemaEvent
        {
            SimpleMapper.Map(command, @event);

            NamedId<long> GetFieldId(long? id)
            {
                if (id.HasValue && Snapshot.SchemaDef.FieldsById.TryGetValue(id.Value, out var field))
                {
                    return field.NamedId();
                }

                return null;
            }

            if (command is ParentFieldCommand pc && @event is ParentFieldEvent pe)
            {
                if (pc.ParentFieldId.HasValue)
                {
                    if (Snapshot.SchemaDef.FieldsById.TryGetValue(pc.ParentFieldId.Value, out var field))
                    {
                        pe.ParentFieldId = field.NamedId();

                        if (command is FieldCommand fc && @event is FieldEvent fe)
                        {
                            if (field is IArrayField arrayField && arrayField.FieldsById.TryGetValue(fc.FieldId, out var nestedField))
                            {
                                fe.FieldId = nestedField.NamedId();
                            }
                        }
                    }
                }
                else if (command is FieldCommand fc && @event is FieldEvent fe)
                {
                    fe.FieldId = GetFieldId(fc.FieldId);
                }
            }

            RaiseEvent(@event);
        }

        private void RaiseEvent(SchemaEvent @event)
        {
            if (@event.SchemaId == null)
            {
                @event.SchemaId = Snapshot.NamedId();
            }

            if (@event.AppId == null)
            {
                @event.AppId = Snapshot.AppId;
            }

            RaiseEvent(Envelope.Create(@event));
        }

        private NamedId<long> CreateFieldId(AddField command)
        {
            return NamedId.Of(Snapshot.SchemaFieldsTotal + 1, command.Name);
        }

        private void VerifyNotDeleted()
        {
            if (Snapshot.IsDeleted)
            {
                throw new DomainException("Schema has already been deleted.");
            }
        }

        public Task<J<ISchemaEntity>> GetStateAsync()
        {
            return J.AsTask<ISchemaEntity>(Snapshot);
        }
    }
}