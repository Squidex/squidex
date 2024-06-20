// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using NodaTime;
using Squidex.Domain.Apps.Core.Apps;
using Squidex.Domain.Apps.Core.Contents;
using Squidex.Domain.Apps.Core.ConvertContent;
using Squidex.Domain.Apps.Core.Schemas;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Json.Objects;

namespace Squidex.Domain.Apps.Core.Operations.ConvertContent;

public class DefaultValuesTests
{
    private readonly Instant now = Instant.FromUtc(2017, 10, 12, 16, 30, 10);
    private readonly LanguagesConfig languages = LanguagesConfig.English.Set(Language.DE);
    private readonly Schema schema;

    public DefaultValuesTests()
    {
        schema =
            new Schema { Name = "my-schema" }
                .AddString(1, "myString", Partitioning.Language,
                    new StringFieldProperties { DefaultValue = "en-string", IsRequired = true })
                .AddNumber(2, "myNumber", Partitioning.Invariant,
                    new NumberFieldProperties())
                .AddDateTime(3, "myDatetime", Partitioning.Invariant,
                    new DateTimeFieldProperties { DefaultValue = now })
                .AddBoolean(4, "myBoolean", Partitioning.Invariant,
                    new BooleanFieldProperties { DefaultValue = true, IsRequired = true })
                .AddArray(5, "myArray", Partitioning.Invariant, a =>
                    a.AddString(51, "myArrayString",
                        new StringFieldProperties { DefaultValue = "array-string" }));
    }

    [Fact]
    public void Should_enrich_with_default_values()
    {
        var source =
            new ContentData()
                .AddField("myString",
                    new ContentFieldData()
                        .AddLocalized("de", "de-string"))
                .AddField("myNumber",
                    new ContentFieldData()
                        .AddInvariant(456))
                .AddField("myArray",
                    new ContentFieldData()
                        .AddInvariant(
                            JsonValue.Array(
                                JsonValue.Object())));

        var actual =
            new ContentConverter(ResolvedComponents.Empty, schema)
                .Add(new AddDefaultValues(languages.ToResolver()))
                .Convert(source);

        var expected =
            new ContentData()
                .AddField("myString",
                    new ContentFieldData()
                        .AddLocalized("de", "de-string")
                        .AddLocalized("en", "en-string"))
                .AddField("myNumber",
                    new ContentFieldData()
                        .AddInvariant(456))
                .AddField("myDatetime",
                    new ContentFieldData()
                        .AddInvariant(now.ToString()))
                .AddField("myBoolean",
                    new ContentFieldData()
                        .AddInvariant(true))
                .AddField("myArray",
                    new ContentFieldData()
                        .AddInvariant(
                            JsonValue.Array(
                                JsonValue.Object()
                                    .Add("myArrayString", "array-string"))));

        Assert.Equal(expected, actual);
    }

    [Fact]
    public void Should_enrich_with_default_values_and_ignore_required_fields()
    {
        var source =
            new ContentData()
                .AddField("myNumber",
                    new ContentFieldData()
                        .AddInvariant(456))
                .AddField("myArray",
                    new ContentFieldData()
                        .AddInvariant(
                            JsonValue.Array(
                                JsonValue.Object())));

        var actual =
            new ContentConverter(ResolvedComponents.Empty, schema)
                .Add(new AddDefaultValues(languages.ToResolver()) { IgnoreRequiredFields = true })
                .Convert(source);

        var expected =
            new ContentData()
                .AddField("myNumber",
                    new ContentFieldData()
                        .AddInvariant(456))
                .AddField("myDatetime",
                    new ContentFieldData()
                        .AddInvariant(now.ToString()))
                .AddField("myArray",
                    new ContentFieldData()
                        .AddInvariant(
                            JsonValue.Array(
                                JsonValue.Object()
                                    .Add("myArrayString", "array-string"))));

        Assert.Equal(expected, actual);
    }

    [Fact]
    public void Should_enrich_with_default_values_and_ignore_non_master_fields()
    {
        var source =
            new ContentData()
                .AddField("myNumber",
                    new ContentFieldData()
                        .AddInvariant(456))
                .AddField("myArray",
                    new ContentFieldData()
                        .AddInvariant(
                            JsonValue.Array(
                                JsonValue.Object())));

        var actual =
            new ContentConverter(ResolvedComponents.Empty, schema)
                .Add(new AddDefaultValues(languages.ToResolver()) { IgnoreNonMasterFields = true })
                .Convert(source);

        var expected =
            new ContentData()
                .AddField("myString",
                    new ContentFieldData()
                        .AddLocalized("en", "en-string"))
                .AddField("myNumber",
                    new ContentFieldData()
                        .AddInvariant(456))
                .AddField("myDatetime",
                    new ContentFieldData()
                        .AddInvariant(now.ToString()))
                .AddField("myBoolean",
                    new ContentFieldData()
                        .AddInvariant(true))
                .AddField("myArray",
                    new ContentFieldData()
                        .AddInvariant(
                            JsonValue.Array(
                                JsonValue.Object()
                                    .Add("myArrayString", "array-string"))));

        Assert.Equal(expected, actual);
    }

    [Fact]
    public void Should_not_enrich_with_default_values_if_string_is_empty()
    {
        var source =
            new ContentData()
                .AddField("myString",
                    new ContentFieldData()
                        .AddLocalized("de", string.Empty))
                .AddField("myNumber",
                    new ContentFieldData()
                        .AddInvariant(456));

        var actual =
            new ContentConverter(ResolvedComponents.Empty, schema)
                .Add(new AddDefaultValues(languages.ToResolver()))
                .Convert(source);

        var expected =
            new ContentData()
                .AddField("myString",
                    new ContentFieldData()
                        .AddLocalized("de", string.Empty)
                        .AddLocalized("en", "en-string"))
                .AddField("myNumber",
                    new ContentFieldData()
                        .AddInvariant(456))
                .AddField("myDatetime",
                    new ContentFieldData()
                        .AddInvariant(now.ToString()))
                .AddField("myBoolean",
                    new ContentFieldData()
                        .AddInvariant(true))
                .AddField("myArray",
                    new ContentFieldData()
                        .AddInvariant(
                            JsonValue.Array()));

        Assert.Equal(expected, actual);
    }

    [Fact]
    public void Should_enrich_fields_if_at_field_names_is_given()
    {
        var source =
            new ContentData()
                .AddField("myString",
                    new ContentFieldData()
                        .AddLocalized("de", string.Empty))
                .AddField("myNumber",
                    new ContentFieldData()
                        .AddInvariant(456));

        var actual =
            new ContentConverter(ResolvedComponents.Empty, schema)
                .Add(new AddDefaultValues(languages.ToResolver())
                {
                    FieldNames = ["myString"]
                })
                .Convert(source);

        var expected =
            new ContentData()
                .AddField("myString",
                    new ContentFieldData()
                        .AddLocalized("de", string.Empty)
                        .AddLocalized("en", "en-string"))
                .AddField("myNumber",
                    new ContentFieldData()
                        .AddInvariant(456));

        Assert.Equal(expected, actual);
    }
}
