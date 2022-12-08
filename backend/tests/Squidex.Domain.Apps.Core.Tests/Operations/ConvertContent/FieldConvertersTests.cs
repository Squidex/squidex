// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Domain.Apps.Core.Apps;
using Squidex.Domain.Apps.Core.Contents;
using Squidex.Domain.Apps.Core.ConvertContent;
using Squidex.Domain.Apps.Core.Schemas;
using Squidex.Domain.Apps.Core.TestHelpers;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Json.Objects;

namespace Squidex.Domain.Apps.Core.Operations.ConvertContent;

public class FieldConvertersTests
{
    private readonly LanguagesConfig languages = LanguagesConfig.English.Set(Language.DE);

    public static IEnumerable<object?[]> InvalidValues()
    {
        yield return new object?[] { JsonValue.Null };
        yield return new object?[] { JsonValue.Create(false) }; // Undefined
    }

    [Fact]
    public void Should_not_change_data_if_all_field_values_have_correct_type()
    {
        var field1 = Fields.Number(1, "number1", Partitioning.Language);
        var field2 = Fields.Number(2, "number2", Partitioning.Language);

        var schema =
            new Schema("my-schema")
                .AddField(field1)
                .AddField(field2);

        var source =
            new ContentData()
                .AddField(field1.Name,
                    new ContentFieldData()
                        .AddLocalized("en", 1))
                .AddField(field2.Name,
                    new ContentFieldData()
                        .AddLocalized("en", JsonValue.Null)
                        .AddLocalized("de", 1));

        var actual =
            new ContentConverter(ResolvedComponents.Empty, schema)
                .Add(new ExcludeChangedTypes(TestUtils.DefaultSerializer))
                .Convert(source);

        var expected = source;

        Assert.Equal(expected, actual);
    }

    [Fact]
    public void Should_remove_fields_with_invalid_data()
    {
        var field1 = Fields.Number(1, "number1", Partitioning.Language);
        var field2 = Fields.Number(2, "number2", Partitioning.Language);

        var schema =
            new Schema("my-schema")
                .AddField(field1)
                .AddField(field2);

        var source =
            new ContentData()
                .AddField(field1.Name,
                    new ContentFieldData()
                        .AddLocalized("en", 1))
                .AddField(field2.Name,
                    new ContentFieldData()
                        .AddLocalized("en", "2")
                        .AddLocalized("de", 2));

        var actual =
            new ContentConverter(ResolvedComponents.Empty, schema)
                .Add(new ExcludeChangedTypes(TestUtils.DefaultSerializer))
                .Convert(source);

        var expected =
            new ContentData()
                .AddField(field1.Name,
                    new ContentFieldData()
                        .AddLocalized("en", 1));

        Assert.Equal(expected, actual);
    }

    [Fact]
    public void Should_remove_hidden_fields()
    {
        var field1 = Fields.Number(1, "number1", Partitioning.Language);
        var field2 = Fields.Number(2, "number2", Partitioning.Language).Hide();

        var schema =
            new Schema("my-schema")
                .AddField(field1)
                .AddField(field2);

        var source =
            new ContentData()
                .AddField(field1.Name,
                    new ContentFieldData()
                        .AddLocalized("en", 1))
                .AddField(field2.Name,
                    new ContentFieldData()
                        .AddLocalized("en", JsonValue.Null)
                        .AddLocalized("de", 1));

        var actual =
            new ContentConverter(ResolvedComponents.Empty, schema)
                .Add(ExcludeHidden.Instance)
                .Convert(source);

        var expected =
            new ContentData()
                .AddField(field1.Name,
                    new ContentFieldData()
                        .AddLocalized("en", 1));

        Assert.Equal(expected, actual);
    }

    [Fact]
    public void Should_not_remove_hidden_fields()
    {
        var field1 = Fields.Number(1, "number1", Partitioning.Language);
        var field2 = Fields.Number(2, "number2", Partitioning.Language);

        var schema =
            new Schema("my-schema")
                .AddField(field1)
                .AddField(field2)
                .HideField(2);

        var source =
            new ContentData()
                .AddField(field1.Name,
                    new ContentFieldData()
                        .AddLocalized("en", 1))
                .AddField(field2.Name,
                    new ContentFieldData()
                        .AddLocalized("en", "2"));

        var actual =
            new ContentConverter(ResolvedComponents.Empty, schema)
                .Add(new ExcludeChangedTypes(TestUtils.DefaultSerializer))
                .Convert(source);

        var expected =
            new ContentData()
                .AddField(field1.Name,
                    new ContentFieldData()
                        .AddLocalized("en", 1));

        Assert.Equal(expected, actual);
    }

    [Fact]
    public void Should_remove_old_languages()
    {
        var field1 = Fields.String(1, "string1", Partitioning.Language);

        var schema =
            new Schema("my-schema")
                .AddField(field1);

        var source =
            new ContentData()
                .AddField(field1.Name,
                    new ContentFieldData()
                        .AddLocalized("en", "EN")
                        .AddLocalized("de", "DE")
                        .AddLocalized("it", "IT"));

        var actual =
            new ContentConverter(ResolvedComponents.Empty, schema)
                .Add(new ResolveLanguages(languages))
                .Convert(source);

        var expected =
            new ContentData()
                .AddField(field1.Name,
                    new ContentFieldData()
                        .AddLocalized("en", "EN")
                        .AddLocalized("de", "DE"));

        Assert.Equal(expected, actual);
    }

    [Fact]
    public void Should_remove_unwanted_languages()
    {
        var field1 = Fields.String(1, "string1", Partitioning.Language);

        var schema =
            new Schema("my-schema")
                .AddField(field1);

        var source =
            new ContentData()
                .AddField(field1.Name,
                    new ContentFieldData()
                        .AddLocalized("en", "EN")
                        .AddLocalized("de", "DE")
                        .AddLocalized("it", "IT"));

        var actual =
            new ContentConverter(ResolvedComponents.Empty, schema)
                .Add(new ResolveLanguages(languages, true, new[] { Language.DE }))
                .Convert(source);

        var expected =
            new ContentData()
                .AddField(field1.Name,
                    new ContentFieldData()
                        .AddLocalized("de", "DE"));

        Assert.Equal(expected, actual);
    }

    [Theory]
    [MemberData(nameof(InvalidValues))]
    public void Should_resolve_master_language_from_invariant(JsonValue value)
    {
        var field1 = Fields.String(1, "string", Partitioning.Language);

        var schema =
            new Schema("my-schema")
                .AddField(field1);

        var source =
            new ContentData()
                .AddField(field1.Name,
                    new ContentFieldData()
                        .AddLocalized("iv", "A")
                        .AddLocalized("it", "B"));

        if (value != false)
        {
            source[field1.Name]!["en"] = value!;
        }

        var actual =
            new ContentConverter(ResolvedComponents.Empty, schema)
                .Add(new ResolveLanguages(languages, false))
                .Convert(source);

        var expected =
            new ContentData()
                .AddField(field1.Name,
                    new ContentFieldData()
                        .AddLocalized("en", "A"));

        Assert.Equal(expected, actual);
    }

    [Theory]
    [MemberData(nameof(InvalidValues))]
    public void Should_remove_unwanted_languages_and_invariant(JsonValue value)
    {
        var field1 = Fields.String(1, "string", Partitioning.Language);

        var schema =
            new Schema("my-schema")
                .AddField(field1);

        var source =
            new ContentData()
                .AddField(field1.Name,
                    new ContentFieldData()
                        .AddLocalized("iv", "A")
                        .AddLocalized("it", "B"));

        if (value != false)
        {
            source[field1.Name]!["en"] = value!;
        }

        var actual =
            new ContentConverter(ResolvedComponents.Empty, schema)
                .Add(new ResolveLanguages(languages, false, Language.DE))
                .Convert(source);

        var expected =
            new ContentData()
                .AddField(field1.Name,
                    new ContentFieldData());

        Assert.Equal(expected, actual);
    }

    [Theory]
    [MemberData(nameof(InvalidValues))]
    public void Should_not_resolve_master_language_if_not_found(JsonValue value)
    {
        var field1 = Fields.String(1, "string", Partitioning.Language);

        var schema =
            new Schema("my-schema")
                .AddField(field1);

        var source =
            new ContentData()
                .AddField(field1.Name,
                    new ContentFieldData()
                        .AddLocalized("es", "A")
                        .AddLocalized("it", "B"));

        if (value != false)
        {
            source[field1.Name]!["en"] = value;
        }

        var actual =
            new ContentConverter(ResolvedComponents.Empty, schema)
                .Add(new ResolveLanguages(languages))
                .Convert(source);

        var expected =
            new ContentData()
                .AddField(field1.Name,
                    new ContentFieldData());

        if (value != false)
        {
            expected[field1.Name]!["en"] = value;
        }

        Assert.Equal(expected, actual);
    }

    [Fact]
    public void Should_resolve_language_from_master_and_filter()
    {
        var field1 = Fields.String(1, "string", Partitioning.Language);

        var schema =
            new Schema("my-schema")
                .AddField(field1);

        var source =
            new ContentData()
                .AddField(field1.Name,
                    new ContentFieldData()
                        .AddLocalized("en", "A"));

        var actual =
            new ContentConverter(ResolvedComponents.Empty, schema)
                .Add(new ResolveLanguages(languages, true, Language.DE))
                .Convert(source);

        var expected =
            new ContentData()
                .AddField(field1.Name,
                    new ContentFieldData()
                        .AddLocalized("de", "A"));

        Assert.Equal(expected, actual);
    }

    [Fact]
    public void Should_keep_invariant()
    {
        var field1 = Fields.String(1, "string", Partitioning.Invariant);

        var schema =
            new Schema("my-schema")
                .AddField(field1);

        var source =
            new ContentData()
                .AddField(field1.Name,
                    new ContentFieldData()
                        .AddInvariant("A"));

        var actual =
            new ContentConverter(ResolvedComponents.Empty, schema)
                .Add(new ResolveLanguages(languages))
                .Convert(source);

        var expected = source;

        Assert.Equal(expected, actual);
    }

    [Theory]
    [MemberData(nameof(InvalidValues))]
    public void Should_resolve_invariant_from_master_language(JsonValue value)
    {
        var field1 = Fields.String(1, "string", Partitioning.Invariant);

        var schema =
            new Schema("my-schema")
                .AddField(field1);

        var source =
            new ContentData()
                .AddField(field1.Name,
                    new ContentFieldData()
                        .AddLocalized("de", "DE")
                        .AddLocalized("en", "EN"));

        if (value != false)
        {
            source[field1.Name]![InvariantPartitioning.Key] = value;
        }

        var actual =
            new ContentConverter(ResolvedComponents.Empty, schema)
                .Add(new ResolveInvariant(languages))
                .Convert(source);

        var expected =
            new ContentData()
                .AddField(field1.Name,
                    new ContentFieldData()
                        .AddInvariant("EN"));

        Assert.Equal(expected, actual);
    }

    [Theory]
    [MemberData(nameof(InvalidValues))]
    public void Should_resolve_invariant_from_first_language(JsonValue value)
    {
        var field1 = Fields.String(1, "string", Partitioning.Invariant);

        var schema =
            new Schema("my-schema")
                .AddField(field1);

        var source =
            new ContentData()
                .AddField(field1.Name,
                    new ContentFieldData()
                        .AddLocalized("de", "DE")
                        .AddLocalized("it", "IT"));

        if (value != false)
        {
            source[field1.Name]![InvariantPartitioning.Key] = value;
        }

        var actual =
            new ContentConverter(ResolvedComponents.Empty, schema)
                .Add(new ResolveInvariant(languages))
                .Convert(source);

        var expected =
            new ContentData()
                .AddField(field1.Name,
                    new ContentFieldData()
                        .AddInvariant("DE"));

        Assert.Equal(expected, actual);
    }

    [Fact]
    public void Should_not_resolve_invariant_if_not_found()
    {
        var field1 = Fields.String(1, "string", Partitioning.Invariant);

        var schema =
            new Schema("my-schema")
                .AddField(field1);

        var source =
            new ContentData()
                .AddField(field1.Name,
                    new ContentFieldData());

        var actual =
            new ContentConverter(ResolvedComponents.Empty, schema)
                .Add(new ResolveLanguages(languages))
                .Convert(source);

        var expected = source;

        Assert.Equal(expected, actual);
    }

    [Theory]
    [MemberData(nameof(InvalidValues))]
    public void Should_resolve_from_fallback_language_if_found(JsonValue value)
    {
        var field1 = Fields.String(1, "string", Partitioning.Language);

        var schema =
            new Schema("my-schema")
                .AddField(field1);

        var config =
            LanguagesConfig.English
                .Set(Language.DE)
                .Set(Language.IT)
                .Set(Language.ES, false, Language.IT);

        var source =
            new ContentData()
                .AddField(field1.Name,
                    new ContentFieldData()
                        .AddLocalized("en", "EN")
                        .AddLocalized("it", "IT"));

        if (value != false)
        {
            source[field1.Name]!["de"] = value!;
        }

        var actual =
            new ContentConverter(ResolvedComponents.Empty, schema)
                .Add(new ResolveLanguages(config))
                .Convert(source);

        var expected =
            new ContentData()
                .AddField(field1.Name,
                    new ContentFieldData()
                        .AddLocalized("en", "EN")
                        .AddLocalized("it", "IT")
                        .AddLocalized("es", "IT")
                        .AddLocalized("de", "EN"));

        Assert.Equal(expected, actual);
    }

    [Fact]
    public void Should_return_master_language_if_languages_to_filter_are_invalid()
    {
        var field1 = Fields.String(1, "string", Partitioning.Language);

        var schema =
            new Schema("my-schema")
                .AddField(field1);

        var source =
            new ContentData()
                .AddField(field1.Name,
                    new ContentFieldData()
                        .AddLocalized("en", "EN")
                        .AddLocalized("de", "DE"));

        var actual =
            new ContentConverter(ResolvedComponents.Empty, schema)
                .Add(new ResolveLanguages(languages, true, Language.IT))
                .Convert(source);

        var expected =
            new ContentData()
                .AddField(field1.Name,
                    new ContentFieldData()
                        .AddLocalized("en", "EN"));

        Assert.Equal(expected, actual);
    }

    [Fact]
    public void Should_return_same_values_if_resolving_fallback_languages_from_invariant_field()
    {
        var field = Fields.String(1, "string", Partitioning.Invariant);

        var source = new ContentFieldData();

        var actual =
            new ResolveLanguages(languages)
                .ConvertFieldAfter(field, source);

        Assert.Same(source, actual);
    }

    [Fact]
    public void Should_return_same_values_if_filtered_languages_is_empty()
    {
        var field = Fields.String(1, "string", Partitioning.Language);

        var source = new ContentFieldData();

        var actual =
            new ResolveLanguages(languages, true, Array.Empty<Language>())
                .ConvertFieldAfter(field, source);

        Assert.Same(source, actual);
    }

    [Fact]
    public void Should_return_same_values_if_filtering_languages_from_invariant_field()
    {
        var field = Fields.String(1, "string", Partitioning.Invariant);

        var source = new ContentFieldData();

        var actual =
            new ResolveLanguages(languages)
                .ConvertFieldAfter(field, source);

        Assert.Same(source, actual);
    }

    /*
    [Fact]
    public void Should_add_schema_name_to_component()
    {
        var field = Fields.Component(1, "component", Partitioning.Invariant);

        var componentId = DomainId.NewGuid();
        var component = new Schema("my-component");
        var components = new ResolvedComponents(new Dictionary<DomainId, Schema>
        {
            [componentId] = component
        });

        var source =
            new ContentFieldData()
                .AddInvariant(
                    new JsonObject()
                        .Add(Component.Discriminator, componentId));

        var expected =
            new ContentFieldData()
                .AddInvariant(
                    new JsonObject()
                        .Add(Component.Discriminator, componentId)
                        .Add("schemaName", component.Name));

        var actual = FieldConverters.AddSchemaName(components)(data, field);

        var expected = new ContentFieldData();

        Assert.Equal(expected, actual);
    }*/
}
