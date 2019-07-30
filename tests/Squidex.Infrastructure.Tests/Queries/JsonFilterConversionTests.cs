﻿// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Collections.Generic;
using System.Linq;
using NJsonSchema;
using Squidex.Infrastructure.Json.Objects;
using Squidex.Infrastructure.Queries.Json;
using Squidex.Infrastructure.TestHelpers;
using Xunit;

namespace Squidex.Infrastructure.Queries
{
    public sealed class JsonFilterConversionTests
    {
        private readonly List<string> errors = new List<string>();
        private readonly JsonSchema schema = new JsonSchema();

        public JsonFilterConversionTests()
        {
            var nested = new JsonSchemaProperty { Title = "nested" };

            nested.Properties["property"] = new JsonSchemaProperty
            {
                Type = JsonObjectType.String
            };

            schema.Properties["boolean"] = new JsonSchemaProperty
            {
                Type = JsonObjectType.Boolean
            };

            schema.Properties["datetime"] = new JsonSchemaProperty
            {
                Type = JsonObjectType.String, Format = JsonFormatStrings.DateTime
            };

            schema.Properties["guid"] = new JsonSchemaProperty
            {
                Type = JsonObjectType.String, Format = JsonFormatStrings.Guid
            };

            schema.Properties["integer"] = new JsonSchemaProperty
            {
                Type = JsonObjectType.Integer
            };

            schema.Properties["number"] = new JsonSchemaProperty
            {
                Type = JsonObjectType.Number
            };

            schema.Properties["string"] = new JsonSchemaProperty
            {
                Type = JsonObjectType.String
            };

            schema.Properties["stringArray"] = new JsonSchemaProperty
            {
                Item = new JsonSchema
                {
                    Type = JsonObjectType.String
                },
                Type = JsonObjectType.Array
            };

            schema.Properties["object"] = nested;
        }

        [Theory]
        [InlineData("contains", "contains(datetime, 2012-11-10T09:08:07Z)")]
        [InlineData("empty", "empty(datetime)")]
        [InlineData("endswith", "endsWith(datetime, 2012-11-10T09:08:07Z)")]
        [InlineData("eq", "datetime == 2012-11-10T09:08:07Z")]
        [InlineData("ge", "datetime >= 2012-11-10T09:08:07Z")]
        [InlineData("gt", "datetime > 2012-11-10T09:08:07Z")]
        [InlineData("le", "datetime <= 2012-11-10T09:08:07Z")]
        [InlineData("lt", "datetime < 2012-11-10T09:08:07Z")]
        [InlineData("ne", "datetime != 2012-11-10T09:08:07Z")]
        [InlineData("startswith", "startsWith(datetime, 2012-11-10T09:08:07Z)")]
        public void Should_parse_datetime_string_filter(string op, string expected)
        {
            var json = new { path = "datetime", op, value = "2012-11-10T09:08:07Z" };

            AssertFilter(json, expected);
        }

        [Fact]
        public void Should_add_error_if_datetime_string_property_got_invalid_string_value()
        {
            var json = new { path = "datetime", op = "eq", value = "invalid" };

            AssertErrors(json, "Expected ISO8601 DateTime String for path 'datetime', but got invalid String.");
        }

        [Fact]
        public void Should_add_error_if_datetime_string_property_got_invalid_value()
        {
            var json = new { path = "datetime", op = "eq", value = 1 };

            AssertErrors(json, "Expected ISO8601 DateTime String for path 'datetime', but got Number.");
        }

        [Theory]
        [InlineData("contains", "contains(guid, bf57d32c-d4dd-4217-8c16-6dcb16975cf3)")]
        [InlineData("empty", "empty(guid)")]
        [InlineData("endswith", "endsWith(guid, bf57d32c-d4dd-4217-8c16-6dcb16975cf3)")]
        [InlineData("eq", "guid == bf57d32c-d4dd-4217-8c16-6dcb16975cf3")]
        [InlineData("ge", "guid >= bf57d32c-d4dd-4217-8c16-6dcb16975cf3")]
        [InlineData("gt", "guid > bf57d32c-d4dd-4217-8c16-6dcb16975cf3")]
        [InlineData("le", "guid <= bf57d32c-d4dd-4217-8c16-6dcb16975cf3")]
        [InlineData("lt", "guid < bf57d32c-d4dd-4217-8c16-6dcb16975cf3")]
        [InlineData("ne", "guid != bf57d32c-d4dd-4217-8c16-6dcb16975cf3")]
        [InlineData("startswith", "startsWith(guid, bf57d32c-d4dd-4217-8c16-6dcb16975cf3)")]
        public void Should_parse_guid_string_filter(string op, string expected)
        {
            var json = new { path = "guid", op, value = "bf57d32c-d4dd-4217-8c16-6dcb16975cf3" };

            AssertFilter(json, expected);
        }

        [Fact]
        public void Should_add_error_if_guid_string_property_got_invalid_string_value()
        {
            var json = new { path = "guid", op = "eq", value = "invalid" };

            AssertErrors(json, "Expected Guid String for path 'guid', but got invalid String.");
        }

        [Fact]
        public void Should_add_error_if_guid_string_property_got_invalid_value()
        {
            var json = new { path = "guid", op = "eq", value = 1 };

            AssertErrors(json, "Expected Guid String for path 'guid', but got Number.");
        }

        [Theory]
        [InlineData("contains", "contains(string, 'Hello')")]
        [InlineData("empty", "empty(string)")]
        [InlineData("endswith", "endsWith(string, 'Hello')")]
        [InlineData("eq", "string == 'Hello'")]
        [InlineData("ge", "string >= 'Hello'")]
        [InlineData("gt", "string > 'Hello'")]
        [InlineData("le", "string <= 'Hello'")]
        [InlineData("lt", "string < 'Hello'")]
        [InlineData("ne", "string != 'Hello'")]
        [InlineData("startswith", "startsWith(string, 'Hello')")]
        public void Should_parse_string_filter(string op, string expected)
        {
            var json = new { path = "string", op, value = "Hello" };

            AssertFilter(json, expected);
        }

        [Fact]
        public void Should_add_error_if_string_property_got_invalid_value()
        {
            var json = new { path = "string", op = "eq", value = 1 };

            AssertErrors(json, "Expected String for path 'string', but got Number.");
        }

        [Fact]
        public void Should_parse_string_in_filter()
        {
            var json = new { path = "string", op = "in", value = new[] { "Hello" } };

            AssertFilter(json, "string in ['Hello']");
        }

        [Fact]
        public void Should_parse_nested_string_filter()
        {
            var json = new { path = "object.property", op = "in", value = new[] { "Hello" } };

            AssertFilter(json, "object.property in ['Hello']");
        }

        [Theory]
        [InlineData("eq", "number == 12")]
        [InlineData("ge", "number >= 12")]
        [InlineData("gt", "number > 12")]
        [InlineData("le", "number <= 12")]
        [InlineData("lt", "number < 12")]
        [InlineData("ne", "number != 12")]
        public void Should_parse_number_filter(string op, string expected)
        {
            var json = new { path = "number", op, value = 12 };

            AssertFilter(json, expected);
        }

        [Fact]
        public void Should_add_error_if_number_property_got_invalid_value()
        {
            var json = new { path = "number", op = "eq", value = true };

            AssertErrors(json, "Expected Number for path 'number', but got Boolean.");
        }

        [Fact]
        public void Should_parse_number_in_filter()
        {
            var json = new { path = "number", op = "in", value = new[] { 12 } };

            AssertFilter(json, "number in [12]");
        }

        [Theory]
        [InlineData("eq", "boolean == True")]
        [InlineData("ne", "boolean != True")]
        public void Should_parse_boolean_filter(string op, string expected)
        {
            var json = new { path = "boolean", op, value = true };

            AssertFilter(json, expected);
        }

        [Fact]
        public void Should_add_error_if_boolean_property_got_invalid_value()
        {
            var json = new { path = "boolean", op = "eq", value = 1 };

            AssertErrors(json, "Expected Boolean for path 'boolean', but got Number.");
        }

        [Fact]
        public void Should_parse_boolean_in_filter()
        {
            var json = new { path = "boolean", op = "in", value = new[] { true } };

            AssertFilter(json, "boolean in [True]");
        }

        [Theory]
        [InlineData("empty", "empty(stringArray)")]
        [InlineData("eq", "stringArray == 'Hello'")]
        [InlineData("ne", "stringArray != 'Hello'")]
        public void Should_parse_array_filter(string op, string expected)
        {
            var json = new { path = "stringArray", op, value = "Hello" };

            AssertFilter(json, expected);
        }

        [Fact]
        public void Should_parse_array_in_filter()
        {
            var json = new { path = "stringArray", op = "in", value = new[] { "Hello" } };

            AssertFilter(json, "stringArray in ['Hello']");
        }

        [Fact]
        public void Should_add_error_when_using_array_value_for_non_allowed_operator()
        {
            var json = new { path = "string", op = "eq", value = new[] { "Hello" } };

            AssertErrors(json, "Array value is not allowed for 'Equals' operator and path 'string'.");
        }

        private void AssertFilter(object json, string expectedFilter)
        {
            var filter = Convert(json);

            Assert.Empty(errors);

            Assert.Equal(expectedFilter, filter);
        }

        private void AssertErrors(object json, params string[] expectedErrors)
        {
            var filter = Convert(json);

            Assert.Equal(expectedErrors.ToList(), errors);

            Assert.Null(filter);
        }

        private string Convert<T>(T value)
        {
            var json = JsonHelper.DefaultSerializer.Serialize(value, true);

            var jsonFilter = JsonHelper.DefaultSerializer.Deserialize<FilterNode<IJsonValue>>(json);

            return JsonFilterVisitor.Parse(jsonFilter, schema, errors)?.ToString();
        }
    }
}
