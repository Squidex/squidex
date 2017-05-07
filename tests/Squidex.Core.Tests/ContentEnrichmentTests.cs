// ==========================================================================
//  ContentEnrichmentTests.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using Moq;
using NodaTime;
using NodaTime.Text;
using Squidex.Core.Contents;
using Squidex.Core.Schemas;
using Squidex.Infrastructure;
using Xunit;

namespace Squidex.Core
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
                    .AddOrUpdateField(new JsonField(1, "my-json",
                        new JsonFieldProperties()))
                    .AddOrUpdateField(new StringField(2, "my-string",
                        new StringFieldProperties { DefaultValue = "EN-String", IsLocalizable = true }))
                    .AddOrUpdateField(new NumberField(3, "my-number",
                        new NumberFieldProperties { DefaultValue = 123 }))
                    .AddOrUpdateField(new BooleanField(4, "my-boolean",
                        new BooleanFieldProperties { DefaultValue = true }))
                    .AddOrUpdateField(new DateTimeField(5, "my-datetime",
                        new DateTimeFieldProperties { DefaultValue = now }))
                    .AddOrUpdateField(new GeolocationField(6, "my-geolocation",
                        new GeolocationFieldProperties()))
                    .AddOrUpdateField(new AssetsField(7, "my-assets",
                        new AssetsFieldProperties(), new Mock<IAssetTester>().Object));

            var data =
                new ContentData()
                    .AddField("my-string",
                        new ContentFieldData()
                            .AddValue("de", "DE-String"))
                    .AddField("my-number",
                        new ContentFieldData()
                            .AddValue("iv", 456));

            data.Enrich(schema, languagesConfig);

            Assert.Equal(456, (int)data["my-number"]["iv"]);

            Assert.Equal("DE-String", (string)data["my-string"]["de"]);
            Assert.Equal("EN-String", (string)data["my-string"]["en"]);

            Assert.Equal(now, InstantPattern.General.Parse((string)data["my-datetime"]["iv"]).Value);

            Assert.Equal(true, (bool)data["my-boolean"]["iv"]);
        }
    }
}
