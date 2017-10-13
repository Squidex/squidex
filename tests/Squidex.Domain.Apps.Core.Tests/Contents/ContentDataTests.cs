// ==========================================================================
//  ContentDataTests.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;
using Squidex.Domain.Apps.Core.Schemas;
using Squidex.Infrastructure;
using Xunit;

#pragma warning disable xUnit2013 // Do not use equality check to check for collection size.

namespace Squidex.Domain.Apps.Core.Contents
{
    public class ContentDataTests
    {
        private readonly Schema schema =
            Schema.Create("schema", new SchemaProperties())
                .Add(new NumberField(1, "field1", Partitioning.Language))
                .Add(new NumberField(2, "field2", Partitioning.Invariant))
                .Add(new NumberField(3, "field3", Partitioning.Invariant).Hide())
                .Add(new AssetsField(5, "assets1", Partitioning.Invariant))
                .Add(new AssetsField(6, "assets2", Partitioning.Invariant))
                .Add(new JsonField(4, "json", Partitioning.Language));
        private readonly LanguagesConfig languagesConfig = LanguagesConfig.Create(Language.EN, Language.DE);

        [Fact]
        public void Should_convert_to_id_model()
        {
            var input =
               new NamedContentData()
                    .Add("field1",
                        new ContentFieldData()
                            .AddValue("en", "en_string")
                            .AddValue("de", "de_string"))
                    .Add("field2",
                        new ContentFieldData()
                            .AddValue("iv", 3))
                    .Add("invalid",
                        new ContentFieldData()
                            .AddValue("iv", 3));

            var actual = input.ToIdModel(schema, false);

            var expected =
                new IdContentData()
                    .Add(1,
                        new ContentFieldData()
                            .AddValue("en", "en_string")
                            .AddValue("de", "de_string"))
                    .Add(2,
                        new ContentFieldData()
                            .AddValue("iv", 3));

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void Should_convert_to_encoded_id_model()
        {
            var input =
               new NamedContentData()
                    .Add("json",
                        new ContentFieldData()
                            .AddValue("en", new JObject())
                            .AddValue("de", null)
                            .AddValue("it", JValue.CreateNull()));

            var actual = input.ToIdModel(schema, true);

            var expected =
                new IdContentData()
                    .Add(4,
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
                    .Add(1,
                        new ContentFieldData()
                            .AddValue("en", "en_string")
                            .AddValue("de", "de_string"))
                    .Add(2,
                        new ContentFieldData()
                            .AddValue("iv", 3))
                    .Add(99,
                        new ContentFieldData()
                            .AddValue("iv", 3));

            var actual = input.ToNameModel(schema, false);

            var expected =
                new NamedContentData()
                    .Add("field1",
                        new ContentFieldData()
                            .AddValue("en", "en_string")
                            .AddValue("de", "de_string"))
                    .Add("field2",
                        new ContentFieldData()
                            .AddValue("iv", 3));

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void Should_convert_to_encoded_name_model()
        {
            var input =
               new IdContentData()
                    .Add(4,
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
                    .Add("field0",
                        new ContentFieldData()
                            .AddValue("en", "en_string"))
                    .Add("field1",
                        new ContentFieldData()
                            .AddValue("en", "en_string")
                            .AddValue("de", "de_string"));

            var actual = input.ToApiModel(schema, languagesConfig);

            var expected =
                new NamedContentData()
                    .Add("field1",
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
                    .Add("field1",
                        new ContentFieldData()
                            .AddValue("en", "en_string")
                            .AddValue("de", "de_string")
                            .AddValue("it", "it_string"));

            var actual = input.ToApiModel(schema, languagesConfig);

            var expected =
                new NamedContentData()
                    .Add("field1",
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
                    .Add("field2",
                        new ContentFieldData()
                            .AddValue("de", 2)
                            .AddValue("en", 3));

            var actual = input.ToApiModel(schema, languagesConfig);

            var expected =
                new NamedContentData()
                    .Add("field2",
                        new ContentFieldData()
                            .AddValue("iv", 3));

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void Should_provide_master_language_from_invariant()
        {
            var input =
                new NamedContentData()
                    .Add("field1",
                        new ContentFieldData()
                            .AddValue("iv", 3));

            var actual = input.ToApiModel(schema, languagesConfig);

            var expected =
                new NamedContentData()
                    .Add("field1",
                        new ContentFieldData()
                            .AddValue("en", 3));

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void Should_remove_null_values_from_name_model_when_cleaning()
        {
            var input =
                new NamedContentData()
                    .Add("field1", null)
                    .Add("field2",
                        new ContentFieldData()
                            .AddValue("en", 2)
                            .AddValue("it", null));

            var actual = input.ToCleaned();

            var expected =
                new NamedContentData()
                    .Add("field2",
                        new ContentFieldData()
                            .AddValue("en", 2));

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void Should_remove_null_values_from_id_model_when_cleaning()
        {
            var input =
                new IdContentData()
                    .Add(1, null)
                    .Add(2,
                        new ContentFieldData()
                            .AddValue("en", 2)
                            .AddValue("it", null));

            var actual = input.ToCleaned();

            var expected =
                new IdContentData()
                    .Add(2,
                        new ContentFieldData()
                            .AddValue("en", 2));

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void Should_provide_invariant_from_first_language()
        {
            var input =
                new NamedContentData()
                    .Add("field2",
                        new ContentFieldData()
                            .AddValue("de", 2)
                            .AddValue("it", 3));

            var actual = input.ToApiModel(schema, languagesConfig);

            var expected =
                new NamedContentData()
                    .Add("field2",
                        new ContentFieldData()
                            .AddValue("iv", 2));

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void Should_not_include_hidden_field()
        {
            var input =
                new NamedContentData()
                    .Add("field2",
                        new ContentFieldData()
                            .AddValue("iv", 5))
                    .Add("field3",
                        new ContentFieldData()
                            .AddValue("iv", 2));

            var actual = input.ToApiModel(schema, languagesConfig);

            var expected =
                new NamedContentData()
                    .Add("field2",
                        new ContentFieldData()
                            .AddValue("iv", 5));

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void Should_return_original_when_no_language_preferences_defined()
        {
            var data =
                new NamedContentData()
                    .Add("field1",
                        new ContentFieldData()
                            .AddValue("iv", 1));

            Assert.Same(data, data.ToLanguageModel(languagesConfig));
        }

        [Fact]
        public void Should_return_flat_list_when_single_languages_specified()
        {
            var data =
                new NamedContentData()
                    .Add("field1",
                        new ContentFieldData()
                            .AddValue("de", 1)
                            .AddValue("en", 2))
                    .Add("field2",
                        new ContentFieldData()
                            .AddValue("de", null)
                            .AddValue("en", 4))
                    .Add("field3",
                        new ContentFieldData()
                            .AddValue("en", 6))
                    .Add("field4",
                        new ContentFieldData()
                            .AddValue("it", 7));

            var fallbackConfig =
                LanguagesConfig.Create(Language.DE).Add(Language.EN)
                    .Update(Language.DE, false, false, new[] { Language.EN });

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
                    .Add("field1",
                        new ContentFieldData()
                            .AddValue("de", 1)
                            .AddValue("en", 2))
                    .Add("field2",
                        new ContentFieldData()
                            .AddValue("de", null)
                            .AddValue("en", 4))
                    .Add("field3",
                        new ContentFieldData()
                            .AddValue("en", 6))
                    .Add("field4",
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
        public void Should_merge_two_name_models()
        {
            var lhs =
                new NamedContentData()
                    .Add("field1",
                        new ContentFieldData()
                            .AddValue("iv", 1))
                    .Add("field2",
                        new ContentFieldData()
                            .AddValue("de", 2));

            var rhs =
                new NamedContentData()
                    .Add("field2",
                        new ContentFieldData()
                            .AddValue("en", 3))
                    .Add("field3",
                        new ContentFieldData()
                            .AddValue("iv", 4));

            var expected =
                new NamedContentData()
                    .Add("field1",
                        new ContentFieldData()
                            .AddValue("iv", 1))
                    .Add("field2",
                        new ContentFieldData()
                            .AddValue("de", 2)
                            .AddValue("en", 3))
                    .Add("field3",
                        new ContentFieldData()
                            .AddValue("iv", 4));

            var actual = lhs.MergeInto(rhs);

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void Should_merge_two_id_models()
        {
            var lhs =
                new IdContentData()
                    .Add(1,
                        new ContentFieldData()
                            .AddValue("iv", 1))
                    .Add(2,
                        new ContentFieldData()
                            .AddValue("de", 2));

            var rhs =
                new IdContentData()
                    .Add(2,
                        new ContentFieldData()
                            .AddValue("en", 3))
                    .Add(3,
                        new ContentFieldData()
                            .AddValue("iv", 4));

            var expected =
                new IdContentData()
                    .Add(1,
                        new ContentFieldData()
                            .AddValue("iv", 1))
                    .Add(2,
                        new ContentFieldData()
                            .AddValue("de", 2)
                            .AddValue("en", 3))
                    .Add(3,
                        new ContentFieldData()
                            .AddValue("iv", 4));

            var actual = lhs.MergeInto(rhs);

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void Should_be_equal_when_data_have_same_structure()
        {
            var lhs =
                new NamedContentData()
                    .Add("field1",
                        new ContentFieldData()
                            .AddValue("iv", 2))
                    .Add("field2",
                        new ContentFieldData()
                            .AddValue("iv", 2));

            var rhs =
                new NamedContentData()
                    .Add("field1",
                        new ContentFieldData()
                            .AddValue("iv", 2))
                    .Add("field2",
                        new ContentFieldData()
                            .AddValue("iv", 2));

            Assert.True(lhs.Equals(rhs));
            Assert.True(lhs.Equals((object)rhs));
            Assert.Equal(lhs.GetHashCode(), rhs.GetHashCode());
        }

        [Fact]
        public void Should_not_be_equal_when_data_have_not_same_structure()
        {
            var lhs =
                new NamedContentData()
                    .Add("field1",
                        new ContentFieldData()
                            .AddValue("iv", 2))
                    .Add("field2",
                        new ContentFieldData()
                            .AddValue("iv", 2));

            var rhs =
                new NamedContentData()
                    .Add("field1",
                        new ContentFieldData()
                            .AddValue("en", 2))
                    .Add("field3",
                        new ContentFieldData()
                            .AddValue("iv", 2));

            Assert.False(lhs.Equals(rhs));
            Assert.False(lhs.Equals((object)rhs));
            Assert.NotEqual(lhs.GetHashCode(), rhs.GetHashCode());
        }

        [Fact]
        public void Should_remove_ids()
        {
            var id1 = Guid.NewGuid();
            var id2 = Guid.NewGuid();

            var input =
                new NamedContentData()
                    .Add("assets1",
                        new ContentFieldData()
                            .AddValue("iv", new JArray(id1.ToString(), id2.ToString())));

            var ids = input.GetReferencedIds(schema).ToArray();

            Assert.Equal(new[] { id1, id2 }, ids);
        }

        [Fact]
        public void Should_cleanup_deleted_ids()
        {
            var id1 = Guid.NewGuid();
            var id2 = Guid.NewGuid();

            var input =
                new IdContentData()
                    .Add(5,
                        new ContentFieldData()
                            .AddValue("iv", new JArray(id1.ToString(), id2.ToString())));

            var actual = input.ToCleanedReferences(schema, new HashSet<Guid>(new[] { id2 }));

            var cleanedValue = (JArray)actual[5]["iv"];

            Assert.Equal(1, cleanedValue.Count);
            Assert.Equal(id1.ToString(), cleanedValue[0]);
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
