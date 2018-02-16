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
using Squidex.Domain.Apps.Core.Schemas;
using Squidex.Domain.Apps.Entities.Schemas.Commands;
using Squidex.Domain.Apps.Entities.Schemas.State;
using Squidex.Domain.Apps.Entities.TestHelpers;
using Squidex.Domain.Apps.Events.Schemas;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Commands;
using Xunit;

namespace Squidex.Domain.Apps.Entities.Schemas
{
    public class SchemaGrainTests : HandlerTestBase<SchemaGrain, SchemaState>
    {
        private readonly IAppProvider appProvider = A.Fake<IAppProvider>();
        private readonly FieldRegistry registry = new FieldRegistry(new TypeNameRegistry());
        private readonly string fieldName = "age";
        private readonly NamedId<long> fieldId;
        private readonly SchemaGrain sut;

        protected override Guid Id
        {
            get { return SchemaId; }
        }

        public SchemaGrainTests()
        {
            A.CallTo(() => appProvider.GetSchemaAsync(AppId, SchemaName))
                .Returns((ISchemaEntity)null);

            fieldId = new NamedId<long>(1, fieldName);

            sut = new SchemaGrain(Store, appProvider, registry);
            sut.ActivateAsync(Id).Wait();
        }

        [Fact]
        public async Task Command_should_throw_exception_if_rule_is_deleted()
        {
            await ExecuteCreateAsync();
            await ExecuteDeleteAsync();

            await Assert.ThrowsAsync<DomainException>(ExecutePublishAsync);
        }

        [Fact]
        public async Task Create_should_create_schema_and_create_events()
        {
            var properties = new SchemaProperties();

            var command = new CreateSchema { Name = SchemaName, SchemaId = SchemaId, Properties = properties };

            var result = await sut.ExecuteAsync(CreateCommand(command));

            result.ShouldBeEquivalent(EntityCreatedResult.Create(Id, 0));

            Assert.Equal(AppId, sut.Snapshot.AppId.Id);

            Assert.Equal(SchemaName, sut.Snapshot.Name);
            Assert.Equal(SchemaName, sut.Snapshot.SchemaDef.Name);

            LastEvents
                .ShouldHaveSameEvents(
                    CreateEvent(new SchemaCreated { Name = SchemaName, Properties = properties })
                );
        }

        [Fact]
        public async Task Create_should_create_schema_with_initial_fields()
        {
            var properties = new SchemaProperties();

            var fields = new List<CreateSchemaField>
            {
                new CreateSchemaField { Name = "field1", Properties = ValidProperties() },
                new CreateSchemaField { Name = "field2", Properties = ValidProperties() }
            };

            var command = new CreateSchema { Name = SchemaName, SchemaId = SchemaId, Properties = properties, Fields = fields };

            var result = await sut.ExecuteAsync(CreateCommand(command));

            result.ShouldBeEquivalent(EntityCreatedResult.Create(Id, 0));

            var @event = (SchemaCreated)LastEvents.Single().Payload;

            Assert.Equal(AppId, sut.Snapshot.AppId.Id);
            Assert.Equal(SchemaName, sut.Snapshot.Name);
            Assert.Equal(SchemaName, sut.Snapshot.SchemaDef.Name);

            Assert.Equal(2, @event.Fields.Count);
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
        public async Task Add_should_create_events_and_update_state()
        {
            var command = new AddField { Name = fieldName, Properties = ValidProperties() };

            await ExecuteCreateAsync();

            var result = await sut.ExecuteAsync(CreateCommand(command));

            result.ShouldBeEquivalent(EntityCreatedResult.Create(1, 1));

            Assert.Equal(command.Properties, sut.Snapshot.SchemaDef.FieldsById[1].RawProperties);

            LastEvents
                .ShouldHaveSameEvents(
                    CreateEvent(new FieldAdded { Name = fieldName, FieldId = fieldId, Properties = command.Properties })
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

            Assert.Equal(command.Properties, sut.Snapshot.SchemaDef.FieldsById[1].RawProperties);

            LastEvents
                .ShouldHaveSameEvents(
                    CreateEvent(new FieldUpdated { FieldId = fieldId, Properties = command.Properties })
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

            Assert.False(sut.Snapshot.SchemaDef.FieldsById[1].IsDisabled);

            LastEvents
                .ShouldHaveSameEvents(
                    CreateEvent(new FieldLocked { FieldId = fieldId })
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

            Assert.True(sut.Snapshot.SchemaDef.FieldsById[1].IsHidden);

            LastEvents
                .ShouldHaveSameEvents(
                    CreateEvent(new FieldHidden { FieldId = fieldId })
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

            Assert.False(sut.Snapshot.SchemaDef.FieldsById[1].IsHidden);

            LastEvents
                .ShouldHaveSameEvents(
                    CreateEvent(new FieldShown { FieldId = fieldId })
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

            Assert.True(sut.Snapshot.SchemaDef.FieldsById[1].IsDisabled);

            LastEvents
                .ShouldHaveSameEvents(
                    CreateEvent(new FieldDisabled { FieldId = fieldId })
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

            Assert.False(sut.Snapshot.SchemaDef.FieldsById[1].IsDisabled);

            LastEvents
                .ShouldHaveSameEvents(
                    CreateEvent(new FieldEnabled { FieldId = fieldId })
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

            Assert.False(sut.Snapshot.SchemaDef.FieldsById.ContainsKey(1));

            LastEvents
                .ShouldHaveSameEvents(
                    CreateEvent(new FieldDeleted { FieldId = fieldId })
                );
        }

        private Task ExecuteCreateAsync()
        {
            return sut.ExecuteAsync(CreateCommand(new CreateSchema { Name = SchemaName }));
        }

        private Task ExecuteAddFieldAsync(string name)
        {
            return sut.ExecuteAsync(CreateCommand(new AddField { Properties = ValidProperties(), Name = name }));
        }

        private Task ExecuteHideFieldAsync(long id)
        {
            return sut.ExecuteAsync(CreateCommand(new HideField { FieldId = id }));
        }

        private Task ExecuteDisableFieldAsync(long id)
        {
            return sut.ExecuteAsync(CreateCommand(new DisableField { FieldId = id }));
        }

        private Task ExecutePublishAsync()
        {
            return sut.ExecuteAsync(CreateCommand(new PublishSchema()));
        }

        private Task ExecuteDeleteAsync()
        {
            return sut.ExecuteAsync(CreateCommand(new DeleteSchema()));
        }

        private static StringFieldProperties ValidProperties()
        {
            return new StringFieldProperties { MinLength = 10, MaxLength = 20 };
        }
    }
}
