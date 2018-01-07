// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using Newtonsoft.Json.Linq;
using NodaTime;
using Squidex.Domain.Apps.Core.Apps;
using Squidex.Domain.Apps.Core.Contents;
using Squidex.Domain.Apps.Core.EnrichContent;
using Squidex.Domain.Apps.Core.Schemas;
using Squidex.Infrastructure;
using Xunit;

#pragma warning disable xUnit2004 // Do not use equality check to test for boolean conditions

namespace Squidex.Domain.Apps.Core.Operations.EnrichContent
{
    public class ContentEnrichmentTests
    {
        private static readonly Instant Now = Instant.FromDateTimeUtc(new DateTime(2017, 10, 12, 16, 30, 10, DateTimeKind.Utc));
        private readonly LanguagesConfig languagesConfig = LanguagesConfig.Build(Language.DE, Language.EN);
        private readonly Schema schema;

        public ContentEnrichmentTests()
        {
            schema =
                new Schema("my-schema")
                    .AddField(new StringField(1, "my-string", Partitioning.Language,
                        new StringFieldProperties { DefaultValue = "en-string" }))
                    .AddField(new NumberField(2, "my-number", Partitioning.Invariant,
                        new NumberFieldProperties()))
                    .AddField(new DateTimeField(3, "my-datetime", Partitioning.Invariant,
                            new DateTimeFieldProperties { DefaultValue = Now }))
                    .AddField(new BooleanField(4, "my-boolean", Partitioning.Invariant,
                        new BooleanFieldProperties { DefaultValue = true }));
        }

        [Fact]
        private void Should_enrich_with_default_values()
        {
            var data =
                new NamedContentData()
                    .AddField("my-string",
                        new ContentFieldData()
                            .AddValue("de", "de-string"))
                    .AddField("my-number",
                        new ContentFieldData()
                            .AddValue("iv", 456));

            data.Enrich(schema, languagesConfig.ToResolver());

            Assert.Equal(456, (int)data["my-number"]["iv"]);

            Assert.Equal("de-string", (string)data["my-string"]["de"]);
            Assert.Equal("en-string", (string)data["my-string"]["en"]);

            Assert.Equal(Now.ToString(), (string)data["my-datetime"]["iv"]);

            Assert.True((bool)data["my-boolean"]["iv"]);
        }

        [Fact]
        private void Should_also_enrich_with_default_values_when_string_is_empty()
        {
            var data =
                new NamedContentData()
                    .AddField("my-string",
                        new ContentFieldData()
                            .AddValue("de", string.Empty))
                    .AddField("my-number",
                        new ContentFieldData()
                            .AddValue("iv", 456));

            data.Enrich(schema, languagesConfig.ToResolver());

            Assert.Equal("en-string", (string)data["my-string"]["de"]);
            Assert.Equal("en-string", (string)data["my-string"]["en"]);
        }

        [Fact]
        public void Should_get_default_value_from_assets_field()
        {
            var field =
                new AssetsField(1, "1", Partitioning.Invariant,
                    new AssetsFieldProperties());

            Assert.Equal(new JArray(), DefaultValueFactory.CreateDefaultValue(field, Now));
        }

        [Fact]
        public void Should_get_default_value_from_boolean_field()
        {
            var field =
                new BooleanField(1, "1", Partitioning.Invariant,
                    new BooleanFieldProperties { DefaultValue = true });

            Assert.Equal(true, DefaultValueFactory.CreateDefaultValue(field, Now));
        }

        [Fact]
        public void Should_get_default_value_from_datetime_field()
        {
            var field =
                new DateTimeField(1, "1", Partitioning.Invariant,
                    new DateTimeFieldProperties { DefaultValue = FutureDays(15) });

            Assert.Equal(FutureDays(15).ToString(), DefaultValueFactory.CreateDefaultValue(field, Now));
        }

        [Fact]
        public void Should_get_default_value_from_datetime_field_when_set_to_today()
        {
            var field =
                new DateTimeField(1, "1", Partitioning.Invariant,
                    new DateTimeFieldProperties { CalculatedDefaultValue = DateTimeCalculatedDefaultValue.Today });

            Assert.Equal("2017-10-12", DefaultValueFactory.CreateDefaultValue(field, Now));
        }

        [Fact]
        public void Should_get_default_value_from_datetime_field_when_set_to_now()
        {
            var field =
                new DateTimeField(1, "1", Partitioning.Invariant,
                    new DateTimeFieldProperties { CalculatedDefaultValue = DateTimeCalculatedDefaultValue.Now });

            Assert.Equal("2017-10-12T16:30:10Z", DefaultValueFactory.CreateDefaultValue(field, Now));
        }

        [Fact]
        public void Should_get_default_value_from_json_field()
        {
            var field =
                new JsonField(1, "1", Partitioning.Invariant,
                    new JsonFieldProperties());

            Assert.Equal(new JObject(), DefaultValueFactory.CreateDefaultValue(field, Now));
        }

        [Fact]
        public void Should_get_default_value_from_geolocation_field()
        {
            var field =
                new GeolocationField(1, "1", Partitioning.Invariant,
                    new GeolocationFieldProperties());

            Assert.Equal(JValue.CreateNull(), DefaultValueFactory.CreateDefaultValue(field, Now));
        }

        [Fact]
        public void Should_get_default_value_from_number_field()
        {
            var field =
                new NumberField(1, "1", Partitioning.Invariant,
                    new NumberFieldProperties { DefaultValue = 12 });

            Assert.Equal(12, DefaultValueFactory.CreateDefaultValue(field, Now));
        }

        [Fact]
        public void Should_get_default_value_from_references_field()
        {
            var field =
                new ReferencesField(1, "1", Partitioning.Invariant,
                    new ReferencesFieldProperties());

            Assert.Equal(new JArray(), DefaultValueFactory.CreateDefaultValue(field, Now));
        }

        [Fact]
        public void Should_get_default_value_from_string_field()
        {
            var field =
                new StringField(1, "1", Partitioning.Invariant,
                    new StringFieldProperties { DefaultValue = "default" });

            Assert.Equal("default", DefaultValueFactory.CreateDefaultValue(field, Now));
        }

        [Fact]
        public void Should_get_default_value_from_tags_field()
        {
            var field =
                new TagsField(1, "1", Partitioning.Invariant,
                    new TagsFieldProperties());

            Assert.Equal(new JArray(), DefaultValueFactory.CreateDefaultValue(field, Now));
        }

        private static Instant FutureDays(int days)
        {
            return Instant.FromDateTimeUtc(DateTime.UtcNow.Date.AddDays(days));
        }
    }
}
