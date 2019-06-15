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
                    return UpdateReturnAsync(addField, async c =>
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

                        return await GetRawStateAsync();
                    });

                case CreateSchema createSchema:
                    return CreateReturnAsync(createSchema, async c =>
                    {
                        await GuardSchema.CanCreate(c, appProvider);

                        Create(c);

                        return await GetRawStateAsync();
                    });

                case SynchronizeSchema synchronizeSchema:
                    return UpdateReturnAsync(synchronizeSchema, async c =>
                    {
                        GuardSchema.CanSynchronize(c);

                        Synchronize(c);

                        return await GetRawStateAsync();
                    });

                case DeleteField deleteField:
                    return UpdateReturnAsync(deleteField, async c =>
                    {
                        GuardSchemaField.CanDelete(Snapshot.SchemaDef, deleteField);

                        DeleteField(c);

                        return await GetRawStateAsync();
                    });

                case LockField lockField:
                    return UpdateReturnAsync(lockField, async c =>
                    {
                        GuardSchemaField.CanLock(Snapshot.SchemaDef, lockField);

                        LockField(c);

                        return await GetRawStateAsync();
                    });

                case HideField hideField:
                    return UpdateReturnAsync(hideField, async c =>
                    {
                        GuardSchemaField.CanHide(Snapshot.SchemaDef, c);

                        HideField(c);

                        return await GetRawStateAsync();
                    });

                case ShowField showField:
                    return UpdateReturnAsync(showField, async c =>
                    {
                        GuardSchemaField.CanShow(Snapshot.SchemaDef, c);

                        ShowField(c);

                        return await GetRawStateAsync();
                    });

                case DisableField disableField:
                    return UpdateReturnAsync(disableField, async c =>
                    {
                        GuardSchemaField.CanDisable(Snapshot.SchemaDef, c);

                        DisableField(c);

                        return await GetRawStateAsync();
                    });

                case EnableField enableField:
                    return UpdateReturnAsync(enableField, async c =>
                    {
                        GuardSchemaField.CanEnable(Snapshot.SchemaDef, c);

                        EnableField(c);

                        return await GetRawStateAsync();
                    });

                case UpdateField updateField:
                    return UpdateReturnAsync(updateField, async c =>
                    {
                        GuardSchemaField.CanUpdate(Snapshot.SchemaDef, c);

                        UpdateField(c);

                        return await GetRawStateAsync();
                    });

                case ReorderFields reorderFields:
                    return UpdateReturnAsync(reorderFields, async c =>
                    {
                        GuardSchema.CanReorder(Snapshot.SchemaDef, c);

                        Reorder(c);

                        return await GetRawStateAsync();
                    });

                case UpdateSchema updateSchema:
                    return UpdateReturnAsync(updateSchema, async c =>
                    {
                        GuardSchema.CanUpdate(Snapshot.SchemaDef, c);

                        Update(c);

                        return await GetRawStateAsync();
                    });

                case PublishSchema publishSchema:
                    return UpdateReturnAsync(publishSchema, async c =>
                    {
                        GuardSchema.CanPublish(Snapshot.SchemaDef, c);

                        Publish(c);

                        return await GetRawStateAsync();
                    });

                case UnpublishSchema unpublishSchema:
                    return UpdateReturnAsync(unpublishSchema, async c =>
                    {
                        GuardSchema.CanUnpublish(Snapshot.SchemaDef, c);

                        Unpublish(c);

                        return await GetRawStateAsync();
                    });

                case ConfigureScripts configureScripts:
                    return UpdateReturnAsync(configureScripts, async c =>
                    {
                        GuardSchema.CanConfigureScripts(Snapshot.SchemaDef, c);

                        ConfigureScripts(c);

                        return await GetRawStateAsync();
                    });

                case ChangeCategory changeCategory:
                    return UpdateReturnAsync(changeCategory, async c =>
                    {
                        GuardSchema.CanChangeCategory(Snapshot.SchemaDef, c);

                        ChangeCategory(c);

                        return await GetRawStateAsync();
                    });

                case ConfigurePreviewUrls configurePreviewUrls:
                    return UpdateReturnAsync(configurePreviewUrls, async c =>
                    {
                        GuardSchema.CanConfigurePreviewUrls(c);

                        ConfigurePreviewUrls(c);

                        return await GetRawStateAsync();
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

        public Task<ISchemaEntity> GetRawStateAsync()
        {
            return Task.FromResult<ISchemaEntity>(Snapshot);
        }

        public Task<J<ISchemaEntity>> GetStateAsync()
        {
            return J.AsTask<ISchemaEntity>(Snapshot);
        }
    }
}