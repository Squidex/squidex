// ==========================================================================
//  SchemaTests.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Newtonsoft.Json.Linq;
using NJsonSchema;
using Squidex.Domain.Apps.Core.Schemas.Edm;
using Squidex.Domain.Apps.Core.Schemas.JsonSchema;
using Squidex.Infrastructure;
using Xunit;

namespace Squidex.Domain.Apps.Core.Schemas
{
    public class SchemaTests
    {
        private Schema sut = Schema.Create("my-name", new SchemaProperties());

        private sealed class InvalidProperties : FieldProperties
        {
            public override JToken GetDefaultValue()
            {
                return null;
            }

            public override T Accept<T>(IFieldPropertiesVisitor<T> visitor)
            {
                return default(T);
            }
        }

        [Fact]
        public void Should_instantiate_field()
        {
            var properties = new SchemaProperties { Hints = "my-hint", Label = "my-label" };

            var schema = Schema.Create("my-name", properties);

            Assert.Equal("my-name", schema.Name);
            Assert.Equal(properties, schema.Properties);
        }

        [Fact]
        public void Should_throw_exception_if_creating_schema_with_invalid_name()
        {
            Assert.Throws<ArgumentException>(() => Schema.Create(string.Empty, new SchemaProperties()));
        }

        [Fact]
        public void Should_update_schema()
        {
            var properties = new SchemaProperties { Hints = "my-hint", Label = "my-label" };

            sut = sut.Update(properties);

            Assert.Equal(properties, sut.Properties);
        }

        [Fact]
        public void Should_add_field()
        {
            var field = Add();

            Assert.Equal(field, sut.FieldsById[1]);
        }

        [Fact]
        public void Should_throw_exception_if_adding_field_with_name_that_already_exists()
        {
            Add();

            Assert.Throws<ArgumentException>(() => sut.AddField(new NumberField(2, "my-field", Partitioning.Invariant)));
        }

        [Fact]
        public void Should_hide_field()
        {
            Add();

            sut = sut.HideField(1);
            sut = sut.HideField(1);

            Assert.True(sut.FieldsById[1].IsHidden);
        }

        [Fact]
        public void Should_show_field()
        {
            Add();

            sut = sut.HideField(1);
            sut = sut.ShowField(1);
            sut = sut.ShowField(1);

            Assert.False(sut.FieldsById[1].IsHidden);
        }

        [Fact]
        public void Should_disable_field()
        {
            Add();

            sut = sut.DisableField(1);
            sut = sut.DisableField(1);

            Assert.True(sut.FieldsById[1].IsDisabled);
        }

        [Fact]
        public void Should_enable_field()
        {
            Add();

            sut = sut.DisableField(1);
            sut = sut.EnableField(1);
            sut = sut.EnableField(1);

            Assert.False(sut.FieldsById[1].IsDisabled);
        }

        [Fact]
        public void Should_lock_field()
        {
            Add();

            sut = sut.LockField(1);

            Assert.True(sut.FieldsById[1].IsLocked);
        }

        [Fact]
        public void Should_delete_field()
        {
            Add();

            sut = sut.DeleteField(1);

            Assert.Empty(sut.FieldsById);
        }

        [Fact]
        public void Should_update_field()
        {
            Add();

            sut = sut.UpdateField(1, new NumberFieldProperties { Hints = "my-hints" });

            Assert.Equal("my-hints", sut.FieldsById[1].RawProperties.Hints);
        }

        [Fact]
        public void Should_throw_exception_if_updating_with_invalid_properties_type()
        {
            Add();

            Assert.Throws<ArgumentException>(() => sut.UpdateField(1, new StringFieldProperties()));
        }

        [Fact]
        public void Should_publish_schema()
        {
            sut = sut.Publish();

            Assert.True(sut.IsPublished);
        }

        [Fact]
        public void Should_unpublish_schema()
        {
            sut = sut.Publish();
            sut = sut.Unpublish();

            Assert.False(sut.IsPublished);
        }

        [Fact]
        public void Should_reorder_fields()
        {
            var field1 = new StringField(1, "1", Partitioning.Invariant);
            var field2 = new StringField(2, "2", Partitioning.Invariant);
            var field3 = new StringField(3, "3", Partitioning.Invariant);

            sut = sut.AddField(field1);
            sut = sut.AddField(field2);
            sut = sut.AddField(field3);
            sut = sut.ReorderFields(new List<long> { 3, 2, 1 });

            Assert.Equal(new List<Field> { field3, field2, field1 }, sut.Fields.ToList());
        }

        [Fact]
        public void Should_throw_exception_if_not_all_fields_are_covered_for_reordering()
        {
            var field1 = new StringField(1, "1", Partitioning.Invariant);
            var field2 = new StringField(2, "2", Partitioning.Invariant);

            sut = sut.AddField(field1);
            sut = sut.AddField(field2);

            Assert.Throws<ArgumentException>(() => sut.ReorderFields(new List<long> { 1 }));
        }

        [Fact]
        public void Should_throw_exception_if_field_to_reorder_does_not_exist()
        {
            var field1 = new StringField(1, "1", Partitioning.Invariant);
            var field2 = new StringField(2, "2", Partitioning.Invariant);

            sut = sut.AddField(field1);
            sut = sut.AddField(field2);

            Assert.Throws<ArgumentException>(() => sut.ReorderFields(new List<long> { 1, 4 }));
        }

        [Fact]
        public void Should_build_schema()
        {
            var languagesConfig = LanguagesConfig.Create(Language.DE, Language.EN);

            var jsonSchema = BuildMixedSchema().BuildJsonSchema(languagesConfig.ToResolver(), (n, s) => new JsonSchema4 { Reference = s });

            Assert.NotNull(jsonSchema);
        }

        [Fact]
        public void Should_build_edm_model()
        {
            var languagesConfig = LanguagesConfig.Create(Language.DE, Language.EN);

            var edmModel = BuildMixedSchema().BuildEdmType(languagesConfig.ToResolver(), x => x);

            Assert.NotNull(edmModel);
        }

        private static Schema BuildMixedSchema()
        {
            var schema =
                Schema.Create("user", new SchemaProperties { Hints = "The User" })
                    .AddField(new JsonField(1, "my-json", Partitioning.Invariant,
                        new JsonFieldProperties()))
                    .AddField(new AssetsField(2, "my-assets", Partitioning.Invariant,
                        new AssetsFieldProperties()))
                    .AddField(new StringField(3, "my-string1", Partitioning.Language,
                        new StringFieldProperties { Label = "My String1", IsRequired = true, AllowedValues = ImmutableList.Create("a", "b") }))
                    .AddField(new StringField(4, "my-string2", Partitioning.Invariant,
                        new StringFieldProperties { Hints = "My String1" }))
                    .AddField(new NumberField(5, "my-number", Partitioning.Invariant,
                        new NumberFieldProperties { MinValue = 1, MaxValue = 10 }))
                    .AddField(new BooleanField(6, "my-boolean", Partitioning.Invariant,
                        new BooleanFieldProperties()))
                    .AddField(new DateTimeField(7, "my-datetime", Partitioning.Invariant,
                        new DateTimeFieldProperties { Editor = DateTimeFieldEditor.DateTime }))
                    .AddField(new DateTimeField(8, "my-date", Partitioning.Invariant,
                        new DateTimeFieldProperties { Editor = DateTimeFieldEditor.Date }))
                    .AddField(new GeolocationField(9, "my-geolocation", Partitioning.Invariant,
                        new GeolocationFieldProperties()))
                    .AddField(new ReferencesField(10, "my-references", Partitioning.Invariant,
                        new ReferencesFieldProperties()))
                    .AddField(new TagsField(11, "my-tags", Partitioning.Invariant,
                        new TagsFieldProperties()));

            return schema;
        }

        private NumberField Add()
        {
            var field = new NumberField(1, "my-field", Partitioning.Invariant);

            sut = sut.AddField(field);

            return field;
        }
    }
}
