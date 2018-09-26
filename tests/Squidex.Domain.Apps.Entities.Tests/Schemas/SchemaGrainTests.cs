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
using FakeItEasy;
using Squidex.Domain.Apps.Core;
using Squidex.Domain.Apps.Core.Schemas;
using Squidex.Domain.Apps.Entities.Schemas.Commands;
using Squidex.Domain.Apps.Entities.Schemas.State;
using Squidex.Domain.Apps.Entities.TestHelpers;
using Squidex.Domain.Apps.Events.Schemas;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Commands;
using Squidex.Infrastructure.Log;
using Xunit;

namespace Squidex.Domain.Apps.Entities.Schemas
{
    public class SchemaGrainTests : HandlerTestBase<SchemaGrain, SchemaState>
    {
        private readonly IAppProvider appProvider = A.Fake<IAppProvider>();
        private readonly FieldRegistry registry = new FieldRegistry(new TypeNameRegistry());
        private readonly string fieldName = "age";
        private readonly string arrayName = "array";
        private readonly NamedId<long> fieldId = NamedId.Of(1L, "age");
        private readonly NamedId<long> arrayId = NamedId.Of(1L, "array");
        private readonly NamedId<long> nestedId = NamedId.Of(2L, "age");
        private readonly SchemaGrain sut;

        protected override Guid Id
        {
            get { return SchemaId; }
        }

        public SchemaGrainTests()
        {
            A.CallTo(() => appProvider.GetSchemaAsync(AppId, SchemaName))
                .Returns((ISchemaEntity)null);

            sut = new SchemaGrain(Store, A.Dummy<ISemanticLog>(), appProvider, registry);
            sut.OnActivateAsync(Id).Wait();
        }

        [Fact]
        public async Task Command_should_throw_exception_if_schema_is_deleted()
        {
            await ExecuteCreateAsync();
            await ExecuteDeleteAsync();

            await Assert.ThrowsAsync<DomainException>(ExecutePublishAsync);
        }

        [Fact]
        public async Task Create_should_create_schema_and_create_events()
        {
            var properties = new SchemaProperties();

            var command = new CreateSchema { Name = SchemaName, SchemaId = SchemaId, Properties = properties, Singleton = true };

            var result = await sut.ExecuteAsync(CreateCommand(command));

            result.ShouldBeEquivalent(EntityCreatedResult.Create(Id, 0));

            Assert.Equal(AppId, sut.Snapshot.AppId.Id);

            Assert.Equal(SchemaName, sut.Snapshot.Name);
            Assert.Equal(SchemaName, sut.Snapshot.SchemaDef.Name);
            Assert.True(sut.Snapshot.IsSingleton);

            LastEvents
                .ShouldHaveSameEvents(
                    CreateEvent(new SchemaCreated { Name = SchemaName, Properties = properties, Singleton = true })
                );
        }

        [Fact]
        public async Task Create_should_create_schema_with_initial_fields()
        {
            var properties = new SchemaProperties();

            var fields = new List<CreateSchemaField>
            {
                new CreateSchemaField { Name = "field1", Properties = ValidProperties() },
                new CreateSchemaField { Name = "field2", Properties = ValidProperties() },
                new CreateSchemaField
                {
                    Name = "field3",
                    Partitioning = Partitioning.Language.Key,
                    Properties = new ArrayFieldProperties(),
                    Nested = new List<CreateSchemaNestedField>
                    {
                        new CreateSchemaNestedField { Name = "nested1", Properties = ValidProperties() },
                        new CreateSchemaNestedField { Name = "nested2", Properties = ValidProperties() }
                    }
                }
            };

            var command = new CreateSchema { Name = SchemaName, SchemaId = SchemaId, Properties = properties, Fields = fields };

            var result = await sut.ExecuteAsync(CreateCommand(command));

            result.ShouldBeEquivalent(EntityCreatedResult.Create(Id, 0));

            var @event = (SchemaCreated)LastEvents.Single().Payload;

            Assert.Equal(AppId, sut.Snapshot.AppId.Id);
            Assert.Equal(SchemaName, sut.Snapshot.Name);
            Assert.Equal(SchemaName, sut.Snapshot.SchemaDef.Name);

            Assert.Equal(3, @event.Fields.Count);
        }

        [Fact]
        public async Task Update_should_create_events_and_update_state()
        {
            var command = new UpdateSchema { Properties = new SchemaProperties() };

            await ExecuteCreateAsync();

            var result = await sut.ExecuteAsync(CreateCommand(command));

            result.ShouldBeEquivalent(new EntitySavedResult(1));

            Assert.Equal(command.Properties, sut.Snapshot.SchemaDef.Properties);

            LastEvents
                .ShouldHaveSameEvents(
                    CreateEvent(new SchemaUpdated { Properties = command.Properties })
                );
        }

        [Fact]
        public async Task ConfigureScripts_should_create_events()
        {
            var command = new ConfigureScripts
            {
                ScriptQuery = "<script-query>",
                ScriptCreate = "<script-create>",
                ScriptUpdate = "<script-update>",
                ScriptDelete = "<script-delete>",
                ScriptChange = "<script-change>"
            };

            await ExecuteCreateAsync();

            var result = await sut.ExecuteAsync(CreateCommand(command));

            result.ShouldBeEquivalent(new EntitySavedResult(1));

            LastEvents
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
        public async Task Publish_should_create_events_and_update_state()
        {
            var command = new PublishSchema();

            await ExecuteCreateAsync();

            var result = await sut.ExecuteAsync(CreateCommand(command));

            result.ShouldBeEquivalent(new EntitySavedResult(1));

            Assert.True(sut.Snapshot.SchemaDef.IsPublished);

            LastEvents
                .ShouldHaveSameEvents(
                    CreateEvent(new SchemaPublished())
                );
        }

        [Fact]
        public async Task Unpublish_should_create_events_and_update_state()
        {
            var command = new UnpublishSchema();

            await ExecuteCreateAsync();
            await ExecutePublishAsync();

            var result = await sut.ExecuteAsync(CreateCommand(command));

            result.ShouldBeEquivalent(new EntitySavedResult(2));

            Assert.False(sut.Snapshot.SchemaDef.IsPublished);

            LastEvents
                .ShouldHaveSameEvents(
                    CreateEvent(new SchemaUnpublished())
                );
        }

        [Fact]
        public async Task ChangeCategory_should_create_events_and_update_state()
        {
            var command = new ChangeCategory { Name = "my-category" };

            await ExecuteCreateAsync();

            var result = await sut.ExecuteAsync(CreateCommand(command));

            result.ShouldBeEquivalent(new EntitySavedResult(1));

            Assert.Equal(command.Name, sut.Snapshot.Category);

            LastEvents
                .ShouldHaveSameEvents(
                    CreateEvent(new SchemaCategoryChanged { Name = command.Name })
                );
        }

        [Fact]
        public async Task Delete_should_create_events_and_update_state()
        {
            var command = new DeleteSchema();

            await ExecuteCreateAsync();

            var result = await sut.ExecuteAsync(CreateCommand(command));

            result.ShouldBeEquivalent(new EntitySavedResult(1));

            Assert.True(sut.Snapshot.IsDeleted);

            LastEvents
                .ShouldHaveSameEvents(
                    CreateEvent(new SchemaDeleted())
                );
        }

        [Fact]
        public async Task Reorder_should_create_events_and_update_state()
        {
            var command = new ReorderFields { FieldIds = new List<long> { 1, 2 } };

            await ExecuteCreateAsync();
            await ExecuteAddFieldAsync("field1");
            await ExecuteAddFieldAsync("field2");

            var result = await sut.ExecuteAsync(CreateCommand(command));

            result.ShouldBeEquivalent(new EntitySavedResult(3));

            LastEvents
                .ShouldHaveSameEvents(
                    CreateEvent(new SchemaFieldsReordered { FieldIds = command.FieldIds })
                );
        }

        [Fact]
        public async Task Reorder_should_create_events_and_update_state_for_array()
        {
            var command = new ReorderFields { ParentFieldId = 1, FieldIds = new List<long> { 2, 3 } };

            await ExecuteCreateAsync();
            await ExecuteAddArrayFieldAsync();
            await ExecuteAddFieldAsync("field1", 1);
            await ExecuteAddFieldAsync("field2", 1);

            var result = await sut.ExecuteAsync(CreateCommand(command));

            result.ShouldBeEquivalent(new EntitySavedResult(4));

            LastEvents
                .ShouldHaveSameEvents(
                    CreateEvent(new SchemaFieldsReordered { ParentFieldId = arrayId, FieldIds = command.FieldIds })
                );
        }

        [Fact]
        public async Task Add_should_create_events_and_update_state()
        {
            var command = new AddField { Name = fieldName, Properties = ValidProperties() };

            await ExecuteCreateAsync();

            var result = await sut.ExecuteAsync(CreateCommand(command));

            result.ShouldBeEquivalent(EntityCreatedResult.Create(1, 1));

            Assert.Equal(command.Properties, GetField(1).RawProperties);

            LastEvents
                .ShouldHaveSameEvents(
                    CreateEvent(new FieldAdded { Name = fieldName, FieldId = fieldId, Properties = command.Properties })
                );
        }

        [Fact]
        public async Task Add_should_create_events_and_update_state_for_array()
        {
            var command = new AddField { ParentFieldId = 1, Name = fieldName, Properties = ValidProperties() };

            await ExecuteCreateAsync();
            await ExecuteAddArrayFieldAsync();

            var result = await sut.ExecuteAsync(CreateCommand(command));

            result.ShouldBeEquivalent(EntityCreatedResult.Create(2, 2));

            Assert.NotEqual(command.Properties, GetField(1).RawProperties);
            Assert.Equal(command.Properties, GetNestedField(1, 2).RawProperties);

            LastEvents
                .ShouldHaveSameEvents(
                    CreateEvent(new FieldAdded { ParentFieldId = arrayId, Name = fieldName, FieldId = nestedId, Properties = command.Properties })
                );
        }

        [Fact]
        public async Task UpdateField_should_create_events_and_update_state()
        {
            var command = new UpdateField { FieldId = 1, Properties = new StringFieldProperties() };

            await ExecuteCreateAsync();
            await ExecuteAddFieldAsync(fieldName);

            var result = await sut.ExecuteAsync(CreateCommand(command));

            result.ShouldBeEquivalent(new EntitySavedResult(2));

            Assert.Equal(command.Properties, GetField(1).RawProperties);

            LastEvents
                .ShouldHaveSameEvents(
                    CreateEvent(new FieldUpdated { FieldId = fieldId, Properties = command.Properties })
                );
        }

        [Fact]
        public async Task UpdateField_should_create_events_and_update_state_for_array()
        {
            var command = new UpdateField { ParentFieldId = 1, FieldId = 2, Properties = new StringFieldProperties() };

            await ExecuteCreateAsync();
            await ExecuteAddArrayFieldAsync();
            await ExecuteAddFieldAsync(fieldName, 1);

            var result = await sut.ExecuteAsync(CreateCommand(command));

            result.ShouldBeEquivalent(new EntitySavedResult(3));

            Assert.NotEqual(command.Properties, GetField(1).RawProperties);
            Assert.Equal(command.Properties, GetNestedField(1, 2).RawProperties);

            LastEvents
                .ShouldHaveSameEvents(
                    CreateEvent(new FieldUpdated { ParentFieldId = arrayId, FieldId = nestedId, Properties = command.Properties })
                );
        }

        [Fact]
        public async Task LockField_should_create_events_and_update_state()
        {
            var command = new LockField { FieldId = 1 };

            await ExecuteCreateAsync();
            await ExecuteAddFieldAsync(fieldName);

            var result = await sut.ExecuteAsync(CreateCommand(command));

            result.ShouldBeEquivalent(new EntitySavedResult(2));

            Assert.False(GetField(1).IsDisabled);

            LastEvents
                .ShouldHaveSameEvents(
                    CreateEvent(new FieldLocked { FieldId = fieldId })
                );
        }

        [Fact]
        public async Task LockField_should_create_events_and_update_state_for_array()
        {
            var command = new LockField { ParentFieldId = 1, FieldId = 2 };

            await ExecuteCreateAsync();
            await ExecuteAddArrayFieldAsync();
            await ExecuteAddFieldAsync(fieldName, 1);

            var result = await sut.ExecuteAsync(CreateCommand(command));

            result.ShouldBeEquivalent(new EntitySavedResult(3));

            Assert.False(GetField(1).IsLocked);
            Assert.True(GetNestedField(1, 2).IsLocked);

            LastEvents
                .ShouldHaveSameEvents(
                    CreateEvent(new FieldLocked { ParentFieldId = arrayId, FieldId = nestedId })
                );
        }

        [Fact]
        public async Task HideField_should_create_events_and_update_state()
        {
            var command = new HideField { FieldId = 1 };

            await ExecuteCreateAsync();
            await ExecuteAddFieldAsync(fieldName);

            var result = await sut.ExecuteAsync(CreateCommand(command));

            result.ShouldBeEquivalent(new EntitySavedResult(2));

            Assert.True(GetField(1).IsHidden);

            LastEvents
                .ShouldHaveSameEvents(
                    CreateEvent(new FieldHidden { FieldId = fieldId })
                );
        }

        [Fact]
        public async Task HideField_should_create_events_and_update_state_for_array()
        {
            var command = new HideField { ParentFieldId = 1, FieldId = 2 };

            await ExecuteCreateAsync();
            await ExecuteAddArrayFieldAsync();
            await ExecuteAddFieldAsync(fieldName, 1);

            var result = await sut.ExecuteAsync(CreateCommand(command));

            result.ShouldBeEquivalent(new EntitySavedResult(3));

            Assert.False(GetField(1).IsHidden);
            Assert.True(GetNestedField(1, 2).IsHidden);

            LastEvents
                .ShouldHaveSameEvents(
                    CreateEvent(new FieldHidden { ParentFieldId = arrayId, FieldId = nestedId })
                );
        }

        [Fact]
        public async Task ShowField_should_create_events_and_update_state()
        {
            var command = new ShowField { FieldId = 1 };

            await ExecuteCreateAsync();
            await ExecuteAddFieldAsync(fieldName);
            await ExecuteHideFieldAsync(1);

            var result = await sut.ExecuteAsync(CreateCommand(command));

            result.ShouldBeEquivalent(new EntitySavedResult(3));

            Assert.False(GetField(1).IsHidden);

            LastEvents
                .ShouldHaveSameEvents(
                    CreateEvent(new FieldShown { FieldId = fieldId })
                );
        }

        [Fact]
        public async Task ShowField_should_create_events_and_update_state_for_array()
        {
            var command = new ShowField { ParentFieldId = 1, FieldId = 2 };

            await ExecuteCreateAsync();
            await ExecuteAddArrayFieldAsync();
            await ExecuteAddFieldAsync(fieldName, 1);
            await ExecuteHideFieldAsync(2, 1);

            var result = await sut.ExecuteAsync(CreateCommand(command));

            result.ShouldBeEquivalent(new EntitySavedResult(4));

            Assert.False(GetField(1).IsHidden);
            Assert.False(GetNestedField(1, 2).IsHidden);

            LastEvents
                .ShouldHaveSameEvents(
                    CreateEvent(new FieldShown { ParentFieldId = arrayId, FieldId = nestedId })
                );
        }

        [Fact]
        public async Task DisableField_should_create_events_and_update_state()
        {
            var command = new DisableField { FieldId = 1 };

            await ExecuteCreateAsync();
            await ExecuteAddFieldAsync(fieldName);

            var result = await sut.ExecuteAsync(CreateCommand(command));

            result.ShouldBeEquivalent(new EntitySavedResult(2));

            Assert.True(GetField(1).IsDisabled);

            LastEvents
                .ShouldHaveSameEvents(
                    CreateEvent(new FieldDisabled { FieldId = fieldId })
                );
        }

        [Fact]
        public async Task DisableField_should_create_events_and_update_state_for_array()
        {
            var command = new DisableField { ParentFieldId = 1, FieldId = 2 };

            await ExecuteCreateAsync();
            await ExecuteAddArrayFieldAsync();
            await ExecuteAddFieldAsync(fieldName, 1);

            var result = await sut.ExecuteAsync(CreateCommand(command));

            result.ShouldBeEquivalent(new EntitySavedResult(3));

            Assert.False(GetField(1).IsDisabled);
            Assert.True(GetNestedField(1, 2).IsDisabled);

            LastEvents
                .ShouldHaveSameEvents(
                    CreateEvent(new FieldDisabled { ParentFieldId = arrayId, FieldId = nestedId })
                );
        }

        [Fact]
        public async Task EnableField_should_create_events_and_update_state()
        {
            var command = new EnableField { FieldId = 1 };

            await ExecuteCreateAsync();
            await ExecuteAddFieldAsync(fieldName);
            await ExecuteDisableFieldAsync(1);

            var result = await sut.ExecuteAsync(CreateCommand(command));

            result.ShouldBeEquivalent(new EntitySavedResult(3));

            Assert.False(GetField(1).IsDisabled);

            LastEvents
                .ShouldHaveSameEvents(
                    CreateEvent(new FieldEnabled { FieldId = fieldId })
                );
        }

        [Fact]
        public async Task EnableField_should_create_events_and_update_state_for_array()
        {
            var command = new EnableField { ParentFieldId = 1, FieldId = 2 };

            await ExecuteCreateAsync();
            await ExecuteAddArrayFieldAsync();
            await ExecuteAddFieldAsync(fieldName, 1);
            await ExecuteDisableFieldAsync(2, 1);

            var result = await sut.ExecuteAsync(CreateCommand(command));

            result.ShouldBeEquivalent(new EntitySavedResult(4));

            Assert.False(GetField(1).IsDisabled);
            Assert.False(GetNestedField(1, 2).IsDisabled);

            LastEvents
                .ShouldHaveSameEvents(
                    CreateEvent(new FieldEnabled { ParentFieldId = arrayId, FieldId = nestedId })
                );
        }

        [Fact]
        public async Task DeleteField_should_create_events_and_update_state()
        {
            var command = new DeleteField { FieldId = 1 };

            await ExecuteCreateAsync();
            await ExecuteAddFieldAsync(fieldName);

            var result = await sut.ExecuteAsync(CreateCommand(command));

            result.ShouldBeEquivalent(new EntitySavedResult(2));

            Assert.Null(GetField(1));

            LastEvents
                .ShouldHaveSameEvents(
                    CreateEvent(new FieldDeleted { FieldId = fieldId })
                );
        }

        [Fact]
        public async Task DeleteField_should_create_events_and_update_state_for_array()
        {
            var command = new DeleteField { ParentFieldId = 1, FieldId = 2 };

            await ExecuteCreateAsync();
            await ExecuteAddArrayFieldAsync();
            await ExecuteAddFieldAsync(fieldName, 1);

            var result = await sut.ExecuteAsync(CreateCommand(command));

            result.ShouldBeEquivalent(new EntitySavedResult(3));

            Assert.NotNull(GetField(1));
            Assert.Null(GetNestedField(1, 2));

            LastEvents
                .ShouldHaveSameEvents(
                    CreateEvent(new FieldDeleted { ParentFieldId = arrayId, FieldId = nestedId })
                );
        }

        private Task ExecuteCreateAsync()
        {
            return sut.ExecuteAsync(CreateCommand(new CreateSchema { Name = SchemaName }));
        }

        private Task ExecuteAddArrayFieldAsync()
        {
            return sut.ExecuteAsync(CreateCommand(new AddField { Properties = new ArrayFieldProperties(), Name = arrayName }));
        }

        private Task ExecuteAddFieldAsync(string name, long? parentId = null)
        {
            return sut.ExecuteAsync(CreateCommand(new AddField { ParentFieldId = parentId, Properties = ValidProperties(), Name = name }));
        }

        private Task ExecuteHideFieldAsync(long id, long? parentId = null)
        {
            return sut.ExecuteAsync(CreateCommand(new HideField { ParentFieldId = parentId, FieldId = id }));
        }

        private Task ExecuteDisableFieldAsync(long id, long? parentId = null)
        {
            return sut.ExecuteAsync(CreateCommand(new DisableField { ParentFieldId = parentId, FieldId = id }));
        }

        private Task ExecutePublishAsync()
        {
            return sut.ExecuteAsync(CreateCommand(new PublishSchema()));
        }

        private Task ExecuteDeleteAsync()
        {
            return sut.ExecuteAsync(CreateCommand(new DeleteSchema()));
        }

        private IField GetField(int id)
        {
            return sut.Snapshot.SchemaDef.FieldsById.GetOrDefault(id);
        }

        private IField GetNestedField(int parentId, int childId)
        {
            return ((IArrayField)sut.Snapshot.SchemaDef.FieldsById[parentId]).FieldsById.GetOrDefault(childId);
        }

        private static StringFieldProperties ValidProperties()
        {
            return new StringFieldProperties { MinLength = 10, MaxLength = 20 };
        }
    }
}
