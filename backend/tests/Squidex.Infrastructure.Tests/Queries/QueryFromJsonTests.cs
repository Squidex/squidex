// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Infrastructure.Collections;
using Squidex.Infrastructure.Json.Objects;
using Squidex.Infrastructure.Queries.Json;
using Squidex.Infrastructure.TestHelpers;
using Squidex.Infrastructure.Validation;
using Xunit;

namespace Squidex.Infrastructure.Queries
{
    public sealed class QueryFromJsonTests
    {
        private static readonly (string Name, string Operator, string Output)[] AllOps =
        {
            ("Contains", "contains", "contains($FIELD, $VALUE)"),
            ("Empty", "empty", "empty($FIELD)"),
            ("Exists", "exists", "exists($FIELD)"),
            ("EndsWith", "endswith", "endsWith($FIELD, $VALUE)"),
            ("Equals", "eq", "$FIELD == $VALUE"),
            ("GreaterThanOrEqual", "ge", "$FIELD >= $VALUE"),
            ("GreaterThan", "gt", "$FIELD > $VALUE"),
            ("LessThanOrEqual", "le", "$FIELD <= $VALUE"),
            ("LessThan", "lt", "$FIELD < $VALUE"),
            ("NotEquals", "ne", "$FIELD != $VALUE"),
            ("StartsWith", "startswith", "startsWith($FIELD, $VALUE)")
        };

        private static readonly QueryModel Model = new QueryModel();

        static QueryFromJsonTests()
        {
            var fields = new List<FilterableField>
            {
                new FilterableField(FilterableFieldType.Object, "object")
                {
                    Fields = ReadonlyList.Create(new FilterableField(FilterableFieldType.String, "property"))
                },
                new FilterableField(FilterableFieldType.Any, "json"),
                new FilterableField(FilterableFieldType.Boolean, "boolean"),
                new FilterableField(FilterableFieldType.DateTime, "datetime"),
                new FilterableField(FilterableFieldType.GeoObject, "geo"),
                new FilterableField(FilterableFieldType.Guid, "guid"),
                new FilterableField(FilterableFieldType.Number, "number"),
                new FilterableField(FilterableFieldType.String, "string"),
                new FilterableField(FilterableFieldType.StringArray, "stringArray"),
                new FilterableField(FilterableFieldType.String, "nested2.value")
            };

            Model = new QueryModel { Fields = fields };
        }

        public class DateTime
        {
            public static IEnumerable<object[]> ValidTests()
            {
                const string value = "2012-11-10T09:08:07Z";

                return BuildTests("datetime", x => true, value, value);
            }

            [Theory]
            [MemberData(nameof(ValidTests))]
            public void Should_parse_filter(string field, string op, string value, string expected)
            {
                var json = new { path = field, op, value };

                AssertFilter(json, expected);
            }

            [Fact]
            public void Should_add_error_if_value_is_invalid()
            {
                var json = new { path = "datetime", op = "eq", value = "invalid" };

                AssertErrors(json, "Expected ISO8601 DateTime String for path 'datetime', but got invalid String.");
            }

            [Fact]
            public void Should_add_error_if_value_type_is_invalid()
            {
                var json = new { path = "datetime", op = "eq", value = 1 };

                AssertErrors(json, "Expected ISO8601 DateTime String for path 'datetime', but got Number.");
            }
        }

        public class Guid
        {
            public static IEnumerable<object[]> ValidTests()
            {
                const string value = "bf57d32c-d4dd-4217-8c16-6dcb16975cf3";

                return BuildTests("guid", x => true, value, value);
            }

            [Theory]
            [MemberData(nameof(ValidTests))]
            public void Should_parse_filter(string field, string op, string value, string expected)
            {
                var json = new { path = field, op, value };

                AssertFilter(json, expected);
            }

            [Fact]
            public void Should_add_error_if_value_is_invalid()
            {
                var json = new { path = "guid", op = "eq", value = "invalid" };

                AssertErrors(json, "Expected Guid String for path 'guid', but got invalid String.");
            }

            [Fact]
            public void Should_add_error_if_value_type_is_invalid()
            {
                var json = new { path = "guid", op = "eq", value = 1 };

                AssertErrors(json, "Expected Guid String for path 'guid', but got Number.");
            }
        }

        public class String
        {
            public static IEnumerable<object[]> ValidTests()
            {
                const string value = "Hello";

                return BuildTests("string", x => true, value, $"'{value}'");
            }

            public static IEnumerable<object[]> ValidInTests()
            {
                const string value = "Hello";

                return BuildInTests("string", value, $"'{value}'");
            }

            [Theory]
            [MemberData(nameof(ValidTests))]
            public void Should_parse_filter(string field, string op, string value, string expected)
            {
                var json = new { path = field, op, value };

                AssertFilter(json, expected);
            }

            [Theory]
            [MemberData(nameof(ValidInTests))]
            public void Should_parse_in_filter(string field, string value, string expected)
            {
                var json = new { path = field, op = "in", value = new[] { value } };

                AssertFilter(json, expected);
            }

            [Fact]
            public void Should_add_error_if_value_type_is_invalid()
            {
                var json = new { path = "string", op = "eq", value = 1 };

                AssertErrors(json, "Expected String for path 'string', but got Number.");
            }

            [Fact]
            public void Should_add_error_if_valid_is_not_a_valid_regex()
            {
                var json = new { path = "string", op = "matchs", value = "((" };

                AssertErrors(json, "'((' is not a valid regular expression.");
            }

            [Fact]
            public void Should_parse_nested_string_filter()
            {
                var json = new { path = "object.property", op = "in", value = new[] { "Hello" } };

                AssertFilter(json, "object.property in ['Hello']");
            }
        }

        public class Geo
        {
            private static bool ValidOperator(string op)
            {
                return op == "lt" || op == "exists";
            }

            public static IEnumerable<object[]> ValidTests()
            {
                var value = new { longitude = 10, latitude = 20, distance = 30 };

                return BuildFlatTests("geo", ValidOperator, value, $"Radius({value.longitude}, {value.latitude}, {value.distance})");
            }

            public static IEnumerable<object[]> InvalidTests()
            {
                var value = new { longitude = 10, latitude = 20, distance = 30 };

                return BuildInvalidOperatorTests("geo", ValidOperator, value);
            }

            [Theory]
            [MemberData(nameof(ValidTests))]
            public void Should_parse_filter(string field, string op, object value, string expected)
            {
                var json = new { path = field, op, value };

                AssertFilter(json, expected);
            }

            [Theory]
            [MemberData(nameof(InvalidTests))]
            public void Should_add_error_if_operator_is_invalid(string field, string op, object value, string expected)
            {
                var json = new { path = field, op, value };

                AssertErrors(json, $"'{expected}' is not a valid operator for type GeoObject at '{field}'.");
            }

            [Fact]
            public void Should_add_error_if_value_is_invalid()
            {
                var json = new { path = "geo", op = "lt", value = new { latitude = 10, longitude = 20 } };

                AssertErrors(json, "Expected Object(geo-json) for path 'geo', but got Object.");
            }

            [Fact]
            public void Should_add_error_if_value_type_is_invalid()
            {
                var json = new { path = "geo", op = "lt", value = 1 };

                AssertErrors(json, "Expected Object(geo-json) for path 'geo', but got Number.");
            }
        }

        public class Number
        {
            private static bool ValidOperator(string op)
            {
                return op.Length == 2 || op == "exists";
            }

            public static IEnumerable<object[]> ValidTests()
            {
                const int value = 12;

                return BuildTests("number", ValidOperator, value, $"{value}");
            }

            public static IEnumerable<object[]> InvalidTests()
            {
                const int value = 12;

                return BuildInvalidOperatorTests("number", ValidOperator, $"{value}");
            }

            public static IEnumerable<object[]> ValidInTests()
            {
                const int value = 12;

                return BuildInTests("number", value, $"{value}");
            }

            [Theory]
            [MemberData(nameof(ValidTests))]
            public void Should_parse_filter(string field, string op, int value, string expected)
            {
                var json = new { path = field, op, value };

                AssertFilter(json, expected);
            }

            [Theory]
            [MemberData(nameof(InvalidTests))]
            public void Should_add_error_if_operator_is_invalid(string field, string op, int value, string expected)
            {
                var json = new { path = field, op, value };

                AssertErrors(json, $"'{expected}' is not a valid operator for type Number at '{field}'.");
            }

            [Theory]
            [MemberData(nameof(ValidInTests))]
            public void Should_parse_in_filter(string field, int value, string expected)
            {
                var json = new { path = field, op = "in", value = new[] { value } };

                AssertFilter(json, expected);
            }

            [Fact]
            public void Should_add_error_if_value_type_is_invalid()
            {
                var json = new { path = "number", op = "eq", value = true };

                AssertErrors(json, "Expected Number for path 'number', but got Boolean.");
            }
        }

        public class Boolean
        {
            private static bool ValidOperator(string op)
            {
                return op == "eq" || op == "ne" || op == "exists";
            }

            public static IEnumerable<object[]> ValidTests()
            {
                const bool value = true;

                return BuildTests("boolean", ValidOperator, value, $"{value}");
            }

            public static IEnumerable<object[]> InvalidTests()
            {
                const bool value = true;

                return BuildInvalidOperatorTests("boolean", ValidOperator, value);
            }

            public static IEnumerable<object[]> ValidInTests()
            {
                const bool value = true;

                return BuildInTests("boolean", value, $"{value}");
            }

            [Theory]
            [MemberData(nameof(ValidTests))]
            public void Should_parse_filter(string field, string op, bool value, string expected)
            {
                var json = new { path = field, op, value };

                AssertFilter(json, expected);
            }

            [Theory]
            [MemberData(nameof(InvalidTests))]
            public void Should_add_error_if_operator_is_invalid(string field, string op, bool value, string expected)
            {
                var json = new { path = field, op, value };

                AssertErrors(json, $"'{expected}' is not a valid operator for type Boolean at '{field}'.");
            }

            [Theory]
            [MemberData(nameof(ValidInTests))]
            public void Should_parse_in_filter(string field, bool value, string expected)
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
        }

        public class Array
        {
            private static bool ValidOperator(string op)
            {
                return op == "eq" || op == "ne" || op == "empty" || op == "exists";
            }

            public static IEnumerable<object[]> ValidTests()
            {
                const string value = "Hello";

                return BuildTests("stringArray", ValidOperator, value, $"'{value}'");
            }

            public static IEnumerable<object[]> ValidInTests()
            {
                const string value = "Hello";

                return BuildInTests("stringArray", value, $"'{value}'");
            }

            [Theory]
            [MemberData(nameof(ValidTests))]
            public void Should_parse_array_filter(string field, string op, string value, string expected)
            {
                var json = new { path = field, op, value };

                AssertFilter(json, expected);
            }

            [Theory]
            [MemberData(nameof(ValidInTests))]
            public void Should_parse_array_in_filter(string field, string value, string expected)
            {
                var json = new { path = field, op = "in", value = new[] { value } };

                AssertFilter(json, expected);
            }

            [Fact]
            public void Should_add_error_if_using_array_value_for_non_allowed_operator()
            {
                var json = new { path = "string", op = "eq", value = new[] { "Hello" } };

                AssertErrors(json, "Array value is not allowed for 'Equals' operator and path 'string'.");
            }

            [Fact]
            public void Should_convert_single_value_to_list_for_in_operator()
            {
                var json = new { path = "string", op = "in", value = "Hello" };

                AssertFilter(json, "string in ['Hello']");
            }
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

            AssertErrors(json, "'notfound' is not a property of 'object'.");
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
        public void Should_throw_exception_if_parsing_invalid_json()
        {
            var json = "invalid";

            Assert.Throws<ValidationException>(() => AssertQuery(json, null));
        }

        private static void AssertQuery(object json, string? expectedFilter)
        {
            var errors = new List<string>();

            var filter = ConvertQuery(json);

            Assert.Empty(errors);
            Assert.Equal(expectedFilter, filter);
        }

        private static void AssertFilter(object json, string? expectedFilter)
        {
            var errors = new List<string>();

            var filter = ConvertFilter(json, errors);

            Assert.Empty(errors);
            Assert.Equal(expectedFilter, filter);
        }

        private static void AssertErrors(object json, string expectedError)
        {
            var errors = new List<string>();

            var filter = ConvertFilter(json, errors);

            Assert.Equal(expectedError, errors.FirstOrDefault());
            Assert.Null(filter);
        }

        private static string? ConvertFilter<T>(T value, List<string> errors)
        {
            var json = TestUtils.DefaultSerializer.Serialize(value, true);

            var jsonFilter = TestUtils.DefaultSerializer.Deserialize<FilterNode<IJsonValue>>(json);

            return JsonFilterVisitor.Parse(jsonFilter, Model, errors)?.ToString();
        }

        private static string? ConvertQuery<T>(T value)
        {
            var json = TestUtils.DefaultSerializer.Serialize(value, true);

            var jsonFilter = Model.Parse(json, TestUtils.DefaultSerializer);

            return jsonFilter.ToString();
        }

        public static IEnumerable<object[]> BuildFlatTests(string field, Predicate<string> opFilter, object value, string valueString)
        {
            var fields = new[]
            {
                $"{field}"
            };

            foreach (var fieldName in fields)
            {
                foreach (var (_, op, output) in AllOps.Where(x => opFilter(x.Operator)))
                {
                    var expected =
                        output
                            .Replace("$FIELD", fieldName, StringComparison.Ordinal)
                            .Replace("$VALUE", valueString, StringComparison.Ordinal);

                    yield return new[] { fieldName, op, value, expected };
                }
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

            foreach (var fieldName in fields)
            {
                foreach (var (_, op, output) in AllOps.Where(x => opFilter(x.Operator)))
                {
                    var expected =
                        output
                            .Replace("$FIELD", fieldName, StringComparison.Ordinal)
                            .Replace("$VALUE", valueString, StringComparison.Ordinal);

                    yield return new[] { fieldName, op, value, expected };
                }
            }
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

        public static IEnumerable<object[]> BuildInvalidOperatorTests(string field, Predicate<string> opFilter, object value)
        {
            foreach (var (name, op, _) in AllOps.Where(x => !opFilter(x.Operator)))
            {
                yield return new[] { field, op, value, name };
            }
        }
    }
}
