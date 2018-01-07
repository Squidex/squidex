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
    public class ContentConversionTests
    {
        private readonly Schema schema;
        private readonly LanguagesConfig languagesConfig = LanguagesConfig.Build(Language.EN, Language.DE);

        public ContentConversionTests()
        {
            schema =
                new Schema("my-schema")
                    .AddField(new NumberField(1, "field1", Partitioning.Language))
                    .AddField(new NumberField(2, "field2", Partitioning.Invariant))
                    .AddField(new NumberField(3, "field3", Partitioning.Invariant))
                    .AddField(new AssetsField(5, "assets1", Partitioning.Invariant))
                    .AddField(new AssetsField(6, "assets2", Partitioning.Invariant))
                    .AddField(new JsonField(4, "json", Partitioning.Language))
                    .HideField(3);
        }

        [Fact]
        public void Should_convert_to_id_model()
        {
            var input =
               new NamedContentData()
                    .AddField("field1",
                        new ContentFieldData()
                            .AddValue("en", "en_string")
                            .AddValue("de", "de_string"))
                    .AddField("field2",
                        new ContentFieldData()
                            .AddValue("iv", 3))
                    .AddField("invalid",
                        new ContentFieldData()
                            .AddValue("iv", 3));

            var actual = input.ToIdModel(schema, false);

            var expected =
                new IdContentData()
                    .AddField(1,
                        new ContentFieldData()
                            .AddValue("en", "en_string")
                            .AddValue("de", "de_string"))
                    .AddField(2,
                        new ContentFieldData()
                            .AddValue("iv", 3));

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void Should_convert_to_encoded_id_model()
        {
            var input =
               new NamedContentData()
                    .AddField("json",
                        new ContentFieldData()
                            .AddValue("en", new JObject())
                            .AddValue("de", null)
                            .AddValue("it", JValue.CreateNull()));

            var actual = input.ToIdModel(schema, true);

            var expected =
                new IdContentData()
                    .AddField(4,
                        new ContentFieldData()
                            .AddValue("en", "e30=")
                            .AddValue("de", null)
                            .AddValue("it", null));

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void Should_convert_to_name_model()
        {
            var input =
               new IdContentData()
                    .AddField(1,
                        new ContentFieldData()
                            .AddValue("en", "en_string")
                            .AddValue("de", "de_string"))
                    .AddField(2,
                        new ContentFieldData()
                            .AddValue("iv", 3))
                    .AddField(99,
                        new ContentFieldData()
                            .AddValue("iv", 3));

            var actual = input.ToNameModel(schema, false);

            var expected =
                new NamedContentData()
                    .AddField("field1",
                        new ContentFieldData()
                            .AddValue("en", "en_string")
                            .AddValue("de", "de_string"))
                    .AddField("field2",
                        new ContentFieldData()
                            .AddValue("iv", 3));

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void Should_convert_to_encoded_name_model()
        {
            var input =
               new IdContentData()
                    .AddField(4,
                        new ContentFieldData()
                            .AddValue("en", "e30=")
                            .AddValue("de", null)
                            .AddValue("it", null));

            var actual = input.ToNameModel(schema, true);

            Assert.True(actual["json"]["en"] is JObject);
        }

        [Fact]
        public void Should_cleanup_old_fields()
        {
            var input =
                new NamedContentData()
                    .AddField("field0",
                        new ContentFieldData()
                            .AddValue("en", "en_string"))
                    .AddField("field1",
                        new ContentFieldData()
                            .AddValue("en", "en_string")
                            .AddValue("de", "de_string"));

            var actual = input.ToApiModel(schema, languagesConfig);

            var expected =
                new NamedContentData()
                    .AddField("field1",
                        new ContentFieldData()
                            .AddValue("en", "en_string")
                            .AddValue("de", "de_string"));

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void Should_cleanup_old_languages()
        {
            var input =
                new NamedContentData()
                    .AddField("field1",
                        new ContentFieldData()
                            .AddValue("en", "en_string")
                            .AddValue("de", "de_string")
                            .AddValue("it", "it_string"));

            var actual = input.ToApiModel(schema, languagesConfig);

            var expected =
                new NamedContentData()
                    .AddField("field1",
                        new ContentFieldData()
                            .AddValue("en", "en_string")
                            .AddValue("de", "de_string"));

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void Should_provide_invariant_from_master_language()
        {
            var input =
                new NamedContentData()
                    .AddField("field2",
                        new ContentFieldData()
                            .AddValue("de", 2)
                            .AddValue("en", 3));

            var actual = input.ToApiModel(schema, languagesConfig);

            var expected =
                new NamedContentData()
                    .AddField("field2",
                        new ContentFieldData()
                            .AddValue("iv", 3));

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void Should_provide_master_language_from_invariant()
        {
            var input =
                new NamedContentData()
                    .AddField("field1",
                        new ContentFieldData()
                            .AddValue("iv", 3));

            var actual = input.ToApiModel(schema, languagesConfig);

            var expected =
                new NamedContentData()
                    .AddField("field1",
                        new ContentFieldData()
                            .AddValue("en", 3));

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void Should_remove_null_values_from_name_model_when_cleaning()
        {
            var input =
                new NamedContentData()
                    .AddField("field1", null)
                    .AddField("field2",
                        new ContentFieldData()
                            .AddValue("en", 2)
                            .AddValue("it", null));

            var actual = input.ToCleaned();

            var expected =
                new NamedContentData()
                    .AddField("field2",
                        new ContentFieldData()
                            .AddValue("en", 2));

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void Should_remove_null_values_from_id_model_when_cleaning()
        {
            var input =
                new IdContentData()
                    .AddField(1, null)
                    .AddField(2,
                        new ContentFieldData()
                            .AddValue("en", 2)
                            .AddValue("it", null));

            var actual = input.ToCleaned();

            var expected =
                new IdContentData()
                    .AddField(2,
                        new ContentFieldData()
                            .AddValue("en", 2));

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void Should_provide_invariant_from_first_language()
        {
            var input =
                new NamedContentData()
                    .AddField("field2",
                        new ContentFieldData()
                            .AddValue("de", 2)
                            .AddValue("it", 3));

            var actual = input.ToApiModel(schema, languagesConfig);

            var expected =
                new NamedContentData()
                    .AddField("field2",
                        new ContentFieldData()
                            .AddValue("iv", 2));

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void Should_not_include_hidden_field()
        {
            var input =
                new NamedContentData()
                    .AddField("field2",
                        new ContentFieldData()
                            .AddValue("iv", 5))
                    .AddField("field3",
                        new ContentFieldData()
                            .AddValue("iv", 2));

            var actual = input.ToApiModel(schema, languagesConfig);

            var expected =
                new NamedContentData()
                    .AddField("field2",
                        new ContentFieldData()
                            .AddValue("iv", 5));

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void Should_return_original_when_no_language_preferences_defined()
        {
            var data =
                new NamedContentData()
                    .AddField("field1",
                        new ContentFieldData()
                            .AddValue("iv", 1));

            Assert.Same(data, data.ToLanguageModel(languagesConfig));
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

            var output = (Dictionary<string, JToken>)data.ToLanguageModel(fallbackConfig, new List<Language> { Language.DE });

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

            var output = (Dictionary<string, JToken>)data.ToLanguageModel(languagesConfig, new List<Language> { Language.DE, Language.EN });

            var expected = new Dictionary<string, JToken>
            {
                { "field1", 1 },
                { "field2", 4 },
                { "field3", 6 }
            };

            Assert.True(expected.EqualsDictionary(output));
        }

        [Fact]
        public void Should_be_equal_fields_when_they_have_same_value()
        {
            var lhs =
                new ContentFieldData()
                    .AddValue("iv", 2);

            var rhs =
                new ContentFieldData()
                    .AddValue("iv", 2);

            Assert.True(lhs.Equals(rhs));
            Assert.True(lhs.Equals((object)rhs));
            Assert.Equal(lhs.GetHashCode(), rhs.GetHashCode());
        }
    }
}
