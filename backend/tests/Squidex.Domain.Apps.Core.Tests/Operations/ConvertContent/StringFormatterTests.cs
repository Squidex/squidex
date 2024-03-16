// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Domain.Apps.Core.ConvertContent;
using Squidex.Domain.Apps.Core.Schemas;
using Squidex.Infrastructure.Json.Objects;
using Xunit;

namespace Squidex.Domain.Apps.Core.Operations.ConvertContent
{
    public sealed class StringFormatterTests
    {
        [Fact]
        public void Should_format_null_value()
        {
            var field = Fields.Array(1, "field", Partitioning.Invariant);

            var formatted = StringFormatter.Format(field, null);

            Assert.Empty(formatted);
        }

        [Fact]
        public void Should_format_null_json_value()
        {
            var field = Fields.Array(1, "field", Partitioning.Invariant);

            var formatted = StringFormatter.Format(field, JsonValue.Null);

            Assert.Empty(formatted);
        }

        [Fact]
        public void Should_format_array_field_without_items()
        {
            var value = JsonValue.Array();

            var field = Fields.Array(1, "field", Partitioning.Invariant);

            var formatted = StringFormatter.Format(field, value);

            Assert.Equal("0 Items", formatted);
        }

        [Fact]
        public void Should_format_array_field_with_single_item()
        {
            var value = JsonValue.Array(JsonValue.Object());

            var field = Fields.Array(1, "field", Partitioning.Invariant);

            var formatted = StringFormatter.Format(field, value);

            Assert.Equal("1 Item", formatted);
        }

        [Fact]
        public void Should_format_array_field_with_multiple_items()
        {
            var value = JsonValue.Array(JsonValue.Object(), JsonValue.Object());

            var field = Fields.Array(1, "field", Partitioning.Invariant);

            var formatted = StringFormatter.Format(field, value);

            Assert.Equal("2 Items", formatted);
        }

        [Fact]
        public void Should_format_array_field_with_wrong_type()
        {
            var value = JsonValue.True;

            var field = Fields.Array(1, "field", Partitioning.Invariant);

            var formatted = StringFormatter.Format(field, value);

            Assert.Equal("0 Items", formatted);
        }

        [Fact]
        public void Should_format_assets_field_without_items()
        {
            var value = JsonValue.Array();

            var field = Fields.Assets(1, "field", Partitioning.Invariant);

            var formatted = StringFormatter.Format(field, value);

            Assert.Equal("0 Assets", formatted);
        }

        [Fact]
        public void Should_format_assets_field_with_single_item()
        {
            var value = JsonValue.Array(JsonValue.Object());

            var field = Fields.Assets(1, "field", Partitioning.Invariant);

            var formatted = StringFormatter.Format(field, value);

            Assert.Equal("1 Asset", formatted);
        }

        [Fact]
        public void Should_format_assets_field_with_multiple_items()
        {
            var value = JsonValue.Array(JsonValue.Object(), JsonValue.Object());

            var field = Fields.Assets(1, "field", Partitioning.Invariant);

            var formatted = StringFormatter.Format(field, value);

            Assert.Equal("2 Assets", formatted);
        }

        [Fact]
        public void Should_format_assets_field_with_wrong_type()
        {
            var value = JsonValue.True;

            var field = Fields.Assets(1, "field", Partitioning.Invariant);

            var formatted = StringFormatter.Format(field, value);

            Assert.Equal("0 Assets", formatted);
        }

        [Fact]
        public void Should_format_boolean_field_with_true()
        {
            var value = JsonValue.True;

            var field = Fields.Boolean(1, "field", Partitioning.Invariant);

            var formatted = StringFormatter.Format(field, value);

            Assert.Equal("Yes", formatted);
        }

        [Fact]
        public void Should_format_boolean_field_with_false()
        {
            var value = JsonValue.False;

            var field = Fields.Boolean(1, "field", Partitioning.Invariant);

            var formatted = StringFormatter.Format(field, value);

            Assert.Equal("No", formatted);
        }

        [Fact]
        public void Should_format_boolean_field_with_wrong_type()
        {
            var value = JsonValue.Zero;

            var field = Fields.Boolean(1, "field", Partitioning.Invariant);

            var formatted = StringFormatter.Format(field, value);

            Assert.Equal("No", formatted);
        }

        [Fact]
        public void Should_format_component_field()
        {
            var value = JsonValue.Object();

            var field = Fields.Component(1, "field", Partitioning.Invariant);

            var formatted = StringFormatter.Format(field, value);

            Assert.Equal("{ Component }", formatted);
        }

        [Fact]
        public void Should_format_components_field_without_items()
        {
            var value = JsonValue.Array();

            var field = Fields.Components(1, "field", Partitioning.Invariant);

            var formatted = StringFormatter.Format(field, value);

            Assert.Equal("0 Components", formatted);
        }

        [Fact]
        public void Should_format_components_field_with_single_item()
        {
            var value = JsonValue.Array(JsonValue.Object());

            var field = Fields.Components(1, "field", Partitioning.Invariant);

            var formatted = StringFormatter.Format(field, value);

            Assert.Equal("1 Component", formatted);
        }

        [Fact]
        public void Should_format_components_field_with_multiple_items()
        {
            var value = JsonValue.Array(JsonValue.Object(), JsonValue.Object());

            var field = Fields.Components(1, "field", Partitioning.Invariant);

            var formatted = StringFormatter.Format(field, value);

            Assert.Equal("2 Components", formatted);
        }

        [Fact]
        public void Should_format_datetime_field()
        {
            var value = JsonValue.Create("2019-01-19T12:00:00Z");

            var field = Fields.DateTime(1, "field", Partitioning.Invariant);

            var formatted = StringFormatter.Format(field, value);

            Assert.Equal("2019-01-19T12:00:00Z", formatted);
        }

        [Fact]
        public void Should_format_geolocation_field_with_correct_data()
        {
            var value = JsonValue.Object().Add("latitude", 18.9).Add("longitude", 10.9);

            var field = Fields.Geolocation(1, "field", Partitioning.Invariant);

            var formatted = StringFormatter.Format(field, value);

            Assert.Equal("18.9, 10.9", formatted);
        }

        [Fact]
        public void Should_format_geolocation_field_with_missing_property()
        {
            var value = JsonValue.Object().Add("latitude", 18.9);

            var field = Fields.Geolocation(1, "field", Partitioning.Invariant);

            var formatted = StringFormatter.Format(field, value);

            Assert.Empty(formatted);
        }

        [Fact]
        public void Should_format_geolocation_field_with_invalid_type()
        {
            var value = JsonValue.Zero;

            var field = Fields.Geolocation(1, "field", Partitioning.Invariant);

            var formatted = StringFormatter.Format(field, value);

            Assert.Empty(formatted);
        }

        [Fact]
        public void Should_format_json_field()
        {
            var value = JsonValue.Object();

            var field = Fields.Json(1, "field", Partitioning.Invariant);

            var formatted = StringFormatter.Format(field, value);

            Assert.Equal("<Json />", formatted);
        }

        [Fact]
        public void Should_format_number_field()
        {
            var value = JsonValue.Create(123);

            var field = Fields.Number(1, "field", Partitioning.Invariant);

            var formatted = StringFormatter.Format(field, value);

            Assert.Equal("123", formatted);
        }

        [Fact]
        public void Should_format_references_field_without_items()
        {
            var value = JsonValue.Array();

            var field = Fields.References(1, "field", Partitioning.Invariant);

            var formatted = StringFormatter.Format(field, value);

            Assert.Equal("0 References", formatted);
        }

        [Fact]
        public void Should_format_references_field_with_single_item()
        {
            var value = JsonValue.Array(JsonValue.Object());

            var field = Fields.References(1, "field", Partitioning.Invariant);

            var formatted = StringFormatter.Format(field, value);

            Assert.Equal("1 Reference", formatted);
        }

        [Fact]
        public void Should_format_references_field_with_multiple_items()
        {
            var value = JsonValue.Array(JsonValue.Object(), JsonValue.Object());

            var field = Fields.References(1, "field", Partitioning.Invariant);

            var formatted = StringFormatter.Format(field, value);

            Assert.Equal("2 References", formatted);
        }

        [Fact]
        public void Should_format_references_field_with_wrong_type()
        {
            var value = JsonValue.True;

            var field = Fields.References(1, "field", Partitioning.Invariant);

            var formatted = StringFormatter.Format(field, value);

            Assert.Equal("0 References", formatted);
        }

        [Fact]
        public void Should_format_string_field()
        {
            var value = JsonValue.Create("hello");

            var field = Fields.String(1, "field", Partitioning.Invariant);

            var formatted = StringFormatter.Format(field, value);

            Assert.Equal("hello", formatted);
        }

        [Fact]
        public void Should_format_string_field_with_photo_editor()
        {
            var value = JsonValue.Create("hello");

            var field = Fields.String(1, "field", Partitioning.Invariant, new StringFieldProperties { Editor = StringFieldEditor.StockPhoto });

            var formatted = StringFormatter.Format(field, value);

            Assert.Equal("[Photo]", formatted);
        }

        [Fact]
        public void Should_format_tags_field()
        {
            var value = JsonValue.Array("hello", "squidex", "and", "team");

            var field = Fields.Tags(1, "field", Partitioning.Invariant);

            var formatted = StringFormatter.Format(field, value);

            Assert.Equal("hello, squidex, and, team", formatted);
        }

        [Fact]
        public void Should_format_tags_field_with_invalid_type()
        {
            var value = JsonValue.Zero;

            var field = Fields.Tags(1, "field", Partitioning.Invariant);

            var formatted = StringFormatter.Format(field, value);

            Assert.Empty(formatted);
        }
    }
}
