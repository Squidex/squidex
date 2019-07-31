﻿// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Newtonsoft.Json;
using Squidex.Infrastructure.Json.Objects;
using Squidex.Infrastructure.TestHelpers;
using Xunit;

namespace Squidex.Infrastructure.Queries
{
    public class QueryJsonTests
    {
        [Theory]
        [InlineData("eq", "property == 12")]
        [InlineData("ne", "property != 12")]
        [InlineData("le", "property <= 12")]
        [InlineData("lt", "property < 12")]
        [InlineData("ge", "property >= 12")]
        [InlineData("gt", "property > 12")]
        [InlineData("contains", "contains(property, 12)")]
        [InlineData("endswith", "endsWith(property, 12)")]
        [InlineData("startswith", "startsWith(property, 12)")]
        public void Should_convert_comparison(string op, string expected)
        {
            var json = new
            {
                path = "property",
                op,
                value = 12
            };

            var filter = SerializeAndDeserialize(json);

            Assert.Equal(expected, filter.ToString());
        }

        [Fact]
        public void Should_convert_comparison_empty()
        {
            var json = new { path = "property", op = "empty" };

            var filter = SerializeAndDeserialize(json);

            Assert.Equal("empty(property)", filter.ToString());
        }

        [Fact]
        public void Should_convert_comparison_in()
        {
            var json = new { path = "property", op = "in", value = new[] { 12, 13 } };

            var filter = SerializeAndDeserialize(json);

            Assert.Equal("property in [12, 13]", filter.ToString());
        }

        [Fact]
        public void Should_convert_comparison_with_deep_path()
        {
            var json = new { path = "property.nested", op = "eq", value = 12 };

            var filter = SerializeAndDeserialize(json);

            Assert.Equal("property.nested == 12", filter.ToString());
        }

        [Fact]
        public void Should_convert_logical_and()
        {
            var json = new
            {
                and = new[]
                {
                    new { path = "property", op = "ge", value = 10 },
                    new { path = "property", op = "lt", value = 20 }
                }
            };

            var filter = SerializeAndDeserialize(json);

            Assert.Equal("(property >= 10 && property < 20)", filter.ToString());
        }

        [Fact]
        public void Should_convert_logical_or()
        {
            var json = new
            {
                or = new[]
                {
                    new { path = "property", op = "ge", value = 10 },
                    new { path = "property", op = "lt", value = 20 }
                }
            };

            var filter = SerializeAndDeserialize(json);

            Assert.Equal("(property >= 10 || property < 20)", filter.ToString());
        }

        [Fact]
        public void Should_convert_logical_not()
        {
            var json = new
            {
                not = new { path = "property", op = "ge", value = 10 }
            };

            var filter = SerializeAndDeserialize(json);

            Assert.Equal("!(property >= 10)", filter.ToString());
        }

        [Fact]
        public void Should_throw_exception_for_invalid_operator()
        {
            var json = new { path = "property", op = "invalid", value = 12 };

            Assert.ThrowsAny<JsonException>(() => SerializeAndDeserialize(json));
        }

        [Fact]
        public void Should_throw_exception_for_missing_path()
        {
            var json = new { op = "invalid", value = 12 };

            Assert.ThrowsAny<JsonException>(() => SerializeAndDeserialize(json));
        }

        [Fact]
        public void Should_throw_exception_for_missing_operator()
        {
            var json = new { path = "property", value = 12 };

            Assert.ThrowsAny<JsonException>(() => SerializeAndDeserialize(json));
        }

        [Fact]
        public void Should_throw_exception_for_missing_value()
        {
            var json = new { path = "property", op = "invalid" };

            Assert.ThrowsAny<JsonException>(() => SerializeAndDeserialize(json));
        }

        [Fact]
        public void Should_throw_exception_for_invalid_property()
        {
            var json = new { path = "property", op = "invalid", value = 12, other = 4 };

            Assert.ThrowsAny<JsonException>(() => SerializeAndDeserialize(json));
        }

        [Fact]
        public void Should_throw_exception_for_invalid_property_after_filter()
        {
            var json = new
            {
                and = new[]
                {
                    new { path = "property", op = "ge", value = 10 },
                    new { path = "property", op = "lt", value = 20 }
                },
                additional = 1
            };

            Assert.ThrowsAny<JsonException>(() => SerializeAndDeserialize(json));
        }

        private static FilterNode<IJsonValue> SerializeAndDeserialize<T>(T value)
        {
            var json = JsonHelper.DefaultSerializer.Serialize(value, true);

            return JsonHelper.DefaultSerializer.Deserialize<FilterNode<IJsonValue>>(json);
        }
    }
}
