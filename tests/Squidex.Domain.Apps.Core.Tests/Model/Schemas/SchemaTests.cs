// ==========================================================================
//  SchemaTests.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System;
using System.Collections.Generic;
using System.Linq;
using Squidex.Domain.Apps.Core.Schemas;
using Xunit;

namespace Squidex.Domain.Apps.Core.Model.Schemas
{
    public class SchemaTests
    {
        private readonly Schema sut = new Schema("my-schema");

        [Fact]
        public void Should_instantiate_schema()
        {
            Assert.Equal("my-schema", sut.Name);
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

            sut.Update(properties);

            Assert.Equal(properties, sut.Properties);
        }

        [Fact]
        public void Should_add_field()
        {
            var field = AddNumberField(1);

            Assert.Equal(field, sut.FieldsById[1]);
        }

        [Fact]
        public void Should_throw_exception_if_adding_field_with_name_that_already_exists()
        {
            AddNumberField(1);

            Assert.Throws<ArgumentException>(() => sut.AddField(new NumberField(2, "my-field-1", Partitioning.Invariant)));
        }

        [Fact]
        public void Should_throw_exception_if_adding_field_with_id_that_already_exists()
        {
            AddNumberField(1);

            Assert.Throws<ArgumentException>(() => sut.AddField(new NumberField(1, "my-field-2", Partitioning.Invariant)));
        }

        [Fact]
        public void Should_hide_field()
        {
            AddNumberField(1);

            sut.FieldsById[1].Hide();
            sut.FieldsById[1].Hide();

            Assert.True(sut.FieldsById[1].IsHidden);
        }

        [Fact]
        public void Should_show_field()
        {
            AddNumberField(1);

            sut.FieldsById[1].Hide();
            sut.FieldsById[1].Show();
            sut.FieldsById[1].Show();

            Assert.False(sut.FieldsById[1].IsHidden);
        }

        [Fact]
        public void Should_disable_field()
        {
            AddNumberField(1);

            sut.FieldsById[1].Disable();
            sut.FieldsById[1].Disable();

            Assert.True(sut.FieldsById[1].IsDisabled);
        }

        [Fact]
        public void Should_enable_field()
        {
            AddNumberField(1);

            sut.FieldsById[1].Disable();
            sut.FieldsById[1].Enable();
            sut.FieldsById[1].Enable();

            Assert.False(sut.FieldsById[1].IsDisabled);
        }

        [Fact]
        public void Should_lock_field()
        {
            AddNumberField(1);

            sut.FieldsById[1].Lock();

            Assert.True(sut.FieldsById[1].IsLocked);
        }

        [Fact]
        public void Should_do_nothing_if_field_to_delete_not_found()
        {
            AddNumberField(1);

            sut.DeleteField(2);

            Assert.Equal(1, sut.FieldsById.Count);
        }

        [Fact]
        public void Should_delete_field()
        {
            AddNumberField(1);

            sut.DeleteField(1);

            Assert.Empty(sut.FieldsById);
        }

        [Fact]
        public void Should_update_field()
        {
            AddNumberField(1);

            sut.FieldsById[1].Update(new NumberFieldProperties { Hints = "my-hints" });

            Assert.Equal("my-hints", sut.FieldsById[1].RawProperties.Hints);
        }

        [Fact]
        public void Should_throw_exception_if_updating_with_invalid_properties_type()
        {
            AddNumberField(1);

            Assert.Throws<ArgumentException>(() => sut.FieldsById[1].Update(new StringFieldProperties()));
        }

        [Fact]
        public void Should_publish_schema()
        {
            sut.Publish();

            Assert.True(sut.IsPublished);
        }

        [Fact]
        public void Should_unpublish_schema()
        {
            sut.Publish();
            sut.Unpublish();

            Assert.False(sut.IsPublished);
        }

        [Fact]
        public void Should_reorder_fields()
        {
            var field1 = AddNumberField(1);
            var field2 = AddNumberField(2);
            var field3 = AddNumberField(3);

            sut.ReorderFields(new List<long> { 3, 2, 1 });

            Assert.Equal(new List<Field> { field3, field2, field1 }, sut.Fields.ToList());
        }

        [Fact]
        public void Should_throw_exception_if_not_all_fields_are_covered_for_reordering()
        {
            AddNumberField(1);
            AddNumberField(2);

            Assert.Throws<ArgumentException>(() => sut.ReorderFields(new List<long> { 1 }));
        }

        [Fact]
        public void Should_throw_exception_if_field_to_reorder_does_not_exist()
        {
            AddNumberField(1);
            AddNumberField(2);

            Assert.Throws<ArgumentException>(() => sut.ReorderFields(new List<long> { 1, 4 }));
        }

        private NumberField AddNumberField(int id)
        {
            var field = new NumberField(id, $"my-field-{id}", Partitioning.Invariant);

            sut.AddField(field);

            return field;
        }
    }
}
