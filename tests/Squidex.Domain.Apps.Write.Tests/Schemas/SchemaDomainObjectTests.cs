// ==========================================================================
//  SchemaDomainObjectTests.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System;
using System.Collections.Generic;
using System.Linq;
using Squidex.Domain.Apps.Core.Schemas;
using Squidex.Domain.Apps.Events.Schemas;
using Squidex.Domain.Apps.Write.Schemas.Commands;
using Squidex.Domain.Apps.Write.TestHelpers;
using Squidex.Infrastructure;
using Squidex.Infrastructure.CQRS;
using Xunit;

// ReSharper disable ConvertToConstant.Local

namespace Squidex.Domain.Apps.Write.Schemas
{
    public class SchemaDomainObjectTests : HandlerTestBase<SchemaDomainObject>
    {
        private readonly string fieldName = "age";
        private readonly NamedId<long> fieldId;
        private readonly SchemaDomainObject sut;

        public SchemaDomainObjectTests()
        {
            fieldId = new NamedId<long>(1, fieldName);

            var fieldRegistry = new FieldRegistry(new TypeNameRegistry());

            sut = new SchemaDomainObject(SchemaId, 0, fieldRegistry);
        }

        [Fact]
        public void Create_should_throw_exception_if_created()
        {
            sut.Create(new CreateSchema { Name = SchemaName });

            Assert.Throws<DomainException>(() =>
            {
                sut.Create(CreateCommand(new CreateSchema { Name = SchemaName }));
            });
        }

        [Fact]
        public void Create_should_throw_exception_if_command_is_not_valid()
        {
            Assert.Throws<ValidationException>(() =>
            {
                sut.Create(CreateCommand(new CreateSchema()));
            });
        }

        [Fact]
        public void Create_should_throw_exception_if_fields_are_not_valid()
        {
            var properties = new SchemaProperties();

            var fields = new List<CreateSchemaField>
            {
                new CreateSchemaField
                {
                    Name = null,
                    Properties = null,
                    Partitioning = "invalid"
                }
            };

            Assert.Throws<ValidationException>(() =>
            {
                sut.Create(CreateCommand(new CreateSchema { Name = SchemaName, Properties = properties, Fields = fields }));
            });
        }

        [Fact]
        public void Create_should_create_schema_and_create_events()
        {
            var properties = new SchemaProperties();

            sut.Create(CreateCommand(new CreateSchema { Name = SchemaName, Properties = properties }));

            Assert.Equal(SchemaName, sut.Schema.Name);

            sut.GetUncomittedEvents()
                .ShouldHaveSameEvents(
                    CreateEvent(new SchemaCreated { Name = SchemaName, Properties = properties, Fields = new List<SchemaCreatedField>() })
                );
        }

        [Fact]
        public void Create_should_create_schema_with_initial_fields()
        {
            var properties = new SchemaProperties();

            var fields = new List<CreateSchemaField>
            {
                new CreateSchemaField { Name = "field1", Properties = new GeolocationFieldProperties() },
                new CreateSchemaField { Name = "field2", Properties = new StringFieldProperties() }
            };

            sut.Create(CreateCommand(new CreateSchema { Name = SchemaName, Properties = properties, Fields = fields  }));

            var @event = (SchemaCreated)sut.GetUncomittedEvents().Single().Payload;

            Assert.Equal(SchemaName, sut.Schema.Name);
            Assert.Equal(2, @event.Fields.Count);
        }

        [Fact]
        public void Update_should_throw_exception_if_not_created()
        {
            Assert.Throws<DomainException>(() =>
            {
                sut.Update(CreateCommand(new UpdateSchema { Properties = new SchemaProperties() }));
            });
        }

        [Fact]
        public void Update_should_throw_exception_if_schema_is_deleted()
        {
            CreateSchema();
            DeleteSchema();

            Assert.Throws<DomainException>(() =>
            {
                sut.Update(CreateCommand(new UpdateSchema { Properties = new SchemaProperties() }));
            });
        }

        [Fact]
        public void Update_should_throw_exception_if_command_is_not_valid()
        {
            CreateSchema();

            Assert.Throws<ValidationException>(() =>
            {
                sut.Update(CreateCommand(new UpdateSchema()));
            });
        }

        [Fact]
        public void Update_should_refresh_properties_and_create_events()
        {
            var properties = new SchemaProperties();

            CreateSchema();

            sut.Update(CreateCommand(new UpdateSchema { Properties = properties }));

            Assert.Equal(properties, sut.Schema.Properties);

            sut.GetUncomittedEvents()
                .ShouldHaveSameEvents(
                    CreateEvent(new SchemaUpdated { Properties = properties })
                );
        }

        [Fact]
        public void Reorder_should_throw_exception_if_not_created()
        {
            Assert.Throws<DomainException>(() =>
            {
                sut.Reorder(CreateCommand(new ReorderFields { FieldIds = new List<long>() }));
            });
        }

        [Fact]
        public void Reorder_should_throw_exception_if_schema_is_deleted()
        {
            CreateSchema();
            DeleteSchema();

            Assert.Throws<DomainException>(() =>
            {
                sut.Reorder(CreateCommand(new ReorderFields { FieldIds = new List<long>() }));
            });
        }

        [Fact]
        public void Reorder_should_throw_exception_if_command_is_not_valid()
        {
            CreateSchema();

            Assert.Throws<ValidationException>(() =>
            {
                sut.Reorder(CreateCommand(new ReorderFields()));
            });
        }

        [Fact]
        public void Reorder_should_refresh_properties_and_create_events()
        {
            var fieldIds = new List<long> { 1, 2 };

            CreateSchema();

            sut.AddField(new AddField { Name = "field1", Properties = new StringFieldProperties() });
            sut.AddField(new AddField { Name = "field2", Properties = new StringFieldProperties() });

            ((IAggregate)sut).ClearUncommittedEvents();

            sut.Reorder(CreateCommand(new ReorderFields { FieldIds = fieldIds }));

            sut.GetUncomittedEvents()
                .ShouldHaveSameEvents(
                    CreateEvent(new SchemaFieldsReordered { FieldIds = fieldIds })
                );
        }

        [Fact]
        public void Publish_should_throw_exception_if_not_created()
        {
            Assert.Throws<DomainException>(() =>
            {
                sut.Publish(CreateCommand(new PublishSchema()));
            });
        }

        [Fact]
        public void Publish_should_throw_exception_if_schema_is_deleted()
        {
            CreateSchema();
            DeleteSchema();

            Assert.Throws<DomainException>(() =>
            {
                sut.Publish(CreateCommand(new PublishSchema()));
            });
        }

        [Fact]
        public void Publish_should_refresh_properties_and_create_events()
        {
            CreateSchema();

            sut.Publish(CreateCommand(new PublishSchema()));

            Assert.True(sut.Schema.IsPublished);

            sut.GetUncomittedEvents()
                .ShouldHaveSameEvents(
                    CreateEvent(new SchemaPublished())
                );
        }

        [Fact]
        public void Unpublish_should_throw_exception_if_not_created()
        {
            Assert.Throws<DomainException>(() =>
            {
                sut.Unpublish(CreateCommand(new UnpublishSchema()));
            });
        }

        [Fact]
        public void Unpublish_should_throw_exception_if_schema_is_deleted()
        {
            CreateSchema();
            DeleteSchema();

            Assert.Throws<DomainException>(() =>
            {
                sut.Unpublish(CreateCommand(new UnpublishSchema()));
            });
        }

        [Fact]
        public void Unpublish_should_refresh_properties_and_create_events()
        {
            CreateSchema();
            PublishSchema();

            sut.Unpublish(CreateCommand(new UnpublishSchema()));

            Assert.False(sut.Schema.IsPublished);

            sut.GetUncomittedEvents()
                .ShouldHaveSameEvents(
                    CreateEvent(new SchemaUnpublished())
                );
        }

        [Fact]
        public void Delete_should_throw_exception_if_not_created()
        {
            Assert.Throws<DomainException>(() =>
            {
                sut.Delete(CreateCommand(new DeleteSchema()));
            });
        }

        [Fact]
        public void Delete_should_throw_exception_if_already_deleted()
        {
            CreateSchema();
            DeleteSchema();

            Assert.Throws<DomainException>(() =>
            {
                sut.Delete(CreateCommand(new DeleteSchema()));
            });
        }

        [Fact]
        public void Delete_should_refresh_properties_and_create_events()
        {
            CreateSchema();

            sut.Delete(CreateCommand(new DeleteSchema()));

            Assert.True(sut.IsDeleted);

            sut.GetUncomittedEvents()
                .ShouldHaveSameEvents(
                    CreateEvent(new SchemaDeleted())
                );
        }

        [Fact]
        public void AddField_should_throw_exception_if_not_created()
        {
            Assert.Throws<DomainException>(() =>
            {
                sut.AddField(CreateCommand(new AddField { Name = fieldName, Properties = new NumberFieldProperties() }));
            });
        }

        [Fact]
        public void AddField_should_throw_exception_if_command_is_not_valid()
        {
            Assert.Throws<ValidationException>(() =>
            {
                sut.AddField(CreateCommand(new AddField()));
            });
        }

        [Fact]
        public void AddField_should_throw_exception_if_command_contains_invalid_partitioning()
        {
            Assert.Throws<ValidationException>(() =>
            {
                sut.AddField(CreateCommand(new AddField { Name = fieldName, Partitioning = "invalid", Properties = new NumberFieldProperties() }));
            });
        }

        [Fact]
        public void AddField_should_throw_exception_if_schema_is_deleted()
        {
            CreateSchema();
            DeleteSchema();

            Assert.Throws<DomainException>(() =>
            {
                sut.AddField(CreateCommand(new AddField { Name = fieldName, Properties = new NumberFieldProperties() }));
            });
        }

        [Fact]
        public void AddField_should_update_schema_and_create_events()
        {
            var properties = new NumberFieldProperties();

            CreateSchema();

            sut.AddField(CreateCommand(new AddField { Name = fieldName, Properties = properties }));

            Assert.Equal(properties, sut.Schema.FieldsById[1].RawProperties);

            sut.GetUncomittedEvents()
                .ShouldHaveSameEvents(
                    CreateEvent(new FieldAdded { Name = fieldName, FieldId = fieldId, Properties = properties })
                );
        }

        [Fact]
        public void UpdateField_should_throw_exception_if_not_created()
        {
            Assert.Throws<DomainException>(() =>
            {
                sut.UpdateField(CreateCommand(new UpdateField { FieldId = 1, Properties = new NumberFieldProperties() }));
            });
        }

        [Fact]
        public void UpdateField_should_throw_exception_if_command_is_not_valid()
        {
            Assert.Throws<ValidationException>(() =>
            {
                sut.UpdateField(CreateCommand(new UpdateField()));
            });
        }

        [Fact]
        public void UpdateField_should_throw_exception_if_field_is_not_found()
        {
            CreateSchema();

            Assert.Throws<DomainObjectNotFoundException>(() =>
            {
                sut.UpdateField(CreateCommand(new UpdateField { FieldId = 1, Properties = new NumberFieldProperties() }));
            });
        }

        [Fact]
        public void UpdateField_should_throw_exception_if_schema_is_deleted()
        {
            CreateSchema();
            DeleteSchema();

            Assert.Throws<DomainException>(() =>
            {
                sut.UpdateField(CreateCommand(new UpdateField { FieldId = 1, Properties = new NumberFieldProperties() }));
            });
        }

        [Fact]
        public void UpdateField_should_update_schema_and_create_events()
        {
            var properties = new NumberFieldProperties();

            CreateSchema();
            CreateField();

            sut.UpdateField(CreateCommand(new UpdateField { FieldId = 1, Properties = properties }));

            Assert.Equal(properties, sut.Schema.FieldsById[1].RawProperties);

            sut.GetUncomittedEvents()
                .ShouldHaveSameEvents(
                    CreateEvent(new FieldUpdated { FieldId = fieldId, Properties = properties })
                );
        }

        [Fact]
        public void HideField_should_throw_exception_if_not_created()
        {
            Assert.Throws<DomainException>(() =>
            {
                sut.HideField(CreateCommand(new HideField { FieldId = 1 }));
            });
        }

        [Fact]
        public void HideField_should_throw_exception_if_field_is_not_found()
        {
            CreateSchema();

            Assert.Throws<DomainObjectNotFoundException>(() =>
            {
                sut.HideField(CreateCommand(new HideField { FieldId = 2 }));
            });
        }

        [Fact]
        public void HideField_should_throw_exception_if_schema_is_deleted()
        {
            CreateSchema();
            DeleteSchema();

            Assert.Throws<DomainException>(() =>
            {
                sut.HideField(CreateCommand(new HideField { FieldId = 1 }));
            });
        }

        [Fact]
        public void HideField_should_update_schema_and_create_events()
        {
            CreateSchema();
            CreateField();

            sut.HideField(CreateCommand(new HideField { FieldId = 1 }));

            Assert.True(sut.Schema.FieldsById[1].IsHidden);

            sut.GetUncomittedEvents()
                .ShouldHaveSameEvents(
                    CreateEvent(new FieldHidden { FieldId = fieldId })
                );
        }

        [Fact]
        public void ShowField_should_throw_exception_if_not_created()
        {
            Assert.Throws<DomainException>(() =>
            {
                sut.ShowField(CreateCommand(new ShowField { FieldId = 1 }));
            });
        }

        [Fact]
        public void ShowField_should_throw_exception_if_field_is_not_found()
        {
            CreateSchema();

            Assert.Throws<DomainObjectNotFoundException>(() =>
            {
                sut.ShowField(CreateCommand(new ShowField { FieldId = 2 }));
            });
        }

        [Fact]
        public void ShowField_should_throw_exception_if_schema_is_deleted()
        {
            CreateSchema();
            DeleteSchema();

            Assert.Throws<DomainException>(() =>
            {
                sut.ShowField(CreateCommand(new ShowField { FieldId = 1 }));
            });
        }

        [Fact]
        public void ShowField_should_update_schema_and_create_events()
        {
            CreateSchema();
            CreateField();

            sut.HideField(CreateCommand(new HideField { FieldId = 1 }));
            sut.ShowField(CreateCommand(new ShowField { FieldId = 1 }));

            Assert.False(sut.Schema.FieldsById[1].IsHidden);

            sut.GetUncomittedEvents().Skip(1)
                .ShouldHaveSameEvents(
                    CreateEvent(new FieldShown { FieldId = fieldId })
                );
        }

        [Fact]
        public void DisableField_should_throw_exception_if_not_created()
        {
            Assert.Throws<DomainException>(() =>
            {
                sut.DisableField(CreateCommand(new DisableField { FieldId = 1 }));
            });
        }

        [Fact]
        public void DisableField_should_throw_exception_if_field_is_not_found()
        {
            CreateSchema();

            Assert.Throws<DomainObjectNotFoundException>(() =>
            {
                sut.DisableField(CreateCommand(new DisableField { FieldId = 2 }));
            });
        }

        [Fact]
        public void DisableField_should_throw_exception_if_schema_is_deleted()
        {
            CreateSchema();
            DeleteSchema();

            Assert.Throws<DomainException>(() =>
            {
                sut.DisableField(CreateCommand(new DisableField { FieldId = 1 }));
            });
        }

        [Fact]
        public void DisableField_should_update_schema_and_create_events()
        {
            CreateSchema();
            CreateField();

            sut.DisableField(CreateCommand(new DisableField { FieldId = 1 }));

            Assert.True(sut.Schema.FieldsById[1].IsDisabled);

            sut.GetUncomittedEvents()
                .ShouldHaveSameEvents(
                    CreateEvent(new FieldDisabled { FieldId = fieldId })
                );
        }

        [Fact]
        public void EnableField_should_throw_exception_if_not_created()
        {
            Assert.Throws<DomainException>(() =>
            {
                sut.EnableField(CreateCommand(new EnableField { FieldId = 1 }));
            });
        }

        [Fact]
        public void EnableField_should_throw_exception_if_field_is_not_found()
        {
            CreateSchema();

            Assert.Throws<DomainObjectNotFoundException>(() =>
            {
                sut.EnableField(CreateCommand(new EnableField { FieldId = 2 }));
            });
        }

        [Fact]
        public void EnableField_should_throw_exception_if_schema_is_deleted()
        {
            CreateSchema();
            DeleteSchema();

            Assert.Throws<DomainException>(() =>
            {
                sut.EnableField(CreateCommand(new EnableField { FieldId = 1 }));
            });
        }

        [Fact]
        public void EnableField_should_update_schema_and_create_events()
        {
            CreateSchema();
            CreateField();

            sut.DisableField(CreateCommand(new DisableField { FieldId = 1 }));
            sut.EnableField(CreateCommand(new EnableField { FieldId = 1 }));

            Assert.False(sut.Schema.FieldsById[1].IsDisabled);

            sut.GetUncomittedEvents().Skip(1)
                .ShouldHaveSameEvents(
                    CreateEvent(new FieldEnabled { FieldId = fieldId })
                );
        }

        [Fact]
        public void DeleteField_should_throw_exception_if_not_created()
        {
            Assert.Throws<DomainException>(() =>
            {
                sut.DeleteField(CreateCommand(new DeleteField { FieldId = 1 }));
            });
        }

        [Fact]
        public void DeleteField_should_throw_exception_if_schema_is_deleted()
        {
            CreateSchema();
            DeleteSchema();

            Assert.Throws<DomainException>(() =>
            {
                sut.DeleteField(CreateCommand(new DeleteField { FieldId = 1 }));
            });
        }

        [Fact]
        public void DeleteField_should_update_schema_and_create_events()
        {
            CreateSchema();
            CreateField();

            sut.DeleteField(CreateCommand(new DeleteField { FieldId = 1 }));

            Assert.False(sut.Schema.FieldsById.ContainsKey(1));

            sut.GetUncomittedEvents()
                .ShouldHaveSameEvents(
                    CreateEvent(new FieldDeleted { FieldId = fieldId })
                );
        }

        [Fact]
        public void AddWebhook_should_throw_exception_if_not_created()
        {
            Assert.Throws<DomainException>(() =>
            {
                sut.AddWebhook(CreateCommand(new AddWebhook { Url = new Uri("https://cloud.squidex.io") }));
            });
        }

        [Fact]
        public void AddWebhook_should_throw_exception_if_command_is_not_valid()
        {
            Assert.Throws<ValidationException>(() =>
            {
                sut.AddWebhook(CreateCommand(new AddWebhook()));
            });
        }

        [Fact]
        public void AddWebhook_should_throw_exception_if_schema_is_deleted()
        {
            CreateSchema();
            DeleteSchema();

            Assert.Throws<DomainException>(() =>
            {
                sut.AddWebhook(CreateCommand(new AddWebhook { Url = new Uri("https://cloud.squidex.io") }));
            });
        }

        [Fact]
        public void AddWebhook_should_update_schema_and_create_events()
        {
            var command = new AddWebhook { Url = new Uri("https://cloud.squidex.io") };

            CreateSchema();

            sut.AddWebhook(CreateCommand(command));

            sut.GetUncomittedEvents()
                .ShouldHaveSameEvents(
                    CreateEvent(new WebhookAdded { Id = command.Id, Url = command.Url, SharedSecret = command.SharedSecret })
                );
        }

        [Fact]
        public void DeleteWebhook_should_throw_exception_if_not_created()
        {
            Assert.Throws<DomainException>(() =>
            {
                sut.DeleteWebhook(CreateCommand(new DeleteWebhook()));
            });
        }

        [Fact]
        public void DeleteWebhook_should_throw_exception_if_webhook_not_found()
        {
            CreateSchema();

            Assert.Throws<DomainObjectNotFoundException>(() =>
            {
                sut.DeleteWebhook(CreateCommand(new DeleteWebhook { Id = Guid.NewGuid() }));
            });
        }

        [Fact]
        public void DeleteWebhook_should_throw_exception_if_schema_is_deleted()
        {
            CreateSchema();
            DeleteSchema();

            Assert.Throws<DomainException>(() =>
            {
                sut.DeleteWebhook(CreateCommand(new DeleteWebhook { Id = Guid.NewGuid() }));
            });
        }

        [Fact]
        public void DeleteWebhook_should_update_schema_and_create_events()
        {
            var createCommand = new AddWebhook { Url = new Uri("https://cloud.squidex.io") };

            CreateSchema();

            sut.AddWebhook(CreateCommand(createCommand));
            sut.DeleteWebhook(CreateCommand(new DeleteWebhook { Id = createCommand.Id }));

            sut.GetUncomittedEvents()
                .ShouldHaveSameEvents(
                    CreateEvent(new WebhookAdded { Id = createCommand.Id, Url = createCommand.Url, SharedSecret = createCommand.SharedSecret }),
                    CreateEvent(new WebhookDeleted { Id = createCommand.Id })
                );
        }

        private void CreateField()
        {
            sut.AddField(new AddField { Name = fieldName, Properties = new NumberFieldProperties() });

            ((IAggregate)sut).ClearUncommittedEvents();
        }

        private void CreateSchema()
        {
            sut.Create(CreateCommand(new CreateSchema { Name = SchemaName }));

            ((IAggregate)sut).ClearUncommittedEvents();
        }

        private void PublishSchema()
        {
            sut.Publish(CreateCommand(new PublishSchema()));

            ((IAggregate)sut).ClearUncommittedEvents();
        }

        private void DeleteSchema()
        {
            sut.Delete(CreateCommand(new DeleteSchema()));

            ((IAggregate)sut).ClearUncommittedEvents();
        }
    }
}
