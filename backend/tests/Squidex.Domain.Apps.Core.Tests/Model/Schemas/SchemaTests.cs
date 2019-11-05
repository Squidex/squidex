﻿// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using Squidex.Domain.Apps.Core.Schemas;
using Xunit;

#pragma warning disable SA1310 // Field names must not contain underscore

namespace Squidex.Domain.Apps.Core.Model.Schemas
{
    public class SchemaTests
    {
        private readonly Schema schema_0 = new Schema("my-schema");

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
            var properties = new SchemaProperties { Hints = "my-hint", Label = "my-label" };

            var schema_1 = schema_0.Update(properties);

            Assert.NotSame(properties, schema_0.Properties);
            Assert.Same(properties, schema_1.Properties);
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

            Assert.Throws<ArgumentException>(() => schema_1.AddNumber(2, "my-field-1", Partitioning.Invariant));
        }

        [Fact]
        public void Should_throw_exception_if_adding_field_with_id_that_already_exists()
        {
            var schema_1 = schema_0.AddField(CreateField(1));

            Assert.Throws<ArgumentException>(() => schema_1.AddNumber(1, "my-field-2", Partitioning.Invariant));
        }

        [Fact]
        public void Should_hide_field()
        {
            var schema_1 = schema_0.AddField(CreateField(1));

            var schema_2 = schema_1.UpdateField(1, f => f.Hide());
            var schema_3 = schema_2.UpdateField(1, f => f.Hide());

            Assert.False(schema_1.FieldsById[1].IsHidden);
            Assert.True(schema_3.FieldsById[1].IsHidden);
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
            Assert.False(schema_4.FieldsById[1].IsHidden);
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
            Assert.True(schema_3.FieldsById[1].IsDisabled);
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
            Assert.False(schema_4.FieldsById[1].IsDisabled);
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
            Assert.True(schema_3.FieldsById[1].IsLocked);
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
            var properties = new NumberFieldProperties();

            var schema_1 = schema_0.AddField(CreateField(1));
            var schema_2 = schema_1.UpdateField(1, f => f.Update(properties));

            Assert.NotSame(properties, schema_1.FieldsById[1].RawProperties);
            Assert.Same(properties, schema_2.FieldsById[1].RawProperties);
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

            Assert.Empty(schema_2.FieldsById);
        }

        [Fact]
        public void Should_also_remove_deleted_fields_from_lists()
        {
            var field = CreateField(1);

            var schema_1 = schema_0
                .AddField(field)
                .ConfigureFieldsInLists(field.Name)
                .ConfigureFieldsInReferences(field.Name);
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

            Assert.False(schema_0.IsPublished);
            Assert.True(schema_1.IsPublished);
        }

        [Fact]
        public void Should_unpublish_schema()
        {
            var schema_1 = schema_0.Publish();
            var schema_2 = schema_1.Unpublish();

            Assert.True(schema_1.IsPublished);
            Assert.False(schema_2.IsPublished);
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

            Assert.Equal(new List<RootField> { field3, field2, field1 }, schema_4.Fields.ToList());
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

            Assert.Equal("Category", schema_1.Category);
        }

        [Fact]
        public void Should_set_list_fields()
        {
            var schema_1 = schema_0.ConfigureFieldsInLists("1");

            Assert.Equal(new[] { "1" }, schema_1.FieldsInLists);
        }

        [Fact]
        public void Should_set_reference_fields()
        {
            var schema_1 = schema_0.ConfigureFieldsInReferences("2");

            Assert.Equal(new[] { "2" }, schema_1.FieldsInReferences);
        }

        [Fact]
        public void Should_configure_scripts()
        {
            var scripts = new SchemaScripts
            {
                Query = "<query-script>"
            };

            var schema_1 = schema_0.ConfigureScripts(scripts);

            Assert.Equal(scripts, schema_1.Scripts);

            Assert.Equal("<query-script>", schema_1.Scripts.Query);
        }

        [Fact]
        public void Should_configure_preview_urls()
        {
            var urls = new Dictionary<string, string>
            {
                ["web"] = "Url"
            };

            var schema_1 = schema_0.ConfigurePreviewUrls(urls);

            Assert.Equal(urls, schema_1.PreviewUrls);

            Assert.Equal("Url", schema_1.PreviewUrls["web"]);
        }

        [Fact]
        public void Should_serialize_and_deserialize_schema()
        {
            var schemaSource =
                TestUtils.MixedSchema(true)
                    .ChangeCategory("Category")
                    .ConfigureFieldsInLists("field2")
                    .ConfigureFieldsInReferences("field1")
                    .ConfigurePreviewUrls(new Dictionary<string, string>
                    {
                        ["web"] = "Url"
                    })
                    .ConfigureScripts(new SchemaScripts
                    {
                        Create = "<create-script>"
                    });

            var schemaTarget = schemaSource.SerializeAndDeserialize();

            schemaTarget.Should().BeEquivalentTo(schemaSource);
        }

        private static RootField<NumberFieldProperties> CreateField(int id)
        {
            return Fields.Number(id, $"my-field-{id}", Partitioning.Invariant);
        }
    }
}
