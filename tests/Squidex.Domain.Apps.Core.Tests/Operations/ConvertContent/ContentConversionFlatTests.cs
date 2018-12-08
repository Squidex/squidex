// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Collections.Generic;
using Squidex.Domain.Apps.Core.Apps;
using Squidex.Domain.Apps.Core.Contents;
using Squidex.Domain.Apps.Core.ConvertContent;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Json.Objects;
using Xunit;

#pragma warning disable xUnit2013 // Do not use equality check to check for collection size.

namespace Squidex.Domain.Apps.Core.Operations.ConvertContent
{
    public class ContentConversionFlatTests
    {
        private readonly LanguagesConfig languagesConfig = LanguagesConfig.Build(Language.EN, Language.DE);

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
                            .AddValue("de", JsonValue.Null)
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
                {
                    "field1",
                    new ContentFieldData()
                        .AddValue("de", 1)
                        .AddValue("en", 2)
                },
                {
                    "field2",
                    new ContentFieldData()
                        .AddValue("de", JsonValue.Null)
                        .AddValue("en", 4)
                },
                { "field3", JsonValue.Create(6) },
                { "field4", JsonValue.Create(7) }
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
                            .AddValue("de", JsonValue.Null)
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

            var output = (Dictionary<string, IJsonValue>)data.ToFlatLanguageModel(fallbackConfig, new List<Language> { Language.DE });

            var expected = new Dictionary<string, IJsonValue>
            {
                { "field1", JsonValue.Create(1) },
                { "field2", JsonValue.Create(4) },
                { "field3", JsonValue.Create(6) }
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
                            .AddValue("de", JsonValue.Null)
                            .AddValue("en", 4))
                    .AddField("field3",
                        new ContentFieldData()
                            .AddValue("en", 6))
                    .AddField("field4",
                        new ContentFieldData()
                            .AddValue("it", 7));

            var output = (Dictionary<string, IJsonValue>)data.ToFlatLanguageModel(languagesConfig, new List<Language> { Language.DE, Language.EN });

            var expected = new Dictionary<string, IJsonValue>
            {
                { "field1", JsonValue.Create(1) },
                { "field2", JsonValue.Create(4) },
                { "field3", JsonValue.Create(6) }
            };

            Assert.True(expected.EqualsDictionary(output));
        }
    }
}
