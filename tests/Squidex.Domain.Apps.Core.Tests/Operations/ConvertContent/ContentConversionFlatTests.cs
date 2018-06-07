// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using Squidex.Domain.Apps.Core.Apps;
using Squidex.Domain.Apps.Core.Contents;
using Squidex.Domain.Apps.Core.ConvertContent;
using Squidex.Domain.Apps.Core.Schemas;
using Squidex.Infrastructure;
using Xunit;

#pragma warning disable xUnit2013 // Do not use equality check to check for collection size.

namespace Squidex.Domain.Apps.Core.Operations.ConvertContent
{
    public class ContentConversionFlatTests
    {
        private readonly Schema schema;
        private readonly LanguagesConfig languagesConfig = LanguagesConfig.Build(Language.EN, Language.DE);

        public ContentConversionFlatTests()
        {
            schema =
                new Schema("my-schema")
                    .AddNumber(1, "field1", Partitioning.Language)
                    .AddNumber(2, "field2", Partitioning.Invariant)
                    .AddNumber(3, "field3", Partitioning.Invariant)
                    .AddAssets(5, "assets1", Partitioning.Invariant)
                    .AddAssets(6, "assets2", Partitioning.Invariant)
                    .AddJson(4, "json", Partitioning.Language)
                    .UpdateField(3, f => f.Hide());
        }

        [Fact]
        public void Should_return_original_when_no_language_preferences_defined()
        {
            var data =
                new NamedContentData()
                    .AddField("field1",
                        new ContentFieldData()
                            .AddValue("iv", 1));

            Assert.Same(data, data.ToFlatLanguageModel(languagesConfig));
        }

        [Fact]
        public void Should_return_flatten_value()
        {
            var data =
                new NamedContentData()
                    .AddField("field1",
                        new ContentFieldData()
                            .AddValue("de", 1)
                            .AddValue("en", 2))
                    .AddField("field2",
                        new ContentFieldData()
                            .AddValue("de", null)
                            .AddValue("en", 4))
                    .AddField("field3",
                        new ContentFieldData()
                            .AddValue("en", 6))
                    .AddField("field4",
                        new ContentFieldData()
                            .AddValue("it", 7));

            var output = data.ToFlatten();

            var expected = new Dictionary<string, object>
            {
                { "field1", new ContentFieldData().AddValue("de", 1).AddValue("en", 2) },
                { "field2", new ContentFieldData().AddValue("de", null).AddValue("en", 4) },
                { "field3", (JValue)6 },
                { "field4", (JValue)7 }
            };

            Assert.True(expected.EqualsDictionary(output));
        }

        [Fact]
        public void Should_return_flat_list_when_single_languages_specified()
        {
            var data =
                new NamedContentData()
                    .AddField("field1",
                        new ContentFieldData()
                            .AddValue("de", 1)
                            .AddValue("en", 2))
                    .AddField("field2",
                        new ContentFieldData()
                            .AddValue("de", null)
                            .AddValue("en", 4))
                    .AddField("field3",
                        new ContentFieldData()
                            .AddValue("en", 6))
                    .AddField("field4",
                        new ContentFieldData()
                            .AddValue("it", 7));

            var fallbackConfig =
                LanguagesConfig.Build(
                    new LanguageConfig(Language.EN),
                    new LanguageConfig(Language.DE, false, Language.EN));

            var output = (Dictionary<string, JToken>)data.ToFlatLanguageModel(fallbackConfig, new List<Language> { Language.DE });

            var expected = new Dictionary<string, JToken>
            {
                { "field1", 1 },
                { "field2", 4 },
                { "field3", 6 }
            };

            Assert.True(expected.EqualsDictionary(output));
        }

        [Fact]
        public void Should_return_flat_list_when_languages_specified()
        {
            var data =
                new NamedContentData()
                    .AddField("field1",
                        new ContentFieldData()
                            .AddValue("de", 1)
                            .AddValue("en", 2))
                    .AddField("field2",
                        new ContentFieldData()
                            .AddValue("de", null)
                            .AddValue("en", 4))
                    .AddField("field3",
                        new ContentFieldData()
                            .AddValue("en", 6))
                    .AddField("field4",
                        new ContentFieldData()
                            .AddValue("it", 7));

            var output = (Dictionary<string, JToken>)data.ToFlatLanguageModel(languagesConfig, new List<Language> { Language.DE, Language.EN });

            var expected = new Dictionary<string, JToken>
            {
                { "field1", 1 },
                { "field2", 4 },
                { "field3", 6 }
            };

            Assert.True(expected.EqualsDictionary(output));
        }
    }
}
