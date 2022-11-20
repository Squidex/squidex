// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Domain.Apps.Core.Schemas;
using Squidex.Domain.Apps.Core.TestHelpers;
using Squidex.Infrastructure.Collections;

#pragma warning disable SA1310 // Field names must not contain underscore

namespace Squidex.Domain.Apps.Core.Model.Schemas;

public class SchemaTests
{
    private readonly Schema schema_0 = new Schema("my-schema");

    public static IEnumerable<object[]> FieldProperyTypes()
    {
        return typeof(Schema).Assembly.GetTypes().Where(x => x.BaseType == typeof(FieldProperties)).Select(x => new object[] { x });
    }

    [Theory]
    [MemberData(nameof(FieldProperyTypes))]
    public void Should_make_deep_equal_test(Type type)
    {
        var lhs = (FieldProperties)Activator.CreateInstance(type)!;
        var rhs = (FieldProperties)Activator.CreateInstance(type)!;

        Assert.True(lhs.Equals(rhs));
    }

    [Fact]
    public void Should_instantiate_schema()
    {
        Assert.Equal("my-schema", schema_0.Name);
    }

    [Fact]
    public void Should_throw_exception_if_creating_schema_with_invalid_name()
    {
        Assert.Throws<ArgumentException>(() => new Schema(string.Empty));
    }

    [Fact]
    public void Should_update_schema()
    {
        var properties1 = new SchemaProperties { Hints = "my-hint", Label = "my-label" };
        var properties2 = new SchemaProperties { Hints = "my-hint", Label = "my-label" };

        var schema_1 = schema_0.Update(properties1);
        var schema_2 = schema_1.Update(properties2);

        Assert.NotSame(properties1, schema_0.Properties);
        Assert.Same(properties1, schema_1.Properties);
        Assert.Same(properties1, schema_2.Properties);
        Assert.Same(schema_1, schema_2);
    }

    [Fact]
    public void Should_add_field()
    {
        var field = CreateField(1);

        var schema_1 = schema_0.AddField(field);

        Assert.Empty(schema_0.Fields);
        Assert.Equal(field, schema_1.FieldsById[1]);
    }

    [Fact]
    public void Should_throw_exception_if_adding_field_with_name_that_already_exists()
    {
        var schema_1 = schema_0.AddField(CreateField(1));

        Assert.Throws<ArgumentException>(() => schema_1.AddNumber(2, "myField1", Partitioning.Invariant));
    }

    [Fact]
    public void Should_throw_exception_if_adding_field_with_id_that_already_exists()
    {
        var schema_1 = schema_0.AddField(CreateField(1));

        Assert.Throws<ArgumentException>(() => schema_1.AddNumber(1, "myField2", Partitioning.Invariant));
    }

    [Fact]
    public void Should_hide_field()
    {
        var schema_1 = schema_0.AddField(CreateField(1));

        var schema_2 = schema_1.UpdateField(1, f => f.Hide());
        var schema_3 = schema_2.UpdateField(1, f => f.Hide());

        Assert.False(schema_1.FieldsById[1].IsHidden);
        Assert.True(schema_2.FieldsById[1].IsHidden);
        Assert.True(schema_3.FieldsById[1].IsHidden);
        Assert.Same(schema_2, schema_3);
    }

    [Fact]
    public void Should_return_same_schema_if_field_to_hide_does_not_exist()
    {
        var schema_1 = schema_0.UpdateField(1, f => f.Hide());

        Assert.Same(schema_0, schema_1);
    }

    [Fact]
    public void Should_show_field()
    {
        var schema_1 = schema_0.AddField(CreateField(1));

        var schema_2 = schema_1.UpdateField(1, f => f.Hide());
        var schema_3 = schema_2.UpdateField(1, f => f.Show());
        var schema_4 = schema_3.UpdateField(1, f => f.Show());

        Assert.True(schema_2.FieldsById[1].IsHidden);
        Assert.False(schema_3.FieldsById[1].IsHidden);
        Assert.False(schema_4.FieldsById[1].IsHidden);
        Assert.Same(schema_3, schema_4);
    }

    [Fact]
    public void Should_return_same_schema_if_field_to_show_does_not_exist()
    {
        var schema_1 = schema_0.UpdateField(1, f => f.Show());

        Assert.Same(schema_0, schema_1);
    }

    [Fact]
    public void Should_disable_field()
    {
        var schema_1 = schema_0.AddField(CreateField(1));

        var schema_2 = schema_1.UpdateField(1, f => f.Disable());
        var schema_3 = schema_2.UpdateField(1, f => f.Disable());

        Assert.False(schema_1.FieldsById[1].IsDisabled);
        Assert.True(schema_2.FieldsById[1].IsDisabled);
        Assert.True(schema_3.FieldsById[1].IsDisabled);
        Assert.Same(schema_2, schema_3);
    }

    [Fact]
    public void Should_return_same_schema_if_field_to_disable_does_not_exist()
    {
        var schema_1 = schema_0.UpdateField(1, f => f.Disable());

        Assert.Same(schema_0, schema_1);
    }

    [Fact]
    public void Should_enable_field()
    {
        var schema_1 = schema_0.AddField(CreateField(1));

        var schema_2 = schema_1.UpdateField(1, f => f.Disable());
        var schema_3 = schema_2.UpdateField(1, f => f.Enable());
        var schema_4 = schema_3.UpdateField(1, f => f.Enable());

        Assert.True(schema_2.FieldsById[1].IsDisabled);
        Assert.False(schema_3.FieldsById[1].IsDisabled);
        Assert.False(schema_4.FieldsById[1].IsDisabled);
        Assert.Same(schema_3, schema_4);
    }

    [Fact]
    public void Should_return_same_schema_if_field_to_enable_does_not_exist()
    {
        var schema_1 = schema_0.UpdateField(1, f => f.Enable());

        Assert.Same(schema_0, schema_1);
    }

    [Fact]
    public void Should_lock_field()
    {
        var schema_1 = schema_0.AddField(CreateField(1));

        var schema_2 = schema_1.UpdateField(1, f => f.Lock());
        var schema_3 = schema_2.UpdateField(1, f => f.Lock());

        Assert.False(schema_1.FieldsById[1].IsLocked);
        Assert.True(schema_2.FieldsById[1].IsLocked);
        Assert.True(schema_3.FieldsById[1].IsLocked);
        Assert.Same(schema_2, schema_3);
    }

    [Fact]
    public void Should_return_same_schema_if_field_to_lock_does_not_exist()
    {
        var schema_1 = schema_0.UpdateField(1, f => f.Lock());

        Assert.Same(schema_0, schema_1);
    }

    [Fact]
    public void Should_update_field()
    {
        var properties1 = new NumberFieldProperties
        {
            MinValue = 10
        };
        var properties2 = new NumberFieldProperties
        {
            MinValue = 10
        };

        var schema_1 = schema_0.AddField(CreateField(1));
        var schema_2 = schema_1.UpdateField(1, f => f.Update(properties1));
        var schema_3 = schema_2.UpdateField(1, f => f.Update(properties2));

        Assert.NotSame(properties1, schema_1.FieldsById[1].RawProperties);
        Assert.Same(properties1, schema_2.FieldsById[1].RawProperties);
        Assert.Same(properties1, schema_3.FieldsById[1].RawProperties);
        Assert.Same(schema_2, schema_3);
    }

    [Fact]
    public void Should_throw_exception_if_updating_with_invalid_properties_type()
    {
        var schema_1 = schema_0.AddField(CreateField(1));

        Assert.Throws<ArgumentException>(() => schema_1.UpdateField(1, f => f.Update(new StringFieldProperties())));
    }

    [Fact]
    public void Should_return_same_schema_if_field_to_update_does_not_exist()
    {
        var schema_1 = schema_0.UpdateField(1, f => f.Update(new StringFieldProperties()));

        Assert.Same(schema_0, schema_1);
    }

    [Fact]
    public void Should_delete_field()
    {
        var schema_1 = schema_0.AddField(CreateField(1));
        var schema_2 = schema_1.DeleteField(1);
        var schema_3 = schema_2.DeleteField(1);

        Assert.Empty(schema_2.FieldsById);
        Assert.Empty(schema_3.FieldsById);
        Assert.Same(schema_2, schema_3);
    }

    [Fact]
    public void Should_also_remove_deleted_fields_from_lists()
    {
        var field = CreateField(1);

        var schema_1 = schema_0
            .AddField(field)
            .SetFieldsInLists(field.Name)
            .SetFieldsInReferences(field.Name);
        var schema_2 = schema_1.DeleteField(1);

        Assert.Empty(schema_2.FieldsById);
        Assert.Empty(schema_2.FieldsInLists);
        Assert.Empty(schema_2.FieldsInReferences);
    }

    [Fact]
    public void Should_return_same_schema_if_field_to_delete_does_not_exist()
    {
        var schema_1 = schema_0.DeleteField(1);

        Assert.Same(schema_0, schema_1);
    }

    [Fact]
    public void Should_publish_schema()
    {
        var schema_1 = schema_0.Publish();
        var schema_2 = schema_1.Publish();

        Assert.False(schema_0.IsPublished);
        Assert.True(schema_1.IsPublished);
        Assert.True(schema_2.IsPublished);
        Assert.Same(schema_1, schema_2);
    }

    [Fact]
    public void Should_unpublish_schema()
    {
        var schema_1 = schema_0.Publish();
        var schema_2 = schema_1.Unpublish();
        var schema_3 = schema_2.Unpublish();

        Assert.True(schema_1.IsPublished);
        Assert.False(schema_2.IsPublished);
        Assert.False(schema_3.IsPublished);
        Assert.Same(schema_2, schema_3);
    }

    [Fact]
    public void Should_reorder_fields()
    {
        var field1 = CreateField(1);
        var field2 = CreateField(2);
        var field3 = CreateField(3);

        var schema_1 = schema_0.AddField(field1);
        var schema_2 = schema_1.AddField(field2);
        var schema_3 = schema_2.AddField(field3);
        var schema_4 = schema_3.ReorderFields(new List<long> { 3, 2, 1 });
        var schema_5 = schema_4.ReorderFields(new List<long> { 3, 2, 1 });

        Assert.Equal(new List<RootField> { field3, field2, field1 }, schema_4.Fields.ToList());
        Assert.Equal(new List<RootField> { field3, field2, field1 }, schema_5.Fields.ToList());
        Assert.Same(schema_4, schema_5);
    }

    [Fact]
    public void Should_throw_exception_if_not_all_fields_are_covered_for_reordering()
    {
        var field1 = CreateField(1);
        var field2 = CreateField(2);

        var schema_1 = schema_0.AddField(field1);
        var schema_2 = schema_1.AddField(field2);

        Assert.Throws<ArgumentException>(() => schema_2.ReorderFields(new List<long> { 1 }));
    }

    [Fact]
    public void Should_throw_exception_if_field_to_reorder_does_not_exist()
    {
        var field1 = CreateField(1);
        var field2 = CreateField(2);

        var schema_1 = schema_0.AddField(field1);
        var schema_2 = schema_1.AddField(field2);

        Assert.Throws<ArgumentException>(() => schema_2.ReorderFields(new List<long> { 1, 4 }));
    }

    [Fact]
    public void Should_change_category()
    {
        var schema_1 = schema_0.ChangeCategory("Category");
        var schema_2 = schema_1.ChangeCategory("Category");

        Assert.Equal("Category", schema_1.Category);
        Assert.Equal("Category", schema_2.Category);
        Assert.Same(schema_1, schema_2);
    }

    [Fact]
    public void Should_set_list_fields()
    {
        var schema_1 = schema_0.SetFieldsInLists("2");
        var schema_2 = schema_1.SetFieldsInLists("2");

        Assert.Equal(new[] { "2" }, schema_1.FieldsInLists);
        Assert.Equal(new[] { "2" }, schema_2.FieldsInLists);
        Assert.Same(schema_1, schema_2);
    }

    [Fact]
    public void Should_also_set_list_fields_if_reordered()
    {
        var schema_1 = schema_0.SetFieldsInLists("2", "1");
        var schema_2 = schema_1.SetFieldsInLists("1", "2");

        Assert.Equal(new[] { "2", "1" }, schema_1.FieldsInLists);
        Assert.Equal(new[] { "1", "2" }, schema_2.FieldsInLists);
        Assert.NotSame(schema_1, schema_2);
    }

    [Fact]
    public void Should_set_reference_fields()
    {
        var schema_1 = schema_0.SetFieldsInReferences("2");
        var schema_2 = schema_1.SetFieldsInReferences("2");

        Assert.Equal(new[] { "2" }, schema_1.FieldsInReferences);
        Assert.Equal(new[] { "2" }, schema_2.FieldsInReferences);
        Assert.Same(schema_1, schema_2);
    }

    [Fact]
    public void Should_also_set_reference_fields_if_reordered()
    {
        var schema_1 = schema_0.SetFieldsInReferences("2", "1");
        var schema_2 = schema_1.SetFieldsInReferences("1", "2");

        Assert.Equal(new[] { "2", "1" }, schema_1.FieldsInReferences);
        Assert.Equal(new[] { "1", "2" }, schema_2.FieldsInReferences);
        Assert.NotSame(schema_1, schema_2);
    }

    [Fact]
    public void Should_set_field_rules()
    {
        var schema_1 = schema_0.SetFieldRules(FieldRule.Hide("2"));
        var schema_2 = schema_1.SetFieldRules(FieldRule.Hide("2"));

        Assert.NotEmpty(schema_1.FieldRules);
        Assert.NotEmpty(schema_2.FieldRules);
        Assert.Same(schema_1, schema_2);
    }

    [Fact]
    public void Should_set_scripts()
    {
        var scripts1 = new SchemaScripts
        {
            Query = "<query-script>"
        };
        var scripts2 = new SchemaScripts
        {
            Query = "<query-script>"
        };

        var schema_1 = schema_0.SetScripts(scripts1);
        var schema_2 = schema_1.SetScripts(scripts2);

        Assert.Equal("<query-script>", schema_1.Scripts.Query);

        Assert.Equal(scripts1, schema_1.Scripts);
        Assert.Equal(scripts1, schema_2.Scripts);
        Assert.Same(schema_1, schema_2);
    }

    [Fact]
    public void Should_set_preview_urls()
    {
        var urls1 = new Dictionary<string, string>
        {
            ["web"] = "Url"
        }.ToReadonlyDictionary();
        var urls2 = new Dictionary<string, string>
        {
            ["web"] = "Url"
        }.ToReadonlyDictionary();

        var schema_1 = schema_0.SetPreviewUrls(urls1);
        var schema_2 = schema_1.SetPreviewUrls(urls2);

        Assert.Equal("Url", schema_1.PreviewUrls["web"]);
        Assert.Equal(urls1, schema_1.PreviewUrls);
        Assert.Equal(urls1, schema_2.PreviewUrls);
        Assert.Same(schema_1, schema_2);
    }

    [Fact]
    public void Should_serialize_and_deserialize_schema()
    {
        var schemaSource =
            TestSchema.MixedSchema(SchemaType.Singleton).Schema
                .ChangeCategory("Category")
                .SetFieldRules(FieldRule.Hide("2"))
                .SetFieldsInLists("field2")
                .SetFieldsInReferences("field1")
                .SetScripts(new SchemaScripts
                {
                    Create = "<create-script>"
                })
                .SetPreviewUrls(new Dictionary<string, string>
                {
                    ["web"] = "Url"
                }.ToReadonlyDictionary());

        var schemaTarget = schemaSource.SerializeAndDeserialize();

        schemaTarget.Should().BeEquivalentTo(schemaSource);
    }

    [Fact]
    public void Should_deserialize_obsolete_isSingleton_property()
    {
        var schemaSource = new
        {
            name = "my-schema",
            isPublished = true,
            isSingleton = true
        };

        var expected =
            new Schema("my-schema", type: SchemaType.Singleton)
                .Publish();

        var schemaTarget = schemaSource.SerializeAndDeserialize<Schema, object>();

        schemaTarget.Should().BeEquivalentTo(expected);
    }

    private static RootField<NumberFieldProperties> CreateField(int id)
    {
        return Fields.Number(id, $"myField{id}", Partitioning.Invariant);
    }
}
