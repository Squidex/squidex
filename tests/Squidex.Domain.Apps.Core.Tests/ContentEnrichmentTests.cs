// ==========================================================================
//  ContentEnrichmentTests.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System;
using NodaTime;
using NodaTime.Text;
using Squidex.Domain.Apps.Core.Contents;
using Squidex.Domain.Apps.Core.Schemas;
using Squidex.Infrastructure;
using Xunit;

namespace Squidex.Domain.Apps.Core
{
    public class ContentEnrichmentTests
    {
        private readonly LanguagesConfig languagesConfig = LanguagesConfig.Create(Language.DE, Language.EN);

        [Fact]
        private void Should_enrich_with_default_values()
        {
            var now = Instant.FromUnixTimeSeconds(SystemClock.Instance.GetCurrentInstant().ToUnixTimeSeconds());

            var schema =
                Schema.Create("my-schema", new SchemaProperties())
                    .AddOrUpdateField(new JsonField(1, "my-json", Partitioning.Invariant,
                        new JsonFieldProperties()))
                    .AddOrUpdateField(new StringField(2, "my-string", Partitioning.Language,
                        new StringFieldProperties { DefaultValue = "en-string" }))
                    .AddOrUpdateField(new NumberField(3, "my-number", Partitioning.Invariant,
                        new NumberFieldProperties { DefaultValue = 123 }))
                    .AddOrUpdateField(new AssetsField(4, "my-assets", Partitioning.Invariant,
                        new AssetsFieldProperties()))
                    .AddOrUpdateField(new BooleanField(5, "my-boolean", Partitioning.Invariant,
                        new BooleanFieldProperties { DefaultValue = true }))
                    .AddOrUpdateField(new DateTimeField(6, "my-datetime", Partitioning.Invariant,
                        new DateTimeFieldProperties { DefaultValue = now }))
                    .AddOrUpdateField(new ReferencesField(7, "my-references", Partitioning.Invariant,
                        new ReferencesFieldProperties { SchemaId = Guid.NewGuid() }))
                    .AddOrUpdateField(new GeolocationField(8, "my-geolocation", Partitioning.Invariant,
                        new GeolocationFieldProperties()));

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

            Assert.Equal(now, InstantPattern.General.Parse((string)data["my-datetime"]["iv"]).Value);

            Assert.Equal(true, (bool)data["my-boolean"]["iv"]);
        }
    }
}
