// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using NodaTime;
using Squidex.Domain.Apps.Core.ConvertContent;
using Squidex.Domain.Apps.Core.Schemas;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Collections;
using Squidex.Infrastructure.Json.Objects;

namespace Squidex.Domain.Apps.Core.Operations.ConvertContent;

public class DefaultValueFactoryTests
{
    private readonly Instant now = Instant.FromUtc(2017, 10, 12, 16, 30, 10);
    private readonly Language language = Language.DE;

    [Fact]
    public void Should_get_default_value_from_array_field()
    {
        var field =
            Fields.Array(1, "1", Partitioning.Invariant,
                new ArrayFieldProperties());

        Assert.Equal(new JsonArray(), DefaultValueFactory.CreateDefaultValue(field, now, language.Iso2Code));
    }

    [Fact]
    public void Should_get_default_value_from_array_field_if_set_to_null()
    {
        var field =
            Fields.Array(1, "1", Partitioning.Invariant,
                new ArrayFieldProperties { CalculatedDefaultValue = ArrayCalculatedDefaultValue.Null });

        Assert.Equal(JsonValue.Null, DefaultValueFactory.CreateDefaultValue(field, now, language.Iso2Code));
    }

    [Fact]
    public void Should_get_default_value_from_assets_field()
    {
        var field =
            Fields.Assets(1, "1", Partitioning.Invariant,
                new AssetsFieldProperties());

        Assert.Equal(new JsonArray(), DefaultValueFactory.CreateDefaultValue(field, now, language.Iso2Code));
    }

    [Fact]
    public void Should_get_default_value_from_assets_field_if_set()
    {
        var field =
            Fields.Assets(1, "1", Partitioning.Invariant,
                new AssetsFieldProperties { DefaultValue = ReadonlyList.Create("1", "2") });

        Assert.Equal(JsonValue.Array("1", "2"), DefaultValueFactory.CreateDefaultValue(field, now, language.Iso2Code));
    }

    [Fact]
    public void Should_get_default_value_from_assets_field_if_localized()
    {
        var field =
            Fields.Assets(1, "1", Partitioning.Invariant,
                new AssetsFieldProperties
                {
                    DefaultValues = new LocalizedValue<ReadonlyList<string>?>(new Dictionary<string, ReadonlyList<string>?>
                    {
                        [language.Iso2Code] = null
                    }),
                    DefaultValue = ReadonlyList.Create("1", "2")
                });

        Assert.Equal(new JsonArray(), DefaultValueFactory.CreateDefaultValue(field, now, language.Iso2Code));
    }

    [Fact]
    public void Should_get_default_value_from_boolean_field()
    {
        var field =
            Fields.Boolean(1, "1", Partitioning.Invariant,
                new BooleanFieldProperties { DefaultValue = true });

        Assert.Equal(JsonValue.True, DefaultValueFactory.CreateDefaultValue(field, now, language.Iso2Code));
    }

    [Fact]
    public void Should_get_default_value_from_boolean_field_if_localized()
    {
        var field =
            Fields.Boolean(1, "1", Partitioning.Invariant,
                new BooleanFieldProperties
                {
                    DefaultValues = new LocalizedValue<bool?>(new Dictionary<string, bool?>
                    {
                        [language.Iso2Code] = null
                    }),
                    DefaultValue = true
                });

        Assert.Equal(JsonValue.Null, DefaultValueFactory.CreateDefaultValue(field, now, language.Iso2Code));
    }

    [Fact]
    public void Should_get_default_value_from_components_field()
    {
        var field =
            Fields.Components(1, "1", Partitioning.Invariant,
                new ComponentsFieldProperties());

        Assert.Equal(new JsonArray(), DefaultValueFactory.CreateDefaultValue(field, now, language.Iso2Code));
    }

    [Fact]
    public void Should_get_default_value_from_components_field_if_set_to_null()
    {
        var field =
            Fields.Components(1, "1", Partitioning.Invariant,
                new ComponentsFieldProperties { CalculatedDefaultValue = ArrayCalculatedDefaultValue.Null });

        Assert.Equal(JsonValue.Null, DefaultValueFactory.CreateDefaultValue(field, now, language.Iso2Code));
    }

    [Fact]
    public void Should_get_default_value_from_datetime_field()
    {
        var field =
            Fields.DateTime(1, "1", Partitioning.Invariant,
                new DateTimeFieldProperties { DefaultValue = FutureDays(15) });

        Assert.Equal(JsonValue.Create(FutureDays(15)), DefaultValueFactory.CreateDefaultValue(field, now, language.Iso2Code));
    }

    [Fact]
    public void Should_get_default_value_from_datetime_field_if_set_to_today()
    {
        var field =
            Fields.DateTime(1, "1", Partitioning.Invariant,
                new DateTimeFieldProperties { CalculatedDefaultValue = DateTimeCalculatedDefaultValue.Today });

        Assert.Equal(JsonValue.Create("2017-10-12T00:00:00Z"), DefaultValueFactory.CreateDefaultValue(field, now, language.Iso2Code));
    }

    [Fact]
    public void Should_get_default_value_from_datetime_field_if_set_to_now()
    {
        var field =
            Fields.DateTime(1, "1", Partitioning.Invariant,
                new DateTimeFieldProperties { CalculatedDefaultValue = DateTimeCalculatedDefaultValue.Now });

        Assert.Equal(JsonValue.Create("2017-10-12T16:30:10Z"), DefaultValueFactory.CreateDefaultValue(field, now, language.Iso2Code));
    }

    [Fact]
    public void Should_get_default_value_from_datetime_field_if_localized()
    {
        var field =
            Fields.DateTime(1, "1", Partitioning.Invariant,
                new DateTimeFieldProperties
                {
                    DefaultValues = new LocalizedValue<Instant?>(new Dictionary<string, Instant?>
                    {
                        [language.Iso2Code] = null
                    }),
                    DefaultValue = FutureDays(15)
                });

        Assert.Equal(JsonValue.Null, DefaultValueFactory.CreateDefaultValue(field, now, language.Iso2Code));
    }

    [Fact]
    public void Should_get_default_value_from_json_field()
    {
        var field =
            Fields.Json(1, "1", Partitioning.Invariant,
                new JsonFieldProperties());

        Assert.Equal(JsonValue.Null, DefaultValueFactory.CreateDefaultValue(field, now, language.Iso2Code));
    }

    [Fact]
    public void Should_get_default_value_from_geolocation_field()
    {
        var field =
            Fields.Geolocation(1, "1", Partitioning.Invariant,
                new GeolocationFieldProperties());

        Assert.Equal(JsonValue.Null, DefaultValueFactory.CreateDefaultValue(field, now, language.Iso2Code));
    }

    [Fact]
    public void Should_get_default_value_from_number_field()
    {
        var field =
            Fields.Number(1, "1", Partitioning.Invariant,
                new NumberFieldProperties { DefaultValue = 12 });

        Assert.Equal(JsonValue.Create(12), DefaultValueFactory.CreateDefaultValue(field, now, language.Iso2Code));
    }

    [Fact]
    public void Should_get_default_value_from_number_field_if_localized()
    {
        var field =
            Fields.Number(1, "1", Partitioning.Invariant,
                new NumberFieldProperties
                {
                    DefaultValues = new LocalizedValue<double?>(new Dictionary<string, double?>
                    {
                        [language.Iso2Code] = null
                    }),
                    DefaultValue = 12
                });

        Assert.Equal(JsonValue.Null, DefaultValueFactory.CreateDefaultValue(field, now, language.Iso2Code));
    }

    [Fact]
    public void Should_get_default_value_from_references_field()
    {
        var field =
            Fields.References(1, "1", Partitioning.Invariant,
                new ReferencesFieldProperties());

        Assert.Equal(new JsonArray(), DefaultValueFactory.CreateDefaultValue(field, now, language.Iso2Code));
    }

    [Fact]
    public void Should_get_default_value_from_references_field_if_set()
    {
        var field =
            Fields.References(1, "1", Partitioning.Invariant,
                new ReferencesFieldProperties { DefaultValue = ReadonlyList.Create("1", "2") });

        Assert.Equal(JsonValue.Array("1", "2"), DefaultValueFactory.CreateDefaultValue(field, now, language.Iso2Code));
    }

    [Fact]
    public void Should_get_default_value_from_references_field_if_localized()
    {
        var field =
            Fields.References(1, "1", Partitioning.Invariant,
                new ReferencesFieldProperties
                {
                    DefaultValues = new LocalizedValue<ReadonlyList<string>?>(new Dictionary<string, ReadonlyList<string>?>
                    {
                        [language.Iso2Code] = null
                    }),
                    DefaultValue = ReadonlyList.Create("1", "2")
                });

        Assert.Equal(new JsonArray(), DefaultValueFactory.CreateDefaultValue(field, now, language.Iso2Code));
    }

    [Fact]
    public void Should_get_default_value_from_rich_text_field()
    {
        var field = Fields.RichText(1, "1", Partitioning.Invariant);

        Assert.Equal(JsonValue.Null, DefaultValueFactory.CreateDefaultValue(field, now, language.Iso2Code));
    }

    [Fact]
    public void Should_get_default_value_from_string_field()
    {
        var field =
            Fields.String(1, "1", Partitioning.Invariant,
                new StringFieldProperties { DefaultValue = "default" });

        Assert.Equal(JsonValue.Create("default"), DefaultValueFactory.CreateDefaultValue(field, now, language.Iso2Code));
    }

    [Fact]
    public void Should_get_default_value_from_string_field_if_localized()
    {
        var field =
            Fields.String(1, "1", Partitioning.Invariant,
                new StringFieldProperties
                {
                    DefaultValues = new LocalizedValue<string?>(new Dictionary<string, string?>
                    {
                        [language.Iso2Code] = null
                    }),
                    DefaultValue = "default"
                });

        Assert.Equal(JsonValue.Null, DefaultValueFactory.CreateDefaultValue(field, now, language.Iso2Code));
    }

    [Fact]
    public void Should_get_default_value_from_tags_field()
    {
        var field =
            Fields.Tags(1, "1", Partitioning.Invariant,
                new TagsFieldProperties { DefaultValue = ReadonlyList.Create("tag1", "tag2") });

        Assert.Equal(JsonValue.Array("tag1", "tag2"), DefaultValueFactory.CreateDefaultValue(field, now, language.Iso2Code));
    }

    [Fact]
    public void Should_get_default_value_from_tags_field_if_localized()
    {
        var field =
            Fields.Tags(1, "1", Partitioning.Invariant,
                new TagsFieldProperties
                {
                    DefaultValues = new LocalizedValue<ReadonlyList<string>?>(new Dictionary<string, ReadonlyList<string>?>
                    {
                        [language.Iso2Code] = null
                    }),
                    DefaultValue = ReadonlyList.Create("tag1", "tag2")
                });

        Assert.Equal(new JsonArray(), DefaultValueFactory.CreateDefaultValue(field, now, language.Iso2Code));
    }

    private Instant FutureDays(int days)
    {
        return now.WithoutMs().Plus(Duration.FromDays(days));
    }
}
