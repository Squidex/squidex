// ==========================================================================
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
using Squidex.Infrastructure.Validation;
using Xunit;

namespace Squidex.Infrastructure.Queries
{
    public sealed class JsonQueryConversionTests
    {
        private readonly List<string> errors = new List<string>();
        private readonly JsonSchema schema = new JsonSchema();

        public JsonQueryConversionTests()
        {
            var nested = new JsonSchemaProperty { Title = "nested", Type = JsonObjectType.Object };

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

            schema.Properties["reference"] = new JsonSchemaProperty
            {
                Reference = nested
            };
        }

        [Fact]
        public void Should_add_error_if_property_does_not_exist()
        {
            var json = new { path = "notfound", op = "eq", value = 1 };

            AssertErrors(json, "Path 'notfound' does not point to a valid property in the model.");
        }

        [Fact]
        public void Should_add_error_if_nested_property_does_not_exist()
        {
            var json = new { path = "object.notfound", op = "eq", value = 1 };

            AssertErrors(json, "'notfound' is not a property of 'nested'.");
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
        public void Should_parse_date_string_filter()
        {
            var json = new { path = "datetime", op = "eq", value = "2012-11-10" };

            AssertFilter(json, "datetime == 2012-11-10T00:00:00Z");
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

        [Fact]
        public void Should_parse_referenced_string_filter()
        {
            var json = new { path = "reference.property", op = "in", value = new[] { "Hello" } };

            AssertFilter(json, "reference.property in ['Hello']");
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

        [Fact]
        public void Should_parse_query()
        {
            var json = new { skip = 10, take = 20, FullText = "Hello", Filter = new { path = "string", op = "eq", value = "Hello" } };

            AssertQuery(json, "Filter: string == 'Hello'; FullText: 'Hello'; Skip: 10; Take: 20");
        }

        [Fact]
        public void Should_parse_query_with_sorting()
        {
            var json = new { sort = new[] { new { path = "string", order = "ascending" } } };

            AssertQuery(json, "Sort: string Ascending");
        }

        [Fact]
        public void Should_throw_exception_for_invalid_query()
        {
            var json = new { sort = new[] { new { path = "invalid", order = "ascending" } } };

            Assert.Throws<ValidationException>(() => AssertQuery(json, null));
        }

        [Fact]
        public void Should_throw_exception_when_parsing_invalid_json()
        {
            var json = "invalid";

            Assert.Throws<ValidationException>(() => AssertQuery(json, null));
        }

        [Fact]
        public void Should_not_throw_exception_when_parsing_null_string()
        {
            string? json = null;

            Assert.NotNull(schema.Parse(json!, JsonHelper.DefaultSerializer));
        }

        [Fact]
        public void Should_not_throw_exception_when_parsing_null_json()
        {
            var json = "null";

            Assert.NotNull(schema.Parse(json, JsonHelper.DefaultSerializer));
        }

        private void AssertQuery(object json, string? expectedFilter)
        {
            var filter = ConvertQuery(json);

            Assert.Empty(errors);

            Assert.Equal(expectedFilter, filter);
        }

        private void AssertFilter(object json, string? expectedFilter)
        {
            var filter = ConvertFilter(json);

            Assert.Empty(errors);

            Assert.Equal(expectedFilter, filter);
        }

        private void AssertErrors(object json, params string[] expectedErrors)
        {
            var filter = ConvertFilter(json);

            Assert.Equal(expectedErrors.ToList(), errors);

            Assert.Null(filter);
        }

        private string? ConvertFilter<T>(T value)
        {
            var json = JsonHelper.DefaultSerializer.Serialize(value, true);

            var jsonFilter = JsonHelper.DefaultSerializer.Deserialize<FilterNode<IJsonValue>>(json);

            return JsonFilterVisitor.Parse(jsonFilter, schema, errors)?.ToString();
        }

        private string? ConvertQuery<T>(T value)
        {
            var json = JsonHelper.DefaultSerializer.Serialize(value, true);

            var jsonQuery = schema.Parse(json, JsonHelper.DefaultSerializer);

            return jsonQuery.ToString();
        }
    }
}
