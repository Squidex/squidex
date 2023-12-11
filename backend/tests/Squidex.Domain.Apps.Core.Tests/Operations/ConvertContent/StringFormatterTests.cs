// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Domain.Apps.Core.ConvertContent;
using Squidex.Domain.Apps.Core.Schemas;
using Squidex.Infrastructure.Json.Objects;

namespace Squidex.Domain.Apps.Core.Operations.ConvertContent;

public sealed class StringFormatterTests
{
    [Fact]
    public void Should_format_null_value()
    {
        var fieldValue = JsonValue.Null;
        var fieldConfig = Fields.Array(1, "field", Partitioning.Invariant);

        var formatted = StringFormatter.Format(fieldConfig, fieldValue);

        Assert.Empty(formatted);
    }

    [Fact]
    public void Should_format_null_json_value()
    {
        var fieldValue = JsonValue.Null;
        var fieldConfig = Fields.Array(1, "field", Partitioning.Invariant);

        var formatted = StringFormatter.Format(fieldConfig, fieldValue);

        Assert.Empty(formatted);
    }

    [Fact]
    public void Should_format_array_field_without_items()
    {
        var fieldValue = new JsonArray();
        var fieldConfig = Fields.Array(1, "field", Partitioning.Invariant);

        var formatted = StringFormatter.Format(fieldConfig, fieldValue);

        Assert.Equal("0 Items", formatted);
    }

    [Fact]
    public void Should_format_array_field_with_single_item()
    {
        var fieldValue = JsonValue.Array(JsonValue.Object());
        var fieldConfig = Fields.Array(1, "field", Partitioning.Invariant);

        var formatted = StringFormatter.Format(fieldConfig, fieldValue);

        Assert.Equal("1 Item", formatted);
    }

    [Fact]
    public void Should_format_array_field_with_multiple_items()
    {
        var fieldValue = JsonValue.Array(JsonValue.Object(), JsonValue.Object());
        var fieldConfig = Fields.Array(1, "field", Partitioning.Invariant);

        var formatted = StringFormatter.Format(fieldConfig, fieldValue);

        Assert.Equal("2 Items", formatted);
    }

    [Fact]
    public void Should_format_array_field_with_wrong_type()
    {
        var fieldValue = JsonValue.True;
        var fieldConfig = Fields.Array(1, "field", Partitioning.Invariant);

        var formatted = StringFormatter.Format(fieldConfig, fieldValue);

        Assert.Equal("0 Items", formatted);
    }

    [Fact]
    public void Should_format_assets_field_without_items()
    {
        var fieldValue = new JsonArray();
        var fieldConfig = Fields.Assets(1, "field", Partitioning.Invariant);

        var formatted = StringFormatter.Format(fieldConfig, fieldValue);

        Assert.Equal("0 Assets", formatted);
    }

    [Fact]
    public void Should_format_assets_field_with_single_item()
    {
        var fieldValue = JsonValue.Array(JsonValue.Object());
        var fieldConfig = Fields.Assets(1, "field", Partitioning.Invariant);

        var formatted = StringFormatter.Format(fieldConfig, fieldValue);

        Assert.Equal("1 Asset", formatted);
    }

    [Fact]
    public void Should_format_assets_field_with_multiple_items()
    {
        var fieldValue = JsonValue.Array(JsonValue.Object(), JsonValue.Object());
        var fieldConfig = Fields.Assets(1, "field", Partitioning.Invariant);

        var formatted = StringFormatter.Format(fieldConfig, fieldValue);

        Assert.Equal("2 Assets", formatted);
    }

    [Fact]
    public void Should_format_assets_field_with_wrong_type()
    {
        var fieldValue = JsonValue.True;
        var fieldConfig = Fields.Assets(1, "field", Partitioning.Invariant);

        var formatted = StringFormatter.Format(fieldConfig, fieldValue);

        Assert.Equal("0 Assets", formatted);
    }

    [Fact]
    public void Should_format_boolean_field_with_true()
    {
        var fieldValue = JsonValue.True;
        var fieldConfig = Fields.Boolean(1, "field", Partitioning.Invariant);

        var formatted = StringFormatter.Format(fieldConfig, fieldValue);

        Assert.Equal("Yes", formatted);
    }

    [Fact]
    public void Should_format_boolean_field_with_false()
    {
        var fieldValue = JsonValue.False;
        var fieldConfig = Fields.Boolean(1, "field", Partitioning.Invariant);

        var formatted = StringFormatter.Format(fieldConfig, fieldValue);

        Assert.Equal("No", formatted);
    }

    [Fact]
    public void Should_format_boolean_field_with_wrong_type()
    {
        var fieldValue = JsonValue.Zero;
        var fieldConfig = Fields.Boolean(1, "field", Partitioning.Invariant);

        var formatted = StringFormatter.Format(fieldConfig, fieldValue);

        Assert.Equal("No", formatted);
    }

    [Fact]
    public void Should_format_component_field()
    {
        var fieldValue = JsonValue.Object();
        var fieldConfig = Fields.Component(1, "field", Partitioning.Invariant);

        var formatted = StringFormatter.Format(fieldConfig, fieldValue);

        Assert.Equal("{ Component }", formatted);
    }

    [Fact]
    public void Should_format_components_field_without_items()
    {
        var fieldValue = new JsonArray();
        var fieldConfig = Fields.Components(1, "field", Partitioning.Invariant);

        var formatted = StringFormatter.Format(fieldConfig, fieldValue);

        Assert.Equal("0 Components", formatted);
    }

    [Fact]
    public void Should_format_components_field_with_single_item()
    {
        var fieldValue = JsonValue.Array(JsonValue.Object());
        var fieldConfig = Fields.Components(1, "field", Partitioning.Invariant);

        var formatted = StringFormatter.Format(fieldConfig, fieldValue);

        Assert.Equal("1 Component", formatted);
    }

    [Fact]
    public void Should_format_components_field_with_multiple_items()
    {
        var fieldValue = JsonValue.Array(JsonValue.Object(), JsonValue.Object());
        var fieldConfig = Fields.Components(1, "field", Partitioning.Invariant);

        var formatted = StringFormatter.Format(fieldConfig, fieldValue);

        Assert.Equal("2 Components", formatted);
    }

    [Fact]
    public void Should_format_datetime_field()
    {
        var fieldValue = JsonValue.Create("2019-01-19T12:00:00Z");
        var fieldConfig = Fields.DateTime(1, "field", Partitioning.Invariant);

        var formatted = StringFormatter.Format(fieldConfig, fieldValue);

        Assert.Equal("2019-01-19T12:00:00Z", formatted);
    }

    [Fact]
    public void Should_format_geolocation_field_with_correct_data()
    {
        var fieldValue = JsonValue.Object().Add("latitude", 18.9).Add("longitude", 10.9);
        var fieldConfig = Fields.Geolocation(1, "field", Partitioning.Invariant);

        var formatted = StringFormatter.Format(fieldConfig, fieldValue);

        Assert.Equal("18.9, 10.9", formatted);
    }

    [Fact]
    public void Should_format_geolocation_field_with_missing_property()
    {
        var fieldValue = JsonValue.Object().Add("latitude", 18.9);
        var fieldConfig = Fields.Geolocation(1, "field", Partitioning.Invariant);

        var formatted = StringFormatter.Format(fieldConfig, fieldValue);

        Assert.Empty(formatted);
    }

    [Fact]
    public void Should_format_geolocation_field_with_invalid_type()
    {
        var fieldValue = JsonValue.Zero;
        var fieldConfig = Fields.Geolocation(1, "field", Partitioning.Invariant);

        var formatted = StringFormatter.Format(fieldConfig, fieldValue);

        Assert.Empty(formatted);
    }

    [Fact]
    public void Should_format_json_field()
    {
        var fieldValue = JsonValue.Object();
        var fieldConfig = Fields.Json(1, "field", Partitioning.Invariant);

        var formatted = StringFormatter.Format(fieldConfig, fieldValue);

        Assert.Equal("<Json />", formatted);
    }

    [Fact]
    public void Should_format_number_field()
    {
        var fieldValue = JsonValue.Create(123);
        var fieldConfig = Fields.Number(1, "field", Partitioning.Invariant);

        var formatted = StringFormatter.Format(fieldConfig, fieldValue);

        Assert.Equal("123", formatted);
    }

    [Fact]
    public void Should_format_references_field_without_items()
    {
        var fieldValue = new JsonArray();
        var fieldConfig = Fields.References(1, "field", Partitioning.Invariant);

        var formatted = StringFormatter.Format(fieldConfig, fieldValue);

        Assert.Equal("0 References", formatted);
    }

    [Fact]
    public void Should_format_references_field_with_single_item()
    {
        var fieldValue = JsonValue.Array(JsonValue.Object());
        var fieldConfig = Fields.References(1, "field", Partitioning.Invariant);

        var formatted = StringFormatter.Format(fieldConfig, fieldValue);

        Assert.Equal("1 Reference", formatted);
    }

    [Fact]
    public void Should_format_references_field_with_multiple_items()
    {
        var fieldValue = JsonValue.Array(JsonValue.Object(), JsonValue.Object());
        var fieldConfig = Fields.References(1, "field", Partitioning.Invariant);

        var formatted = StringFormatter.Format(fieldConfig, fieldValue);

        Assert.Equal("2 References", formatted);
    }

    [Fact]
    public void Should_format_references_field_with_wrong_type()
    {
        var fieldValue = JsonValue.True;
        var fieldConfig = Fields.References(1, "field", Partitioning.Invariant);

        var formatted = StringFormatter.Format(fieldConfig, fieldValue);

        Assert.Equal("0 References", formatted);
    }

    [Fact]
    public void Should_format_string_field()
    {
        var fieldValue = JsonValue.Create("hello");
        var fieldConfig = Fields.String(1, "field", Partitioning.Invariant);

        var formatted = StringFormatter.Format(fieldConfig, fieldValue);

        Assert.Equal("hello", formatted);
    }

    [Fact]
    public void Should_format_string_field_with_photo_editor()
    {
        var fieldValue = JsonValue.Create("hello");
        var fieldConfig = Fields.String(1, "field", Partitioning.Invariant, new StringFieldProperties { Editor = StringFieldEditor.StockPhoto });

        var formatted = StringFormatter.Format(fieldConfig, fieldValue);

        Assert.Equal("[Photo]", formatted);
    }

    [Fact]
    public void Should_format_tags_field()
    {
        var fieldValue = JsonValue.Array("hello", "squidex", "and", "team");
        var fieldConfig = Fields.Tags(1, "field", Partitioning.Invariant);

        var formatted = StringFormatter.Format(fieldConfig, fieldValue);

        Assert.Equal("hello, squidex, and, team", formatted);
    }

    [Fact]
    public void Should_format_tags_field_with_invalid_type()
    {
        var fieldValue = JsonValue.Zero;
        var fieldConfig = Fields.Tags(1, "field", Partitioning.Invariant);

        var formatted = StringFormatter.Format(fieldConfig, fieldValue);

        Assert.Empty(formatted);
    }

    [Fact]
    public void Should_format_rich_text_if_null()
    {
        var fieldValue = JsonValue.Null;
        var fieldConfig = Fields.RichText(1, "field", Partitioning.Invariant);

        var formatted = StringFormatter.Format(fieldConfig, fieldValue);

        Assert.Empty(formatted);
    }

    [Fact]
    public void Should_format_rich_text()
    {
        var fieldConfig = Fields.RichText(1, "field", Partitioning.Invariant);
        var fieldValue = JsonValue.Object()
            .Add("type", "doc")
            .Add("content", JsonValue.Array(
                JsonValue.Object()
                    .Add("type", "paragraph")
                    .Add("content", JsonValue.Array(
                        JsonValue.Object()
                            .Add("type", "text")
                            .Add("text", "Paragraph1"))),
                JsonValue.Object()
                    .Add("type", "paragraph")
                    .Add("content", JsonValue.Array(
                        JsonValue.Object()
                            .Add("type", "text")
                            .Add("text", "Paragraph2")))));

        var formatted = StringFormatter.Format(fieldConfig, fieldValue);

        Assert.Equal("Paragraph1 Paragraph2", formatted);
    }
}
