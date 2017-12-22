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
using Squidex.Domain.Apps.Entities.Schemas.Commands;
using Squidex.Domain.Apps.Entities.TestHelpers;
using Squidex.Domain.Apps.Events.Schemas;
using Squidex.Infrastructure;
using Xunit;

namespace Squidex.Domain.Apps.Entities.Schemas
{
    public class SchemaDomainObjectTests : HandlerTestBase<SchemaDomainObject>
    {
        private readonly string fieldName = "age";
        private readonly NamedId<long> fieldId;
        private readonly SchemaDomainObject sut;

        protected override Guid Id
        {
            get { return SchemaId; }
        }

        public SchemaDomainObjectTests()
        {
            fieldId = new NamedId<long>(1, fieldName);

            var fieldRegistry = new FieldRegistry(new TypeNameRegistry());

            sut = new SchemaDomainObject(fieldRegistry);
        }

        [Fact]
        public void Create_should_throw_exception_if_created()
        {
            sut.Create(CreateCommand(new CreateSchema { Name = SchemaName }));

            Assert.Throws<DomainException>(() =>
            {
                sut.Create(CreateCommand(new CreateSchema { Name = SchemaName }));
            });
        }

        [Fact]
        public void Create_should_create_schema_and_create_events()
        {
            var properties = new SchemaProperties();

            sut.Create(CreateCommand(new CreateSchema { Name = SchemaName, SchemaId = SchemaId, Properties = properties }));

            Assert.Equal(AppId, sut.Snapshot.AppId);

            Assert.Equal(SchemaName, sut.Snapshot.Name);
            Assert.Equal(SchemaName, sut.Snapshot.SchemaDef.Name);

            sut.GetUncomittedEvents()
                .ShouldHaveSameEvents(
                    CreateEvent(new SchemaCreated { Name = SchemaName, Properties = properties })
                );
        }

        [Fact]
        public void Create_should_create_schema_with_initial_fields()
        {
            var properties = new SchemaProperties();

            var fields = new List<CreateSchemaField>
            {
                new CreateSchemaField { Name = "field1", Properties = ValidProperties() },
                new CreateSchemaField { Name = "field2", Properties = ValidProperties() }
            };

            sut.Create(CreateCommand(new CreateSchema { Name = SchemaName, Properties = properties, Fields = fields }));

            var @event = (SchemaCreated)sut.GetUncomittedEvents().Single().Payload;

            Assert.Equal(AppId, sut.Snapshot.AppId);
            Assert.Equal(SchemaName, sut.Snapshot.Name);
            Assert.Equal(SchemaName, sut.Snapshot.SchemaDef.Name);

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
        public void Update_should_refresh_properties_and_create_events()
        {
            var properties = new SchemaProperties();

            CreateSchema();

            sut.Update(CreateCommand(new UpdateSchema { Properties = properties }));

            Assert.Equal(properties, sut.Snapshot.SchemaDef.Properties);

            sut.GetUncomittedEvents()
                .ShouldHaveSameEvents(
                    CreateEvent(new SchemaUpdated { Properties = properties })
                );
        }

        [Fact]
        public void ConfigureScripts_should_throw_exception_if_not_created()
        {
            Assert.Throws<DomainException>(() =>
            {
                sut.ConfigureScripts(CreateCommand(new ConfigureScripts()));
            });
        }

        [Fact]
        public void ConfigureScripts_should_throw_exception_if_schema_is_deleted()
        {
            CreateSchema();
            DeleteSchema();

            Assert.Throws<DomainException>(() =>
            {
                sut.ConfigureScripts(CreateCommand(new ConfigureScripts()));
            });
        }

        [Fact]
        public void ConfigureScripts_should_create_events()
        {
            CreateSchema();

            sut.ConfigureScripts(CreateCommand(new ConfigureScripts
            {
                ScriptQuery = "<script-query>",
                ScriptCreate = "<script-create>",
                ScriptUpdate = "<script-update>",
                ScriptDelete = "<script-delete>",
                ScriptChange = "<script-change>"
            }));

            sut.GetUncomittedEvents()
                .ShouldHaveSameEvents(
                    CreateEvent(new ScriptsConfigured
                    {
                        ScriptQuery = "<script-query>",
                        ScriptCreate = "<script-create>",
                        ScriptUpdate = "<script-update>",
                        ScriptDelete = "<script-delete>",
                        ScriptChange = "<script-change>"
                    })
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
        public void Reorder_should_refresh_properties_and_create_events()
        {
            var fieldIds = new List<long> { 1, 2 };

            CreateSchema();

            sut.Add(CreateCommand(new AddField { Name = "field1", Properties = ValidProperties() }));
            sut.Add(CreateCommand(new AddField { Name = "field2", Properties = ValidProperties() }));

            sut.ClearUncommittedEvents();

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

            Assert.True(sut.Snapshot.SchemaDef.IsPublished);

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

            Assert.False(sut.Snapshot.SchemaDef.IsPublished);

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

            Assert.True(sut.Snapshot.IsDeleted);

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
                sut.Add(CreateCommand(new AddField { Name = fieldName, Properties = ValidProperties() }));
            });
        }

        [Fact]
        public void AddField_should_throw_exception_if_schema_is_deleted()
        {
            CreateSchema();
            DeleteSchema();

            Assert.Throws<DomainException>(() =>
            {
                sut.Add(CreateCommand(new AddField { Name = fieldName, Properties = new NumberFieldProperties() }));
            });
        }

        [Fact]
        public void Add_should_update_schema_and_create_events()
        {
            var properties = new NumberFieldProperties();

            CreateSchema();

            sut.Add(CreateCommand(new AddField { Name = fieldName, Properties = properties }));

            Assert.Equal(properties, sut.Snapshot.SchemaDef.FieldsById[1].RawProperties);

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

            Assert.Equal(properties, sut.Snapshot.SchemaDef.FieldsById[1].RawProperties);

            sut.GetUncomittedEvents()
                .ShouldHaveSameEvents(
                    CreateEvent(new FieldUpdated { FieldId = fieldId, Properties = properties })
                );
        }

        [Fact]
        public void LockField_should_throw_exception_if_not_created()
        {
            Assert.Throws<DomainException>(() =>
            {
                sut.LockField(CreateCommand(new LockField { FieldId = 1 }));
            });
        }

        [Fact]
        public void LockField_should_throw_exception_if_schema_is_deleted()
        {
            CreateSchema();
            DeleteSchema();

            Assert.Throws<DomainException>(() =>
            {
                sut.LockField(CreateCommand(new LockField { FieldId = 1 }));
            });
        }

        [Fact]
        public void LockField_should_update_schema_and_create_events()
        {
            CreateSchema();
            CreateField();

            sut.LockField(CreateCommand(new LockField { FieldId = 1 }));

            Assert.False(sut.Snapshot.SchemaDef.FieldsById[1].IsDisabled);

            sut.GetUncomittedEvents()
                .ShouldHaveSameEvents(
                    CreateEvent(new FieldLocked { FieldId = fieldId })
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

            Assert.True(sut.Snapshot.SchemaDef.FieldsById[1].IsHidden);

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

            Assert.False(sut.Snapshot.SchemaDef.FieldsById[1].IsHidden);

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

            Assert.True(sut.Snapshot.SchemaDef.FieldsById[1].IsDisabled);

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

            Assert.False(sut.Snapshot.SchemaDef.FieldsById[1].IsDisabled);

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

            Assert.False(sut.Snapshot.SchemaDef.FieldsById.ContainsKey(1));

            sut.GetUncomittedEvents()
                .ShouldHaveSameEvents(
                    CreateEvent(new FieldDeleted { FieldId = fieldId })
                );
        }

        private void CreateField()
        {
            sut.Add(CreateCommand(new AddField { Name = fieldName, Properties = new NumberFieldProperties() }));
            sut.ClearUncommittedEvents();
        }

        private void CreateSchema()
        {
            sut.Create(CreateCommand(new CreateSchema { Name = SchemaName }));
            sut.ClearUncommittedEvents();
        }

        private void PublishSchema()
        {
            sut.Publish(CreateCommand(new PublishSchema()));
            sut.ClearUncommittedEvents();
        }

        private void DeleteSchema()
        {
            sut.Delete(CreateCommand(new DeleteSchema()));
            sut.ClearUncommittedEvents();
        }

        private static StringFieldProperties ValidProperties()
        {
            return new StringFieldProperties { MinLength = 10, MaxLength = 20 };
        }

        private static StringFieldProperties InvalidProperties()
        {
            return new StringFieldProperties { MinLength = 20, MaxLength = 10 };
        }
    }
}
