// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using NodaTime;
using Xunit;

#pragma warning disable xUnit2004 // Do not use equality check to test for boolean conditions

namespace Squidex.Infrastructure.Json.Objects
{
    public class JsonObjectTests
    {
        [Fact]
        public void Should_make_correct_object_equal_comparisons()
        {
            var obj1a = new JsonObject().Add("key1", 1);
            var obj1b = new JsonObject().Add("key1", 1);

            var objOtherValue = new JsonObject().Add("key1", 2);
            var objOtherKey = new JsonObject().Add("key2", 1);

            var objOtherCount = new JsonObject().Add("key1", 1).Add("key2", 2);

            var number = JsonValue.Create(1);

            Assert.Equal(obj1a, obj1b);
            Assert.Equal(obj1a.GetHashCode(), obj1b.GetHashCode());
            Assert.True(obj1a.Equals((object)obj1b));

            Assert.NotEqual(obj1a, objOtherValue);
            Assert.NotEqual(obj1a.GetHashCode(), objOtherValue.GetHashCode());
            Assert.False(obj1a.Equals((object)objOtherValue));

            Assert.NotEqual(obj1a, objOtherKey);
            Assert.NotEqual(obj1a.GetHashCode(), objOtherKey.GetHashCode());
            Assert.False(obj1a.Equals((object)objOtherKey));

            Assert.NotEqual(obj1a, objOtherCount);
            Assert.NotEqual(obj1a.GetHashCode(), objOtherCount.GetHashCode());
            Assert.False(obj1a.Equals((object)objOtherCount));

            Assert.NotEqual(obj1a, number);
            Assert.NotEqual(obj1a.GetHashCode(), number.GetHashCode());
            Assert.False(obj1a.Equals((object)number));
        }

        [Fact]
        public void Should_make_correct_array_equal_comparisons()
        {
            var array1a = JsonValue.Array(1);
            var array1b = JsonValue.Array(1);

            var arrayOtherValue = JsonValue.Array(2);
            var arrayOtherSize = JsonValue.Array(1, 2);

            var number = JsonValue.Create(1);

            Assert.Equal(array1a, array1b);
            Assert.Equal(array1a.GetHashCode(), array1b.GetHashCode());
            Assert.True(array1a.Equals((object)array1b));

            Assert.NotEqual(array1a, arrayOtherValue);
            Assert.NotEqual(array1a.GetHashCode(), arrayOtherValue.GetHashCode());
            Assert.False(array1a.Equals((object)arrayOtherValue));

            Assert.NotEqual(array1a, arrayOtherSize);
            Assert.NotEqual(array1a.GetHashCode(), arrayOtherSize.GetHashCode());
            Assert.False(array1a.Equals((object)arrayOtherSize));

            Assert.NotEqual(array1a, number);
            Assert.NotEqual(array1a.GetHashCode(), number.GetHashCode());
            Assert.False(array1a.Equals((object)number));
        }

        [Fact]
        public void Should_make_correct_array_scalar_comparisons()
        {
            var number_val1_a = JsonValue.Create(1);
            var number_val1_b = JsonValue.Create(1);

            var number_val2 = JsonValue.Create(2);

            var boolean = JsonValue.True;

            Assert.Equal(number_val1_a, number_val1_b);
            Assert.Equal(number_val1_a.GetHashCode(), number_val1_b.GetHashCode());
            Assert.True(number_val1_a.Equals((object)number_val1_b));

            Assert.NotEqual(number_val1_a, number_val2);
            Assert.NotEqual(number_val1_a.GetHashCode(), number_val2.GetHashCode());
            Assert.False(number_val1_a.Equals((object)number_val2));

            Assert.NotEqual(number_val1_a, boolean);
            Assert.NotEqual(number_val1_a.GetHashCode(), boolean.GetHashCode());
            Assert.False(number_val1_a.Equals((object)boolean));
        }

        [Fact]
        public void Should_make_correct_null_comparisons()
        {
            var null_a = JsonValue.Null;
            var null_b = JsonValue.Null;

            var boolean = JsonValue.True;

            Assert.Equal(null_a, null_b);
            Assert.Equal(null_a.GetHashCode(), null_b.GetHashCode());
            Assert.True(null_a.Equals((object)null_b));

            Assert.NotEqual(null_a, boolean);
            Assert.NotEqual(null_a.GetHashCode(), boolean.GetHashCode());
            Assert.False(null_a.Equals((object)boolean));
        }

        [Fact]
        public void Should_create_null()
        {
            var jsons = new[]
            {
                new JsonValue((string?)null),
                JsonValue.Create((string?)null),
                JsonValue.Create((object?)null),
                default
            };

            foreach (var json in jsons)
            {
                Assert.Null(json.RawValue);
                Assert.Equal(JsonValueType.Null, json.Type);

                Assert.Throws<InvalidOperationException>(() => json.AsBoolean);
                Assert.Throws<InvalidOperationException>(() => json.AsNumber);
                Assert.Throws<InvalidOperationException>(() => json.AsString);
                Assert.Throws<InvalidOperationException>(() => json.AsArray);
                Assert.Throws<InvalidOperationException>(() => json.AsObject);
            }
        }

        [Fact]
        public void Should_create_booleans()
        {
            var jsons = new[]
            {
                new JsonValue(true),
                JsonValue.Create(true),
                JsonValue.Create((object?)true),
                true
            };

            foreach (var json in jsons)
            {
                Assert.Equal(true, json.RawValue);
                Assert.Equal(true, json.AsBoolean);
                Assert.Equal(JsonValueType.Boolean, json.Type);

                Assert.Throws<InvalidOperationException>(() => json.AsNumber);
                Assert.Throws<InvalidOperationException>(() => json.AsString);
                Assert.Throws<InvalidOperationException>(() => json.AsArray);
                Assert.Throws<InvalidOperationException>(() => json.AsObject);
            }
        }

        [Fact]
        public void Should_create_floats()
        {
            var jsons = new[]
            {
                new JsonValue(12.5),
                JsonValue.Create(12.5),
                JsonValue.Create((object?)12.5),
                JsonValue.Create((object?)12.5f),
                12.5
            };

            foreach (var json in jsons)
            {
                Assert.Equal(12.5, json.RawValue);
                Assert.Equal(12.5, json.AsNumber);
                Assert.Equal(JsonValueType.Number, json.Type);

                Assert.Throws<InvalidOperationException>(() => json.AsBoolean);
                Assert.Throws<InvalidOperationException>(() => json.AsString);
                Assert.Throws<InvalidOperationException>(() => json.AsArray);
                Assert.Throws<InvalidOperationException>(() => json.AsObject);
            }
        }

        [Fact]
        public void Should_create_more_integers()
        {
            var jsons = new[]
            {
                new JsonValue(12),
                JsonValue.Create(12),
                JsonValue.Create((object?)12L),
                JsonValue.Create((object?)12),
                12
            };

            foreach (var json in jsons)
            {
                Assert.Equal(12d, json.RawValue);
                Assert.Equal(12d, json.AsNumber);
                Assert.Equal(JsonValueType.Number, json.Type);

                Assert.Throws<InvalidOperationException>(() => json.AsBoolean);
                Assert.Throws<InvalidOperationException>(() => json.AsString);
                Assert.Throws<InvalidOperationException>(() => json.AsArray);
                Assert.Throws<InvalidOperationException>(() => json.AsObject);
            }
        }

        [Fact]
        public void Should_create_strings()
        {
            var jsons = new[]
            {
                new JsonValue("text"),
                JsonValue.Create("text"),
                JsonValue.Create((object?)"text"),
                "text"
            };

            foreach (var json in jsons)
            {
                Assert.Equal("text", json.RawValue);
                Assert.Equal("text", json.AsString);
                Assert.Equal(JsonValueType.String, json.Type);

                Assert.Throws<InvalidOperationException>(() => json.AsBoolean);
                Assert.Throws<InvalidOperationException>(() => json.AsNumber);
                Assert.Throws<InvalidOperationException>(() => json.AsArray);
                Assert.Throws<InvalidOperationException>(() => json.AsObject);
            }
        }

        [Fact]
        public void Should_create_instants()
        {
            var instant = Instant.FromUnixTimeSeconds(4123125455);

            var jsons = new[]
            {
                JsonValue.Create(instant),
                JsonValue.Create((object?)instant),
                instant
            };

            foreach (var json in jsons)
            {
                Assert.Equal(instant.ToString(), json.RawValue);
                Assert.Equal(instant.ToString(), json.AsString);
                Assert.Equal(JsonValueType.String, json.Type);

                Assert.Throws<InvalidOperationException>(() => json.AsBoolean);
                Assert.Throws<InvalidOperationException>(() => json.AsNumber);
                Assert.Throws<InvalidOperationException>(() => json.AsArray);
                Assert.Throws<InvalidOperationException>(() => json.AsObject);
            }
        }

        [Fact]
        public void Should_create_ids()
        {
            var id = DomainId.NewGuid();

            var jsons = new[]
            {
                JsonValue.Create(id),
                JsonValue.Create((object?)id),
                id
            };

            var result = id.ToString();

            foreach (var json in jsons)
            {
                Assert.Equal(result, json.RawValue);
                Assert.Equal(result, json.AsString);
                Assert.Equal(JsonValueType.String, json.Type);

                Assert.Throws<InvalidOperationException>(() => json.AsBoolean);
                Assert.Throws<InvalidOperationException>(() => json.AsNumber);
                Assert.Throws<InvalidOperationException>(() => json.AsArray);
                Assert.Throws<InvalidOperationException>(() => json.AsObject);
            }
        }

        [Fact]
        public void Should_create_arrays()
        {
            var input = new JsonArray { 1, 2 };

            var jsons = new[]
            {
                new JsonValue(input),
                JsonValue.Array(1, 2),
                JsonValue.Array(new int[] { 1, 2 }),
                JsonValue.Create(input),
                JsonValue.Create((object?)input),
                JsonValue.Create(new object[] { 1, 2 }),
                input
            };

            var result = new JsonArray { 1, 2 };

            foreach (var json in jsons)
            {
                Assert.Equal(result, json.RawValue);
                Assert.Equal(result, json.AsArray);
                Assert.Equal(JsonValueType.Array, json.Type);

                Assert.Throws<InvalidOperationException>(() => json.AsBoolean);
                Assert.Throws<InvalidOperationException>(() => json.AsNumber);
                Assert.Throws<InvalidOperationException>(() => json.AsString);
                Assert.Throws<InvalidOperationException>(() => json.AsObject);
            }
        }

        [Fact]
        public void Should_create_objects()
        {
            var input = new JsonObject().Add("1", 1).Add("2", 2);

            var jsons = new[]
            {
                new JsonValue(input),
                JsonValue.Create(input),
                JsonValue.Create((object?)input),
                JsonValue.Create(input.ToDictionary(x => x.Key, x => x.Value.RawValue)),
                input,
            };

            var result = new JsonObject().Add("1", 1).Add("2", 2);

            foreach (var json in jsons)
            {
                Assert.Equal(result, json.RawValue);
                Assert.Equal(result, json.AsObject);
                Assert.Equal(JsonValueType.Object, json.Type);

                Assert.Throws<InvalidOperationException>(() => json.AsBoolean);
                Assert.Throws<InvalidOperationException>(() => json.AsNumber);
                Assert.Throws<InvalidOperationException>(() => json.AsString);
                Assert.Throws<InvalidOperationException>(() => json.AsArray);
            }
        }

        [Fact]
        public void Should_clone_number_and_return_same()
        {
            var source = JsonValue.Create(1);

            var clone = source.Clone();

            Assert.Same(source.RawValue, clone.RawValue);
        }

        [Fact]
        public void Should_clone_string_and_return_same()
        {
            var source = JsonValue.Create("test");

            var clone = source.Clone();

            Assert.Same(source.RawValue, clone.RawValue);
        }

        [Fact]
        public void Should_clone_boolean_and_return_same()
        {
            var source = JsonValue.Create(true);

            var clone = source.Clone();

            Assert.Same(source.RawValue, clone.RawValue);
        }

        [Fact]
        public void Should_clone_null_and_return_same()
        {
            var source = JsonValue.Null;

            var clone = source.Clone();

            Assert.Same(source.RawValue, clone.RawValue);
        }

        [Fact]
        public void Should_clone_array_and_also_children()
        {
            var source = JsonValue.Array(new JsonArray(), new JsonArray()).AsArray;

            var clone = ((JsonValue)source).Clone().AsArray;

            Assert.NotSame(source, clone);

            for (var i = 0; i < source.Count; i++)
            {
                Assert.NotSame(clone[i].RawValue, source[i].RawValue);
            }
        }

        [Fact]
        public void Should_clone_object_and_also_children()
        {
            var source = new JsonObject().Add("1", new JsonArray()).Add("2", new JsonArray());

            var clone = ((JsonValue)source).Clone().AsObject;

            Assert.NotSame(source, clone);

            foreach (var (key, value) in clone)
            {
                Assert.NotSame(value.RawValue, source[key].RawValue);
            }
        }

        [Fact]
        public void Should_throw_exception_if_creation_value_from_invalid_type()
        {
            Assert.Throws<ArgumentException>(() => JsonValue.Create(default(TimeSpan)));
        }

        [Fact]
        public void Should_return_null_if_getting_value_by_path_segment_from_null()
        {
            var json = JsonValue.Null;

            var found = json.TryGetByPath("path", out var result);

            Assert.False(found);
            Assert.Equal(default, result);
        }

        [Fact]
        public void Should_return_null_if_getting_value_by_path_segment_from_string()
        {
            var json = JsonValue.Create("string");

            var found = json.TryGetByPath("path", out var result);

            Assert.False(found);
            Assert.Equal(default, result);
        }

        [Fact]
        public void Should_return_null_if_getting_value_by_path_segment_from_boolean()
        {
            var json = JsonValue.True;

            var found = json.TryGetByPath("path", out var result);

            Assert.False(found);
            Assert.Equal(default, result);
        }

        [Fact]
        public void Should_return_null_if_getting_value_by_path_segment_from_number()
        {
            var json = JsonValue.Create(12);

            var found = json.TryGetByPath("path", out var result);

            Assert.False(found);
            Assert.Equal(default, result);
        }

        [Fact]
        public void Should_return_same_object_if_path_is_null()
        {
            JsonValue json = new JsonObject().Add("property", 12);

            var found = json.TryGetByPath((string?)null, out var result);

            Assert.False(found);
            Assert.Equal(json, result);
        }

        [Fact]
        public void Should_return_same_object_if_path_is_empty()
        {
            JsonValue json = new JsonObject().Add("property", 12);

            var found = json.TryGetByPath(string.Empty, out var result);

            Assert.False(found);
            Assert.Equal(json, result);
        }

        [Fact]
        public void Should_return_from_nested_array()
        {
            JsonValue json =
                new JsonObject()
                    .Add("property",
                        JsonValue.Array(
                            JsonValue.Create(12),
                            new JsonObject()
                                .Add("nested", 13)));

            var found = json.TryGetByPath("property[1].nested", out var result);

            Assert.True(found);
            Assert.Equal(JsonValue.Create(13), result);
        }

        [Fact]
        public void Should_return_null_if_property_not_found()
        {
            JsonValue json =
                new JsonObject()
                    .Add("property", 12);

            var found = json.TryGetByPath("notfound", out var result);

            Assert.False(found);
            Assert.Equal(default, result);
        }

        [Fact]
        public void Should_return_null_if_out_of_index1()
        {
            JsonValue json = JsonValue.Array(12, 14);

            var found = json.TryGetByPath("-1", out var result);

            Assert.False(found);
            Assert.Equal(default, result);
        }

        [Fact]
        public void Should_return_null_if_out_of_index2()
        {
            JsonValue json = JsonValue.Array(12, 14);

            var found = json.TryGetByPath("2", out var result);

            Assert.False(found);
            Assert.Equal(default, result);
        }
    }
}
