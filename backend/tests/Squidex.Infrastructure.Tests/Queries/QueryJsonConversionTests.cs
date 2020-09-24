// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
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
    public sealed class QueryJsonConversionTests
    {
        private static readonly (string Operator, string Output)[] AllOps =
        {
            ("contains", "contains($FIELD, $VALUE)"),
            ("empty", "empty($FIELD)"),
            ("endswith", "endsWith($FIELD, $VALUE)"),
            ("eq", "$FIELD == $VALUE"),
            ("ge", "$FIELD >= $VALUE"),
            ("gt", "$FIELD > $VALUE"),
            ("le", "$FIELD <= $VALUE"),
            ("lt", "$FIELD < $VALUE"),
            ("ne", "$FIELD != $VALUE"),
            ("startswith", "startsWith($FIELD, $VALUE)")
        };

        private readonly List<string> errors = new List<string>();
        private readonly JsonSchema schema = new JsonSchema();

        public QueryJsonConversionTests()
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

            schema.Properties["json"] = new JsonSchemaProperty
            {
                Type = JsonObjectType.None
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

        [Fact]
        public void Should_add_error_if_nested_reference_property_does_not_exist()
        {
            var json = new { path = "reference.notfound", op = "eq", value = 1 };

            AssertErrors(json, "'notfound' is not a property of 'nested'.");
        }

        public static IEnumerable<object[]> DateTimeTests()
        {
            const string value = "2012-11-10T09:08:07Z";

            return BuildTests("datetime", x => true, value, value);
        }

        [Theory]
        [MemberData(nameof(DateTimeTests))]
        public void Should_parse_datetime_string_filter(string field, string op, string value, string expected)
        {
            var json = new { path = field, op, value };

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

        public static IEnumerable<object[]> GuidTests()
        {
            const string value = "bf57d32c-d4dd-4217-8c16-6dcb16975cf3";

            return BuildTests("guid", x => true, value, value);
        }

        [Theory]
        [MemberData(nameof(GuidTests))]
        public void Should_parse_guid_string_filter(string field, string op, string value, string expected)
        {
            var json = new { path = field, op, value };

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

        public static IEnumerable<object[]> StringTests()
        {
            const string value = "Hello";

            return BuildTests("string", x => true, value, $"'{value}'");
        }

        [Theory]
        [MemberData(nameof(StringTests))]
        public void Should_parse_string_filter(string field, string op, string value, string expected)
        {
            var json = new { path = field, op, value };

            AssertFilter(json, expected);
        }

        public static IEnumerable<object[]> StringInTests()
        {
            const string value = "Hello";

            return BuildInTests("string", value, $"'{value}'");
        }

        [Theory]
        [MemberData(nameof(StringInTests))]
        public void Should_parse_string_in_filter(string field, string value, string expected)
        {
            var json = new { path = field, op = "in", value = new[] { value } };

            AssertFilter(json, expected);
        }

        [Fact]
        public void Should_add_error_if_string_property_got_invalid_value()
        {
            var json = new { path = "string", op = "eq", value = 1 };

            AssertErrors(json, "Expected String for path 'string', but got Number.");
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

        public static IEnumerable<object[]> NumberTests()
        {
            const int value = 12;

            return BuildTests("number", x => x.Length == 2, value, $"{value}");
        }

        [Theory]
        [MemberData(nameof(NumberTests))]
        public void Should_parse_number_filter(string field, string op, int value, string expected)
        {
            var json = new { path = field, op, value };

            AssertFilter(json, expected);
        }

        public static IEnumerable<object[]> NumberInTests()
        {
            const int value = 12;

            return BuildInTests("number", value, $"{value}");
        }

        [Theory]
        [MemberData(nameof(NumberInTests))]
        public void Should_parse_number_in_filter(string field, int value, string expected)
        {
            var json = new { path = field, op = "in", value = new[] { value } };

            AssertFilter(json, expected);
        }

        [Fact]
        public void Should_add_error_if_number_property_got_invalid_value()
        {
            var json = new { path = "number", op = "eq", value = true };

            AssertErrors(json, "Expected Number for path 'number', but got Boolean.");
        }

        public static IEnumerable<object[]> BooleanTests()
        {
            const bool value = true;

            return BuildTests("boolean", x => x == "eq" || x == "ne", value, $"{value}");
        }

        [Theory]
        [MemberData(nameof(BooleanTests))]
        public void Should_parse_boolean_filter(string field, string op, bool value, string expected)
        {
            var json = new { path = field, op, value };

            AssertFilter(json, expected);
        }

        public static IEnumerable<object[]> BooleanInTests()
        {
            const bool value = true;

            return BuildInTests("boolean", value, $"{value}");
        }

        [Theory]
        [MemberData(nameof(BooleanInTests))]
        public void Should_parse_boolean_in_filter(string field, bool value, string expected)
        {
            var json = new { path = field, op = "in", value = new[] { value } };

            AssertFilter(json, expected);
        }

        [Fact]
        public void Should_add_error_if_boolean_property_got_invalid_value()
        {
            var json = new { path = "boolean", op = "eq", value = 1 };

            AssertErrors(json, "Expected Boolean for path 'boolean', but got Number.");
        }

        public static IEnumerable<object[]> ArrayTests()
        {
            const string value = "Hello";

            return BuildTests("stringArray", x => x == "eq" || x == "ne" || x == "empty", value, $"'{value}'");
        }

        [Theory]
        [MemberData(nameof(ArrayTests))]
        public void Should_parse_array_filter(string field, string op, string value, string expected)
        {
            var json = new { path = field, op, value };

            AssertFilter(json, expected);
        }

        public static IEnumerable<object[]> ArrayInTests()
        {
            const string value = "Hello";

            return BuildInTests("stringArray", value, $"'{value}'");
        }

        [Theory]
        [MemberData(nameof(ArrayInTests))]
        public void Should_parse_array_in_filter(string field, string value, string expected)
        {
            var json = new { path = field, op = "in", value = new[] { value } };

            AssertFilter(json, expected);
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
        public void Should_parse_query_with_top()
        {
            var json = new { skip = 10, top = 20, FullText = "Hello", Filter = new { path = "string", op = "eq", value = "Hello" } };

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

            var jsonFilter = schema.Parse(json, JsonHelper.DefaultSerializer);

            return jsonFilter.ToString();
        }

        public static IEnumerable<object[]> BuildInTests(string field, object value, string valueString)
        {
            var fields = new[]
            {
                $"{field}",
                $"json.{field}",
                $"json.nested.{field}"
            };

            foreach (var f in fields)
            {
                var expected = $"{f} in [{valueString}]";

                yield return new[] { f, value, expected };
            }
        }

        public static IEnumerable<object[]> BuildTests(string field, Predicate<string> opFilter, object value, string valueString)
        {
            var fields = new[]
            {
                $"{field}",
                $"json.{field}",
                $"json.nested.{field}"
            };

            foreach (var f in fields)
            {
                foreach (var op in AllOps.Where(x => opFilter(x.Operator)))
                {
                    var expected =
                        op.Output
                            .Replace("$FIELD", f)
                            .Replace("$VALUE", valueString);

                    yield return new[] { f, op.Operator, value, expected };
                }
            }
        }
    }
}
