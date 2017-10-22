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
using FluentAssertions;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Squidex.Domain.Apps.Core.Schemas;
using Xunit;

namespace Squidex.Domain.Apps.Core.Model.Schemas
{
    public class SchemaTests
    {
        private readonly JsonSerializer serializer = TestData.DefaultSerializer();
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
        public void Should_throw_exception_if_updating_with_invalid_properties_type()
        {
            AddNumberField(1);

            Assert.Throws<ArgumentException>(() => sut.FieldsById[1].Update(new StringFieldProperties()));
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

        [Fact]
        public void Should_serialize_and_deserialize_schema()
        {
            var schemaSource = TestData.MixedSchema();
            var schemaTarget = JToken.FromObject(schemaSource, serializer).ToObject<Schema>(serializer);

            schemaTarget.ShouldBeEquivalentTo(schemaSource);
        }

        private NumberField AddNumberField(int id)
        {
            var field = new NumberField(id, $"my-field-{id}", Partitioning.Invariant);

            sut.AddField(field);

            return field;
        }
    }
}
