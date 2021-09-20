// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FakeItEasy;
using Squidex.Domain.Apps.Core;
using Squidex.Domain.Apps.Core.Schemas;
using Squidex.Domain.Apps.Entities.Schemas.Commands;
using Squidex.Domain.Apps.Entities.TestHelpers;
using Squidex.Domain.Apps.Events.Schemas;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Collections;
using Squidex.Infrastructure.Commands;
using Squidex.Log;
using Xunit;

namespace Squidex.Domain.Apps.Entities.Schemas.DomainObject
{
    public class SchemaDomainObjectTests : HandlerTestBase<SchemaDomainObject.State>
    {
        private readonly string fieldName = "age";
        private readonly string arrayName = "array";
        private readonly NamedId<long> fieldId = NamedId.Of(1L, "age");
        private readonly NamedId<long> arrayId = NamedId.Of(1L, "array");
        private readonly NamedId<long> nestedId = NamedId.Of(2L, "age");
        private readonly SchemaDomainObject sut;

        protected override DomainId Id
        {
            get => DomainId.Combine(AppId, SchemaId);
        }

        public SchemaDomainObjectTests()
        {
            sut = new SchemaDomainObject(PersistenceFactory, A.Dummy<ISemanticLog>());
#pragma warning disable MA0056 // Do not call overridable members in constructor
            sut.Setup(Id);
#pragma warning restore MA0056 // Do not call overridable members in constructor
        }

        [Fact]
        public async Task Command_should_throw_exception_if_schema_is_deleted()
        {
            await ExecuteCreateAsync();
            await ExecuteDeleteAsync();

            await Assert.ThrowsAsync<DomainObjectDeletedException>(ExecutePublishAsync);
        }

        [Fact]
        public async Task Create_should_create_events_and_set_intitial_state()
        {
            var properties = new SchemaProperties();

            var command = new CreateSchema { Name = SchemaName, SchemaId = SchemaId, Properties = properties, Type = SchemaType.Singleton };

            var result = await PublishAsync(command);

            result.ShouldBeEquivalent(sut.Snapshot);

            Assert.Equal(AppId, sut.Snapshot.AppId.Id);

            Assert.Equal(SchemaName, sut.Snapshot.SchemaDef.Name);
            Assert.Equal(SchemaType.Singleton, sut.Snapshot.SchemaDef.Type);

            LastEvents
                .ShouldHaveSameEvents(
                    CreateEvent(new SchemaCreated { Schema = new Schema(command.Name, command.Properties, SchemaType.Singleton) })
                );
        }

        [Fact]
        public async Task Create_should_create_events_and_schema_with_initial_fields()
        {
            var properties = new SchemaProperties();

            var fields = new[]
            {
                new UpsertSchemaField { Name = "field1", Properties = ValidProperties() },
                new UpsertSchemaField { Name = "field2", Properties = ValidProperties() },
                new UpsertSchemaField
                {
                    Name = "field3",
                    Partitioning = Partitioning.Language.Key,
                    Properties = new ArrayFieldProperties(),
                    Nested = new[]
                    {
                        new UpsertSchemaNestedField { Name = "nested1", Properties = ValidProperties() },
                        new UpsertSchemaNestedField { Name = "nested2", Properties = ValidProperties() }
                    }
                }
            };

            var command = new CreateSchema { Name = SchemaName, SchemaId = SchemaId, Properties = properties, Fields = fields };

            var result = await PublishAsync(command);

            result.ShouldBeEquivalent(sut.Snapshot);

            var @event = (SchemaCreated)LastEvents.Single().Payload;

            Assert.Equal(AppId, sut.Snapshot.AppId.Id);
            Assert.Equal(SchemaName, sut.Snapshot.SchemaDef.Name);
            Assert.Equal(SchemaName, sut.Snapshot.SchemaDef.Name);

            Assert.Equal(3, @event.Schema.Fields.Count);
        }

        [Fact]
        public async Task Update_should_create_events_and_update_schema_properties()
        {
            var command = new UpdateSchema { Properties = new SchemaProperties { Label = "My Properties" } };

            await ExecuteCreateAsync();

            var result = await PublishIdempotentAsync(command);

            result.ShouldBeEquivalent(sut.Snapshot);

            Assert.Equal(command.Properties, sut.Snapshot.SchemaDef.Properties);

            LastEvents
                .ShouldHaveSameEvents(
                    CreateEvent(new SchemaUpdated { Properties = command.Properties })
                );
        }

        [Fact]
        public async Task ConfigureScripts_should_create_events_and_update_schema_scripts()
        {
            var command = new ConfigureScripts
            {
                Scripts = new SchemaScripts
                {
                    Query = "<query-script>"
                }
            };

            await ExecuteCreateAsync();

            var result = await PublishIdempotentAsync(command);

            result.ShouldBeEquivalent(sut.Snapshot);

            Assert.Equal("<query-script>", sut.Snapshot.SchemaDef.Scripts.Query);

            LastEvents
                .ShouldHaveSameEvents(
                    CreateEvent(new SchemaScriptsConfigured { Scripts = command.Scripts })
                );
        }

        [Fact]
        public async Task ConfigureFieldRules_should_create_events_and_update_schema_field_rules()
        {
            var command = new ConfigureFieldRules
            {
                FieldRules = new[]
                {
                    new FieldRuleCommand { Field = "field1" }
                }
            };

            await ExecuteCreateAsync();

            var result = await PublishIdempotentAsync(command);

            result.ShouldBeEquivalent(sut.Snapshot);

            Assert.NotEmpty(sut.Snapshot.SchemaDef.FieldRules);

            LastEvents
                .ShouldHaveSameEvents(
                    CreateEvent(new SchemaFieldRulesConfigured { FieldRules = FieldRules.Create(FieldRule.Disable("field1")) })
                );
        }

        [Fact]
        public async Task ConfigureUIFields_should_create_events_for_list_fields_and_update_schema()
        {
            var command = new ConfigureUIFields
            {
                FieldsInLists = FieldNames.Create(fieldName)
            };

            await ExecuteCreateAsync();
            await ExecuteAddFieldAsync(fieldName);

            var result = await PublishIdempotentAsync(command);

            result.ShouldBeEquivalent(sut.Snapshot);

            Assert.Equal(command.FieldsInLists, sut.Snapshot.SchemaDef.FieldsInLists);

            LastEvents
                .ShouldHaveSameEvents(
                    CreateEvent(new SchemaUIFieldsConfigured { FieldsInLists = command.FieldsInLists })
                );
        }

        [Fact]
        public async Task ConfigureUIFields_should_create_events_for_reference_fields_and_update_schema()
        {
            var command = new ConfigureUIFields
            {
                FieldsInReferences = FieldNames.Create(fieldName)
            };

            await ExecuteCreateAsync();
            await ExecuteAddFieldAsync(fieldName);

            var result = await PublishIdempotentAsync(command);

            result.ShouldBeEquivalent(sut.Snapshot);

            Assert.Equal(command.FieldsInReferences, sut.Snapshot.SchemaDef.FieldsInReferences);

            LastEvents
                .ShouldHaveSameEvents(
                    CreateEvent(new SchemaUIFieldsConfigured { FieldsInReferences = command.FieldsInReferences })
                );
        }

        [Fact]
        public async Task Publish_should_create_events_and_update_published_flag()
        {
            var command = new PublishSchema();

            await ExecuteCreateAsync();

            var result = await PublishIdempotentAsync(command);

            result.ShouldBeEquivalent(sut.Snapshot);

            Assert.True(sut.Snapshot.SchemaDef.IsPublished);

            LastEvents
                .ShouldHaveSameEvents(
                    CreateEvent(new SchemaPublished())
                );
        }

        [Fact]
        public async Task Unpublish_should_create_events_and_update_published_flag()
        {
            var command = new UnpublishSchema();

            await ExecuteCreateAsync();
            await ExecutePublishAsync();

            var result = await PublishIdempotentAsync(command);

            result.ShouldBeEquivalent(sut.Snapshot);

            Assert.False(sut.Snapshot.SchemaDef.IsPublished);

            LastEvents
                .ShouldHaveSameEvents(
                    CreateEvent(new SchemaUnpublished())
                );
        }

        [Fact]
        public async Task ChangeCategory_should_create_events_and_update_category()
        {
            var command = new ChangeCategory { Name = "my-category" };

            await ExecuteCreateAsync();

            var result = await PublishIdempotentAsync(command);

            result.ShouldBeEquivalent(sut.Snapshot);

            Assert.Equal(command.Name, sut.Snapshot.SchemaDef.Category);

            LastEvents
                .ShouldHaveSameEvents(
                    CreateEvent(new SchemaCategoryChanged { Name = command.Name })
                );
        }

        [Fact]
        public async Task ConfigurePreviewUrls_should_create_events_and_update_preview_urls()
        {
            var command = new ConfigurePreviewUrls
            {
                PreviewUrls = new Dictionary<string, string>
                {
                    ["Web"] = "web-url"
                }.ToImmutableDictionary()
            };

            await ExecuteCreateAsync();

            var result = await PublishIdempotentAsync(command);

            result.ShouldBeEquivalent(sut.Snapshot);

            Assert.Equal(command.PreviewUrls, sut.Snapshot.SchemaDef.PreviewUrls);

            LastEvents
                .ShouldHaveSameEvents(
                    CreateEvent(new SchemaPreviewUrlsConfigured { PreviewUrls = command.PreviewUrls })
                );
        }

        [Fact]
        public async Task Delete_should_create_events_and_update_deleted_flag()
        {
            var command = new DeleteSchema();

            await ExecuteCreateAsync();

            var result = await PublishAsync(command);

            result.ShouldBeEquivalent(None.Value);

            Assert.True(sut.Snapshot.IsDeleted);

            LastEvents
                .ShouldHaveSameEvents(
                    CreateEvent(new SchemaDeleted())
                );
        }

        [Fact]
        public async Task Reorder_should_create_events_and_reorder_fields()
        {
            var command = new ReorderFields { FieldIds = new[] { 2L, 1L } };

            await ExecuteCreateAsync();
            await ExecuteAddFieldAsync("field1");
            await ExecuteAddFieldAsync("field2");

            var result = await PublishIdempotentAsync(command);

            result.ShouldBeEquivalent(sut.Snapshot);

            LastEvents
                .ShouldHaveSameEvents(
                    CreateEvent(new SchemaFieldsReordered { FieldIds = command.FieldIds })
                );
        }

        [Fact]
        public async Task Reorder_should_create_events_and_reorder_nestedy_fields()
        {
            var command = new ReorderFields { ParentFieldId = 1, FieldIds = new[] { 3L, 2L } };

            await ExecuteCreateAsync();
            await ExecuteAddArrayFieldAsync();
            await ExecuteAddFieldAsync("field1", 1);
            await ExecuteAddFieldAsync("field2", 1);

            var result = await PublishIdempotentAsync(command);

            result.ShouldBeEquivalent(sut.Snapshot);

            LastEvents
                .ShouldHaveSameEvents(
                    CreateEvent(new SchemaFieldsReordered { ParentFieldId = arrayId, FieldIds = command.FieldIds })
                );
        }

        [Fact]
        public async Task Add_should_create_events_and_add_field()
        {
            var command = new AddField { Name = fieldName, Properties = ValidProperties() };

            await ExecuteCreateAsync();

            var result = await PublishAsync(command);

            result.ShouldBeEquivalent(sut.Snapshot);

            Assert.Equal(command.Properties, GetField(1).RawProperties);

            LastEvents
                .ShouldHaveSameEvents(
                    CreateEvent(new FieldAdded { Name = fieldName, FieldId = fieldId, Properties = command.Properties })
                );
        }

        [Fact]
        public async Task Add_should_create_events_and_add_field_to_array()
        {
            var command = new AddField { ParentFieldId = 1, Name = fieldName, Properties = ValidProperties() };

            await ExecuteCreateAsync();
            await ExecuteAddArrayFieldAsync();

            var result = await PublishAsync(command);

            result.ShouldBeEquivalent(sut.Snapshot);

            Assert.Same(command.Properties, GetNestedField(1, 2).RawProperties);

            LastEvents
                .ShouldHaveSameEvents(
                    CreateEvent(new FieldAdded { ParentFieldId = arrayId, Name = fieldName, FieldId = nestedId, Properties = command.Properties })
                );
        }

        [Fact]
        public async Task UpdateField_should_create_events_and_update_field_properties()
        {
            var command = new UpdateField { FieldId = 1, Properties = new StringFieldProperties() };

            await ExecuteCreateAsync();
            await ExecuteAddFieldAsync(fieldName);

            var result = await PublishIdempotentAsync(command);

            result.ShouldBeEquivalent(sut.Snapshot);

            Assert.Equal(command.Properties, GetField(1).RawProperties);

            LastEvents
                .ShouldHaveSameEvents(
                    CreateEvent(new FieldUpdated { FieldId = fieldId, Properties = command.Properties })
                );
        }

        [Fact]
        public async Task UpdateField_should_create_events_and_update_nested_field_properties()
        {
            var command = new UpdateField { ParentFieldId = 1, FieldId = 2, Properties = new StringFieldProperties() };

            await ExecuteCreateAsync();
            await ExecuteAddArrayFieldAsync();
            await ExecuteAddFieldAsync(fieldName, 1);

            var result = await PublishIdempotentAsync(command);

            result.ShouldBeEquivalent(sut.Snapshot);

            Assert.Same(command.Properties, GetNestedField(1, 2).RawProperties);

            LastEvents
                .ShouldHaveSameEvents(
                    CreateEvent(new FieldUpdated { ParentFieldId = arrayId, FieldId = nestedId, Properties = command.Properties })
                );
        }

        [Fact]
        public async Task LockField_should_create_events_and_update_field_locked_flag()
        {
            var command = new LockField { FieldId = 1 };

            await ExecuteCreateAsync();
            await ExecuteAddFieldAsync(fieldName);

            var result = await PublishIdempotentAsync(command);

            result.ShouldBeEquivalent(sut.Snapshot);

            Assert.True(GetField(1).IsLocked);

            LastEvents
                .ShouldHaveSameEvents(
                    CreateEvent(new FieldLocked { FieldId = fieldId })
                );
        }

        [Fact]
        public async Task LockField_should_create_events_and_update_nested_field_locked_flag()
        {
            var command = new LockField { ParentFieldId = 1, FieldId = 2 };

            await ExecuteCreateAsync();
            await ExecuteAddArrayFieldAsync();
            await ExecuteAddFieldAsync(fieldName, 1);

            var result = await PublishIdempotentAsync(command);

            result.ShouldBeEquivalent(sut.Snapshot);

            Assert.True(GetNestedField(1, 2).IsLocked);

            LastEvents
                .ShouldHaveSameEvents(
                    CreateEvent(new FieldLocked { ParentFieldId = arrayId, FieldId = nestedId })
                );
        }

        [Fact]
        public async Task HideField_should_create_events_and_update_field_hidden_flag()
        {
            var command = new HideField { FieldId = 1 };

            await ExecuteCreateAsync();
            await ExecuteAddFieldAsync(fieldName);

            var result = await PublishIdempotentAsync(command);

            result.ShouldBeEquivalent(sut.Snapshot);

            Assert.True(GetField(1).IsHidden);

            LastEvents
                .ShouldHaveSameEvents(
                    CreateEvent(new FieldHidden { FieldId = fieldId })
                );
        }

        [Fact]
        public async Task HideField_should_create_events_and_update_nested_field_hidden_flag()
        {
            var command = new HideField { ParentFieldId = 1, FieldId = 2 };

            await ExecuteCreateAsync();
            await ExecuteAddArrayFieldAsync();
            await ExecuteAddFieldAsync(fieldName, 1);

            var result = await PublishIdempotentAsync(command);

            result.ShouldBeEquivalent(sut.Snapshot);

            Assert.True(GetNestedField(1, 2).IsHidden);

            LastEvents
                .ShouldHaveSameEvents(
                    CreateEvent(new FieldHidden { ParentFieldId = arrayId, FieldId = nestedId })
                );
        }

        [Fact]
        public async Task ShowField_should_create_events_and_update_field_hidden_flag()
        {
            var command = new ShowField { FieldId = 1 };

            await ExecuteCreateAsync();
            await ExecuteAddFieldAsync(fieldName);
            await ExecuteHideFieldAsync(1);

            var result = await PublishIdempotentAsync(command);

            result.ShouldBeEquivalent(sut.Snapshot);

            Assert.False(GetField(1).IsHidden);

            LastEvents
                .ShouldHaveSameEvents(
                    CreateEvent(new FieldShown { FieldId = fieldId })
                );
        }

        [Fact]
        public async Task ShowField_should_create_events_and_update_nested_field_hidden_flag()
        {
            var command = new ShowField { ParentFieldId = 1, FieldId = 2 };

            await ExecuteCreateAsync();
            await ExecuteAddArrayFieldAsync();
            await ExecuteAddFieldAsync(fieldName, 1);
            await ExecuteHideFieldAsync(2, 1);

            var result = await PublishIdempotentAsync(command);

            result.ShouldBeEquivalent(sut.Snapshot);

            Assert.False(GetNestedField(1, 2).IsHidden);

            LastEvents
                .ShouldHaveSameEvents(
                    CreateEvent(new FieldShown { ParentFieldId = arrayId, FieldId = nestedId })
                );
        }

        [Fact]
        public async Task DisableField_should_create_events_and_update_field_disabled_flag()
        {
            var command = new DisableField { FieldId = 1 };

            await ExecuteCreateAsync();
            await ExecuteAddFieldAsync(fieldName);

            var result = await PublishIdempotentAsync(command);

            result.ShouldBeEquivalent(sut.Snapshot);

            Assert.True(GetField(1).IsDisabled);

            LastEvents
                .ShouldHaveSameEvents(
                    CreateEvent(new FieldDisabled { FieldId = fieldId })
                );
        }

        [Fact]
        public async Task DisableField_should_create_events_and_update_nested_field_disabled_flag()
        {
            var command = new DisableField { ParentFieldId = 1, FieldId = 2 };

            await ExecuteCreateAsync();
            await ExecuteAddArrayFieldAsync();
            await ExecuteAddFieldAsync(fieldName, 1);

            var result = await PublishIdempotentAsync(command);

            result.ShouldBeEquivalent(sut.Snapshot);

            Assert.True(GetNestedField(1, 2).IsDisabled);

            LastEvents
                .ShouldHaveSameEvents(
                    CreateEvent(new FieldDisabled { ParentFieldId = arrayId, FieldId = nestedId })
                );
        }

        [Fact]
        public async Task EnableField_should_create_events_and_update_field_disabled_flag()
        {
            var command = new EnableField { FieldId = 1 };

            await ExecuteCreateAsync();
            await ExecuteAddFieldAsync(fieldName);
            await ExecuteDisableFieldAsync(1);

            var result = await PublishIdempotentAsync(command);

            result.ShouldBeEquivalent(sut.Snapshot);

            Assert.False(GetField(1).IsDisabled);

            LastEvents
                .ShouldHaveSameEvents(
                    CreateEvent(new FieldEnabled { FieldId = fieldId })
                );
        }

        [Fact]
        public async Task EnableField_should_create_events_and_update_nested_field_disabled_flag()
        {
            var command = new EnableField { ParentFieldId = 1, FieldId = 2 };

            await ExecuteCreateAsync();
            await ExecuteAddArrayFieldAsync();
            await ExecuteAddFieldAsync(fieldName, 1);
            await ExecuteDisableFieldAsync(2, 1);

            var result = await PublishAsync(command);

            result.ShouldBeEquivalent(sut.Snapshot);

            Assert.False(GetNestedField(1, 2).IsDisabled);

            LastEvents
                .ShouldHaveSameEvents(
                    CreateEvent(new FieldEnabled { ParentFieldId = arrayId, FieldId = nestedId })
                );
        }

        [Fact]
        public async Task DeleteField_should_create_events_and_delete_field()
        {
            var command = new DeleteField { FieldId = 1 };

            await ExecuteCreateAsync();
            await ExecuteAddFieldAsync(fieldName);

            var result = await PublishAsync(command);

            result.ShouldBeEquivalent(sut.Snapshot);

            Assert.Null(GetField(1));

            LastEvents
                .ShouldHaveSameEvents(
                    CreateEvent(new FieldDeleted { FieldId = fieldId })
                );
        }

        [Fact]
        public async Task DeleteField_should_create_events_and_delete_nested_field()
        {
            var command = new DeleteField { ParentFieldId = 1, FieldId = 2 };

            await ExecuteCreateAsync();
            await ExecuteAddArrayFieldAsync();
            await ExecuteAddFieldAsync(fieldName, 1);

            var result = await PublishAsync(command);

            result.ShouldBeEquivalent(sut.Snapshot);

            Assert.Null(GetNestedField(1, 2));

            LastEvents
                .ShouldHaveSameEvents(
                    CreateEvent(new FieldDeleted { ParentFieldId = arrayId, FieldId = nestedId })
                );
        }

        [Fact]
        public async Task Synchronize_should_create_events_and_update_schema()
        {
            var command = new SynchronizeSchema
            {
                Category = "My-Category"
            };

            await ExecuteCreateAsync();

            var result = await PublishIdempotentAsync(command);

            result.ShouldBeEquivalent(sut.Snapshot);

            Assert.Equal(command.Category, sut.Snapshot.SchemaDef.Category);

            LastEvents
                .ShouldHaveSameEvents(
                    CreateEvent(new SchemaCategoryChanged { Name = command.Category })
                );
        }

        private Task ExecuteCreateAsync()
        {
            return PublishAsync(new CreateSchema { Name = SchemaName, SchemaId = SchemaId });
        }

        private Task ExecuteAddArrayFieldAsync()
        {
            return PublishAsync(new AddField { Properties = new ArrayFieldProperties(), Name = arrayName });
        }

        private Task ExecuteAddFieldAsync(string name, long? parentId = null)
        {
            return PublishAsync(new AddField { ParentFieldId = parentId, Properties = ValidProperties(), Name = name });
        }

        private Task ExecuteHideFieldAsync(long id, long? parentId = null)
        {
            return PublishAsync(new HideField { ParentFieldId = parentId, FieldId = id });
        }

        private Task ExecuteDisableFieldAsync(long id, long? parentId = null)
        {
            return PublishAsync(new DisableField { ParentFieldId = parentId, FieldId = id });
        }

        private Task ExecutePublishAsync()
        {
            return PublishAsync(new PublishSchema());
        }

        private Task ExecuteDeleteAsync()
        {
            return PublishAsync(new DeleteSchema());
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

        private async Task<object?> PublishIdempotentAsync<T>(T command) where T : SquidexCommand, IAggregateCommand
        {
            var result = await PublishAsync(command);

            var previousSnapshot = sut.Snapshot;
            var previousVersion = sut.Snapshot.Version;

            await PublishAsync(command);

            Assert.Same(previousSnapshot, sut.Snapshot);
            Assert.Equal(previousVersion, sut.Snapshot.Version);

            return result;
        }

        private async Task<object> PublishAsync<T>(T command) where T : SquidexCommand, IAggregateCommand
        {
            var result = await sut.ExecuteAsync(CreateCommand(command));

            return result.Payload;
        }
    }
}
