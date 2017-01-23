// ==========================================================================
//  ContentDataTests.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System.Collections.Generic;
using FluentAssertions;
using Newtonsoft.Json.Linq;
using Squidex.Core.Schemas;
using Squidex.Infrastructure;
using Xunit;

namespace Squidex.Core.Contents
{
    public class ContentDataTests
    {
        private readonly Schema schema =
            Schema.Create("schema", new SchemaProperties())
                .AddOrUpdateField(
                    new NumberField(1, "field1", new NumberFieldProperties { IsLocalizable = true }))
                .AddOrUpdateField(
                    new NumberField(2, "field2", new NumberFieldProperties { IsLocalizable = false }))
                .AddOrUpdateField(
                    new NumberField(3, "field3", new NumberFieldProperties { IsLocalizable = false }))
                .HideField(3);
        private readonly Language[] languages = { Language.GetLanguage("de"), Language.GetLanguage("en") };
        private readonly Language masterLanguage = Language.GetLanguage("en");

        [Fact]
        public void Should_convert_to_id_model()
        {
            var input =
               new ContentData()
                    .AddField("field1",
                        new ContentFieldData()
                            .AddValue("en", "en_string")
                            .AddValue("de", "de_string"))
                    .AddField("field2",
                        new ContentFieldData()
                            .AddValue("iv", 3));

            var actual = input.ToIdModel(schema);

            var expected =
                new ContentData()
                    .AddField("1",
                        new ContentFieldData()
                            .AddValue("en", "en_string")
                            .AddValue("de", "de_string"))
                    .AddField("2",
                        new ContentFieldData()
                            .AddValue("iv", 3));
            
            actual.ShouldBeEquivalentTo(expected);
        }

        [Fact]
        public void Should_convert_to_from_id_model()
        {
            var input =
               new ContentData()
                    .AddField("1",
                        new ContentFieldData()
                            .AddValue("en", "en_string")
                            .AddValue("de", "de_string"))
                    .AddField("2",
                        new ContentFieldData()
                            .AddValue("iv", 3));

            var actual = input.ToNameModel(schema);

            var expected =
                new ContentData()
                    .AddField("field1",
                        new ContentFieldData()
                            .AddValue("en", "en_string")
                            .AddValue("de", "de_string"))
                    .AddField("field2",
                        new ContentFieldData()
                            .AddValue("iv", 3));

            actual.ShouldBeEquivalentTo(expected);
        }

        [Fact]
        public void Should_cleanup_old_fields()
        {
            var expected =
                new Dictionary<string, Dictionary<string, JToken>>
                {
                    ["field1"] = new Dictionary<string, JToken>
                    {
                        ["en"] = "en_string",
                        ["de"] = "de_string"
                    }
                };

            var input =
                new ContentData()
                    .AddField("field0",
                        new ContentFieldData()
                            .AddValue("en", "en_string"))
                    .AddField("field1",
                        new ContentFieldData()
                            .AddValue("en", "en_string")
                            .AddValue("de", "de_string"));

            var output = input.ToApiModel(schema, languages, masterLanguage);

            output.ShouldBeEquivalentTo(expected);
        }

        [Fact]
        public void Should_cleanup_old_languages()
        {
            var expected =
                new Dictionary<string, Dictionary<string, JToken>>
                {
                    ["field1"] = new Dictionary<string, JToken>
                    {
                        ["en"] = "en_string",
                        ["de"] = "de_string"
                    }
                };

            var input =
                new ContentData()
                    .AddField("field1",
                        new ContentFieldData()
                            .AddValue("en", "en_string")
                            .AddValue("de", "de_string")
                            .AddValue("it", "it_string"));

            var output = input.ToApiModel(schema, languages, masterLanguage);

            output.ShouldBeEquivalentTo(expected);
        }

        [Fact]
        public void Should_provide_invariant_from_master_language()
        {
            var expected =
                new Dictionary<string, Dictionary<string, JToken>>
                {
                    ["field2"] = new Dictionary<string, JToken>
                    {
                        ["iv"] = 3
                    }
                };

            var input =
                new ContentData()
                    .AddField("field2",
                        new ContentFieldData()
                            .AddValue("de", 2)
                            .AddValue("en", 3));

            var output = input.ToApiModel(schema, languages, masterLanguage);

            output.ShouldBeEquivalentTo(expected);
        }

        [Fact]
        public void Should_provide_invariant_from_first_language()
        {
            var expected =
                new Dictionary<string, Dictionary<string, JToken>>
                {
                    ["field2"] = new Dictionary<string, JToken>
                    {
                        ["iv"] = 2
                    }
                };

            var input =
                new ContentData()
                    .AddField("field2",
                        new ContentFieldData()
                            .AddValue("de", 2)
                            .AddValue("it", 3));

            var output = input.ToApiModel(schema, languages, masterLanguage);

            output.ShouldBeEquivalentTo(expected);
        }

        [Fact]
        public void Should_not_include_hidden_field()
        {
            var expected =
                new Dictionary<string, Dictionary<string, JToken>>
                {
                    ["field2"] = new Dictionary<string, JToken>
                    {
                        ["iv"] = 2
                    }
                };

            var input =
                new ContentData()
                    .AddField("field2",
                        new ContentFieldData()
                            .AddValue("iv", 2))
                    .AddField("field3",
                        new ContentFieldData()
                            .AddValue("iv", 2));

            var output = input.ToApiModel(schema, languages, masterLanguage);

            output.ShouldBeEquivalentTo(expected);
        }
    }
}
