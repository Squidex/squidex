﻿// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Domain.Apps.Core.Contents;
using Squidex.Domain.Apps.Core.ConvertContent;
using Squidex.Domain.Apps.Core.Schemas;
using Squidex.Infrastructure.Json.Objects;
using Xunit;

namespace Squidex.Domain.Apps.Core.Operations.ConvertContent
{
    public class ContentConversionTests
    {
        private readonly Schema schema;

        public ContentConversionTests()
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
        public void Should_convert_name_to_id()
        {
            var input =
                new NamedContentData()
                    .AddField("field1",
                        new ContentFieldData()
                            .AddValue("en", "EN"))
                    .AddField("field2",
                        new ContentFieldData()
                            .AddValue("iv", 1))
                    .AddField("invalid",
                        new ContentFieldData()
                            .AddValue("iv", 2));

            var actual = input.ConvertName2Id(schema, (data, field) => field.Name == "field2" ? null : data);

            var expected =
                new IdContentData()
                    .AddField(1,
                        new ContentFieldData()
                            .AddValue("en", "EN"));

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void Should_convert_name_to_name()
        {
            var input =
                new NamedContentData()
                    .AddField("field1",
                        new ContentFieldData()
                            .AddValue("en", "EN"))
                    .AddField("field2",
                        new ContentFieldData()
                            .AddValue("iv", 1))
                    .AddField("invalid",
                        new ContentFieldData()
                            .AddValue("iv", 2));

            var actual = input.ConvertName2Name(schema, (data, field) => field.Name == "field2" ? null : data);

            var expected =
                new NamedContentData()
                    .AddField("field1",
                        new ContentFieldData()
                            .AddValue("en", "EN"));

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void Should_convert_id_to_id()
        {
            var input =
                new IdContentData()
                    .AddField(1,
                        new ContentFieldData()
                            .AddValue("en", "EN"))
                    .AddField(2,
                        new ContentFieldData()
                            .AddValue("iv", 1))
                    .AddField(99,
                        new ContentFieldData()
                            .AddValue("iv", 2));

            var actual = input.ConvertId2Id(schema, (data, field) => field.Name == "field2" ? null : data);

            var expected =
                new IdContentData()
                    .AddField(1,
                        new ContentFieldData()
                            .AddValue("en", "EN"));

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void Should_convert_id_to_name()
        {
            var input =
                new IdContentData()
                    .AddField(1,
                        new ContentFieldData()
                            .AddValue("en", "EN"))
                    .AddField(2,
                        new ContentFieldData()
                            .AddValue("iv", 1))
                    .AddField(99,
                        new ContentFieldData()
                            .AddValue("iv", 2));

            var actual = input.ConvertId2Name(schema, (data, field) => field.Name == "field2" ? null : data);

            var expected =
                new NamedContentData()
                    .AddField("field1",
                        new ContentFieldData()
                            .AddValue("en", "EN"));

            Assert.Equal(expected, actual);
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

        [Fact]
        public void Should_extract_strings()
        {
            var input =
                new NamedContentData()
                    .AddField("field1",
                        new ContentFieldData()
                            .AddValue("en", "hello"))
                    .AddField("field2",
                        new ContentFieldData()
                            .AddValue("iv", "world"));

            var result = input.ToFullText();

            Assert.Equal("hello world", result);
        }

        [Fact]
        public void Should_extract_strings_from_arrays()
        {
            var input =
                new NamedContentData()
                    .AddField("field1",
                        new ContentFieldData()
                            .AddValue("en", JsonValue.Array("hello", "loved")))
                    .AddField("field2",
                        new ContentFieldData()
                            .AddValue("iv", "world"));

            var result = input.ToFullText();

            Assert.Equal("hello loved world", result);
        }

        [Fact]
        public void Should_extract_strings_from_objects()
        {
            var input =
                new NamedContentData()
                    .AddField("field1",
                        new ContentFieldData()
                            .AddValue("en", JsonValue.Array(JsonValue.Object().Add("p1", "hello"))))
                    .AddField("field2",
                        new ContentFieldData()
                            .AddValue("iv", "world"));

            var result = input.ToFullText();

            Assert.Equal("hello world", result);
        }

        [Fact]
        public void Should_skip_long_strings()
        {
            var input =
                new NamedContentData()
                    .AddField("field1",
                        new ContentFieldData()
                            .AddValue("en", "hello"))
                    .AddField("field2",
                        new ContentFieldData()
                            .AddValue("iv", "you"));

            var result = input.ToFullText(maxFieldLength: 3);

            Assert.Equal("you", result);
        }

        [Fact]
        public void Should_trim_long_results()
        {
            var input =
                new NamedContentData()
                    .AddField("field1",
                        new ContentFieldData()
                            .AddValue("en", "hello"))
                    .AddField("field2",
                        new ContentFieldData()
                            .AddValue("iv", "you"));

            var result = input.ToFullText(maxTotalLength: 7);

            Assert.Equal("hello y", result);
        }
    }
}
