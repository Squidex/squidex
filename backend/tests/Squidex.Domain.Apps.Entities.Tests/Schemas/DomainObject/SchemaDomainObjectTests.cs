// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Microsoft.Extensions.Logging;
using Squidex.Domain.Apps.Core;
using Squidex.Domain.Apps.Core.Schemas;
using Squidex.Domain.Apps.Entities.Schemas.Commands;
using Squidex.Domain.Apps.Entities.TestHelpers;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Collections;

namespace Squidex.Domain.Apps.Entities.Schemas.DomainObject;

public class SchemaDomainObjectTests : HandlerTestBase<Schema>
{
    private readonly string fieldName = "age";
    private readonly string arrayName = "array";
    private readonly NamedId<long> fieldId = NamedId.Of(1L, "age");
    private readonly NamedId<long> arrayId = NamedId.Of(1L, "array");
    private readonly NamedId<long> nestedId = NamedId.Of(2L, "age");
    private readonly SchemaDomainObject sut;

    protected override DomainId Id
    {
        get => DomainId.Combine(AppId.Id, SchemaId.Id);
    }

    public SchemaDomainObjectTests()
    {
        var log = A.Fake<ILogger<SchemaDomainObject>>();

#pragma warning disable MA0056 // Do not call overridable members in constructor
        sut = new SchemaDomainObject(Id, PersistenceFactory, log);
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
        var command = new CreateSchema
        {
            Name = SchemaId.Name,
            SchemaId = SchemaId.Id,
            Scripts = null!,
            Properties = new SchemaProperties()
        };

        var actual = await PublishAsync(sut, command);

        await VerifySutAsync(actual);
    }

    [Fact]
    public async Task Create_should_create_events_and_schema_with_initial_fields()
    {
        var command = new CreateSchema
        {
            Name = SchemaId.Name,
            SchemaId = SchemaId.Id,
            Properties = new SchemaProperties(),
            Fields =
            [
                new UpsertSchemaField { Name = "field1", Properties = ValidProperties() },
                new UpsertSchemaField { Name = "field2", Properties = ValidProperties() },
                new UpsertSchemaField
                {
                    Name = "field3",
                    Partitioning = Partitioning.Language.Key,
                    Properties = new ArrayFieldProperties(),
                    Nested =
                    [
                        new UpsertSchemaNestedField { Name = "nested1", Properties = ValidProperties() },
                        new UpsertSchemaNestedField { Name = "nested2", Properties = ValidProperties() }
                    ]
                },
            ]
        };

        var actual = await PublishAsync(sut, command);

        await VerifySutAsync(actual);
    }

    [Fact]
    public async Task Update_should_create_events_and_update_schema_properties()
    {
        var command = new UpdateSchema
        {
            Properties = new SchemaProperties { Label = "My Properties" }
        };

        await ExecuteCreateAsync();

        var actual = await PublishIdempotentAsync(sut, command);

        await VerifySutAsync(actual);
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

        var actual = await PublishIdempotentAsync(sut, command);

        await VerifySutAsync(actual);
    }

    [Fact]
    public async Task ConfigureFieldRules_should_create_events_and_update_schema_field_rules()
    {
        var command = new ConfigureFieldRules
        {
            FieldRules =
            [
                new FieldRuleCommand { Field = "field1" }
            ]
        };

        await ExecuteCreateAsync();

        var actual = await PublishIdempotentAsync(sut, command);

        await VerifySutAsync(actual);
    }

    [Fact]
    public async Task ConfigureUIFields_should_create_events_for_list_fields_and_update_schema()
    {
        var command = new ConfigureUIFields
        {
            FieldsInLists = FieldNames.Create($"data.{fieldName}")
        };

        await ExecuteCreateAsync();
        await ExecuteAddFieldAsync(fieldName);

        var actual = await PublishIdempotentAsync(sut, command);

        await VerifySutAsync(actual);
    }

    [Fact]
    public async Task ConfigureUIFields_should_create_events_for_reference_fields_and_update_schema()
    {
        var command = new ConfigureUIFields
        {
            FieldsInReferences = FieldNames.Create($"data.{fieldName}")
        };

        await ExecuteCreateAsync();
        await ExecuteAddFieldAsync(fieldName);

        var actual = await PublishIdempotentAsync(sut, command);

        await VerifySutAsync(actual);
    }

    [Fact]
    public async Task Publish_should_create_events_and_update_published_flag()
    {
        var command = new PublishSchema();

        await ExecuteCreateAsync();

        var actual = await PublishIdempotentAsync(sut, command);

        await VerifySutAsync(actual);
    }

    [Fact]
    public async Task Unpublish_should_create_events_and_update_published_flag()
    {
        var command = new UnpublishSchema();

        await ExecuteCreateAsync();
        await ExecutePublishAsync();

        var actual = await PublishIdempotentAsync(sut, command);

        await VerifySutAsync(actual);
    }

    [Fact]
    public async Task ChangeCategory_should_create_events_and_update_category()
    {
        var command = new ChangeCategory { Name = "my-category" };

        await ExecuteCreateAsync();

        var actual = await PublishIdempotentAsync(sut, command);

        await VerifySutAsync(actual);
    }

    [Fact]
    public async Task ConfigurePreviewUrls_should_create_events_and_update_preview_urls()
    {
        var command = new ConfigurePreviewUrls
        {
            PreviewUrls = new Dictionary<string, string>
            {
                ["Web"] = "web-url"
            }.ToReadonlyDictionary()
        };

        await ExecuteCreateAsync();

        var actual = await PublishIdempotentAsync(sut, command);

        await VerifySutAsync(actual);
    }

    [Fact]
    public async Task Delete_should_create_events_and_update_deleted_flag()
    {
        var command = new DeleteSchema();

        await ExecuteCreateAsync();

        var actual = await PublishAsync(sut, command);

        await VerifySutAsync(actual, None.Value);
    }

    [Fact]
    public async Task Reorder_should_create_events_and_reorder_fields()
    {
        var command = new ReorderFields
        {
            ParentFieldId = null,
            FieldIds = [2L, 1L]
        };

        await ExecuteCreateAsync();
        await ExecuteAddFieldAsync("field1");
        await ExecuteAddFieldAsync("field2");

        var actual = await PublishIdempotentAsync(sut, command);

        await VerifySutAsync(actual);
    }

    [Fact]
    public async Task Reorder_should_create_events_and_reorder_nestedy_fields()
    {
        var command = new ReorderFields
        {
            ParentFieldId = 1,
            FieldIds = [3L, 2L]
        };

        await ExecuteCreateAsync();
        await ExecuteAddArrayFieldAsync();
        await ExecuteAddFieldAsync("field1", 1);
        await ExecuteAddFieldAsync("field2", 1);

        var actual = await PublishIdempotentAsync(sut, command);

        await VerifySutAsync(actual);
    }

    [Fact]
    public async Task Add_should_create_events_and_add_field()
    {
        var command = new AddField
        {
            ParentFieldId = null,
            Name = fieldName,
            Properties = ValidProperties()
        };

        await ExecuteCreateAsync();

        var actual = await PublishAsync(sut, command);

        await VerifySutAsync(actual);
    }

    [Fact]
    public async Task Add_should_create_events_and_add_field_to_array()
    {
        var command = new AddField
        {
            ParentFieldId = 1,
            Name = fieldName,
            Properties = ValidProperties()
        };

        await ExecuteCreateAsync();
        await ExecuteAddArrayFieldAsync();

        var actual = await PublishAsync(sut, command);

        await VerifySutAsync(actual);
    }

    [Fact]
    public async Task UpdateField_should_create_events_and_update_field_properties()
    {
        var command = new UpdateField
        {
            ParentFieldId = null,
            FieldId = 1,
            Properties = new StringFieldProperties()
        };

        await ExecuteCreateAsync();
        await ExecuteAddFieldAsync(fieldName);

        var actual = await PublishIdempotentAsync(sut, command);

        await VerifySutAsync(actual);
    }

    [Fact]
    public async Task UpdateField_should_create_events_and_update_nested_field_properties()
    {
        var command = new UpdateField
        {
            ParentFieldId = 1,
            FieldId = 2,
            Properties = new StringFieldProperties()
        };

        await ExecuteCreateAsync();
        await ExecuteAddArrayFieldAsync();
        await ExecuteAddFieldAsync(fieldName, 1);

        var actual = await PublishIdempotentAsync(sut, command);

        await VerifySutAsync(actual);
    }

    [Fact]
    public async Task LockField_should_create_events_and_update_field_locked_flag()
    {
        var command = new LockField
        {
            ParentFieldId = null,
            FieldId = 1
        };

        await ExecuteCreateAsync();
        await ExecuteAddFieldAsync(fieldName);

        var actual = await PublishIdempotentAsync(sut, command);

        await VerifySutAsync(actual);
    }

    [Fact]
    public async Task LockField_should_create_events_and_update_nested_field_locked_flag()
    {
        var command = new LockField
        {
            ParentFieldId = 1,
            FieldId = 2
        };

        await ExecuteCreateAsync();
        await ExecuteAddArrayFieldAsync();
        await ExecuteAddFieldAsync(fieldName, 1);

        var actual = await PublishIdempotentAsync(sut, command);

        await VerifySutAsync(actual);
    }

    [Fact]
    public async Task HideField_should_create_events_and_update_field_hidden_flag()
    {
        var command = new HideField
        {
            ParentFieldId = null,
            FieldId = 1
        };

        await ExecuteCreateAsync();
        await ExecuteAddFieldAsync(fieldName);

        var actual = await PublishIdempotentAsync(sut, command);

        await VerifySutAsync(actual);
    }

    [Fact]
    public async Task HideField_should_create_events_and_update_nested_field_hidden_flag()
    {
        var command = new HideField
        {
            ParentFieldId = 1,
            FieldId = 2
        };

        await ExecuteCreateAsync();
        await ExecuteAddArrayFieldAsync();
        await ExecuteAddFieldAsync(fieldName, 1);

        var actual = await PublishIdempotentAsync(sut, command);

        await VerifySutAsync(actual);
    }

    [Fact]
    public async Task ShowField_should_create_events_and_update_field_hidden_flag()
    {
        var command = new ShowField
        {
            ParentFieldId = null,
            FieldId = 1
        };

        await ExecuteCreateAsync();
        await ExecuteAddFieldAsync(fieldName);
        await ExecuteHideFieldAsync(1);

        var actual = await PublishIdempotentAsync(sut, command);

        await VerifySutAsync(actual);
    }

    [Fact]
    public async Task ShowField_should_create_events_and_update_nested_field_hidden_flag()
    {
        var command = new ShowField
        {
            ParentFieldId = 1,
            FieldId = 2
        };

        await ExecuteCreateAsync();
        await ExecuteAddArrayFieldAsync();
        await ExecuteAddFieldAsync(fieldName, 1);
        await ExecuteHideFieldAsync(2, 1);

        var actual = await PublishIdempotentAsync(sut, command);

        await VerifySutAsync(actual);
    }

    [Fact]
    public async Task DisableField_should_create_events_and_update_field_disabled_flag()
    {
        var command = new DisableField
        {
            ParentFieldId = null,
            FieldId = 1
        };

        await ExecuteCreateAsync();
        await ExecuteAddFieldAsync(fieldName);

        var actual = await PublishIdempotentAsync(sut, command);

        await VerifySutAsync(actual);
    }

    [Fact]
    public async Task DisableField_should_create_events_and_update_nested_field_disabled_flag()
    {
        var command = new DisableField
        {
            ParentFieldId = 1,
            FieldId = 2
        };

        await ExecuteCreateAsync();
        await ExecuteAddArrayFieldAsync();
        await ExecuteAddFieldAsync(fieldName, 1);

        var actual = await PublishIdempotentAsync(sut, command);

        await VerifySutAsync(actual);
    }

    [Fact]
    public async Task EnableField_should_create_events_and_update_field_disabled_flag()
    {
        var command = new EnableField
        {
            ParentFieldId = null,
            FieldId = 1
        };

        await ExecuteCreateAsync();
        await ExecuteAddFieldAsync(fieldName);
        await ExecuteDisableFieldAsync(1);

        var actual = await PublishIdempotentAsync(sut, command);

        await VerifySutAsync(actual);
    }

    [Fact]
    public async Task EnableField_should_create_events_and_update_nested_field_disabled_flag()
    {
        var command = new EnableField
        {
            ParentFieldId = 1,
            FieldId = 2
        };

        await ExecuteCreateAsync();
        await ExecuteAddArrayFieldAsync();
        await ExecuteAddFieldAsync(fieldName, 1);
        await ExecuteDisableFieldAsync(2, 1);

        var actual = await PublishAsync(sut, command);

        await VerifySutAsync(actual);
    }

    [Fact]
    public async Task DeleteField_should_create_events_and_delete_field()
    {
        var command = new DeleteField
        {
            ParentFieldId = null,
            FieldId = 1
        };

        await ExecuteCreateAsync();
        await ExecuteAddFieldAsync(fieldName);

        var actual = await PublishAsync(sut, command);

        await VerifySutAsync(actual);
    }

    [Fact]
    public async Task DeleteField_should_create_events_and_delete_nested_field()
    {
        var command = new DeleteField
        {
            ParentFieldId = 1,
            FieldId = 2
        };

        await ExecuteCreateAsync();
        await ExecuteAddArrayFieldAsync();
        await ExecuteAddFieldAsync(fieldName, 1);

        var actual = await PublishAsync(sut, command);

        await VerifySutAsync(actual);
    }

    [Fact]
    public async Task Synchronize_should_create_events_and_update_schema()
    {
        var command = new SynchronizeSchema
        {
            Category = "My-Category"
        };

        await ExecuteCreateAsync();

        var actual = await PublishIdempotentAsync(sut, command);

        await VerifySutAsync(actual);
    }

    private Task ExecuteCreateAsync()
    {
        return PublishAsync(sut, new CreateSchema { Name = SchemaId.Name, SchemaId = SchemaId.Id });
    }

    private Task ExecuteAddArrayFieldAsync()
    {
        return PublishAsync(sut, new AddField { Properties = new ArrayFieldProperties(), Name = arrayName });
    }

    private Task ExecuteAddFieldAsync(string name, long? parentId = null)
    {
        return PublishAsync(sut, new AddField { ParentFieldId = parentId, Properties = ValidProperties(), Name = name });
    }

    private Task ExecuteHideFieldAsync(long id, long? parentId = null)
    {
        return PublishAsync(sut, new HideField { ParentFieldId = parentId, FieldId = id });
    }

    private Task ExecuteDisableFieldAsync(long id, long? parentId = null)
    {
        return PublishAsync(sut, new DisableField { ParentFieldId = parentId, FieldId = id });
    }

    private Task ExecutePublishAsync()
    {
        return PublishAsync(sut, new PublishSchema());
    }

    private Task ExecuteDeleteAsync()
    {
        return PublishAsync(sut, new DeleteSchema());
    }

    private static StringFieldProperties ValidProperties()
    {
        return new StringFieldProperties { MinLength = 10, MaxLength = 20 };
    }

    private async Task VerifySutAsync(object? actual, object? expected = null)
    {
        if (expected == null)
        {
            actual.Should().BeEquivalentTo(sut.Snapshot, o => o.IncludingProperties());
        }
        else
        {
            actual.Should().BeEquivalentTo(expected);
        }

        Assert.Equal(AppId, sut.Snapshot.AppId);

        await Verify(new { sut, events = LastEvents });
    }
}
