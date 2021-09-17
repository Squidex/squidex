// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using NodaTime;
using Xunit;

namespace Squidex.Infrastructure.Json.Objects
{
    public class JsonObjectTests
    {
        [Fact]
        public void Should_make_correct_object_equal_comparisons()
        {
            var obj1a = JsonValue.Object().Add("key1", 1);
            var obj1b = JsonValue.Object().Add("key1", 1);

            var objOtherValue = JsonValue.Object().Add("key1", 2);
            var objOtherKey = JsonValue.Object().Add("key2", 1);

            var objOtherCount = JsonValue.Object().Add("key1", 1).Add("key2", 2);

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
        public void Should_cache_null()
        {
            Assert.Same(JsonValue.Null, JsonValue.Create((string?)null));
            Assert.Same(JsonValue.Null, JsonValue.Create((bool?)null));
            Assert.Same(JsonValue.Null, JsonValue.Create((double?)null));
            Assert.Same(JsonValue.Null, JsonValue.Create((object?)null));
            Assert.Same(JsonValue.Null, JsonValue.Create((Instant?)null));
        }

        [Fact]
        public void Should_cache_true()
        {
            Assert.Same(JsonValue.True, JsonValue.Create(true));
        }

        [Fact]
        public void Should_cache_false()
        {
            Assert.Same(JsonValue.False, JsonValue.Create(false));
        }

        [Fact]
        public void Should_cache_empty()
        {
            Assert.Same(JsonValue.Empty, JsonValue.Create(string.Empty));
        }

        [Fact]
        public void Should_cache_zero()
        {
            Assert.Same(JsonValue.Zero, JsonValue.Create(0));
        }

        [Fact]
        public void Should_create_boolean_from_object()
        {
            Assert.Equal(JsonValue.True, JsonValue.Create((object)true));
        }

        [Fact]
        public void Should_create_value_from_instant()
        {
            var instant = Instant.FromUnixTimeSeconds(4123125455);

            Assert.Equal(instant.ToString(), JsonValue.Create(instant).ToString());
        }

        [Fact]
        public void Should_create_value_from_instant_object()
        {
            var instant = Instant.FromUnixTimeSeconds(4123125455);

            Assert.Equal(instant.ToString(), JsonValue.Create((object)instant).ToString());
        }

        [Fact]
        public void Should_create_array()
        {
            var json = JsonValue.Array<object>(1, "2");

            Assert.Equal("[1, \"2\"]", json.ToJsonString());
            Assert.Equal("[1, \"2\"]", json.ToString());
        }

        [Fact]
        public void Should_create_array_from_object_source()
        {
            var json = JsonValue.Create(new object[] { 1, "2" });

            Assert.Equal("[1, \"2\"]", json.ToJsonString());
            Assert.Equal("[1, \"2\"]", json.ToString());
        }

        [Fact]
        public void Should_create_array_from_source()
        {
            var json = JsonValue.Array<object>(1, "2");

            var copy = new JsonArray(json);

            Assert.Equal("[1, \"2\"]", copy.ToJsonString());
            Assert.Equal("[1, \"2\"]", copy.ToString());

            copy.Clear();

            Assert.Empty(copy);
            Assert.NotEmpty(json);
        }

        [Fact]
        public void Should_create_object()
        {
            var json = JsonValue.Object().Add("key1", 1).Add("key2", "2");

            Assert.Equal("{\"key1\":1, \"key2\":\"2\"}", json.ToJsonString());
            Assert.Equal("{\"key1\":1, \"key2\":\"2\"}", json.ToString());
        }

        [Fact]
        public void Should_create_object_from_clr_source()
        {
            var json = JsonValue.Create(new Dictionary<string, object> { ["key1"] = 1, ["key2"] = "2" });

            Assert.Equal("{\"key1\":1, \"key2\":\"2\"}", json.ToJsonString());
            Assert.Equal("{\"key1\":1, \"key2\":\"2\"}", json.ToString());
        }

        [Fact]
        public void Should_create_number()
        {
            var json = JsonValue.Create(123);

            Assert.Equal("123", json.ToJsonString());
            Assert.Equal("123", json.ToString());
        }

        [Fact]
        public void Should_create_boolean_true()
        {
            var json = JsonValue.Create(true);

            Assert.Equal("true", json.ToJsonString());
            Assert.Equal("true", json.ToString());
        }

        [Fact]
        public void Should_create_boolean_false()
        {
            var json = JsonValue.Create(false);

            Assert.Equal("false", json.ToJsonString());
            Assert.Equal("false", json.ToString());
        }

        [Fact]
        public void Should_create_string()
        {
            var json = JsonValue.Create("hi");

            Assert.Equal("\"hi\"", json.ToJsonString());
            Assert.Equal("hi", json.ToString());
        }

        [Fact]
        public void Should_create_null()
        {
            var json = JsonValue.Create((object?)null);

            Assert.Equal("null", json.ToJsonString());
            Assert.Equal("null", json.ToString());
        }

        [Fact]
        public void Should_clone_number_and_return_same()
        {
            var source = JsonValue.Create(1);

            var clone = source.Clone();

            Assert.Same(source, clone);
        }

        [Fact]
        public void Should_clone_string_and_return_same()
        {
            var source = JsonValue.Create("test");

            var clone = source.Clone();

            Assert.Same(source, clone);
        }

        [Fact]
        public void Should_clone_boolean_and_return_same()
        {
            var source = JsonValue.Create(true);

            var clone = source.Clone();

            Assert.Same(source, clone);
        }

        [Fact]
        public void Should_clone_null_and_return_same()
        {
            var source = JsonValue.Null;

            var clone = source.Clone();

            Assert.Same(source, clone);
        }

        [Fact]
        public void Should_clone_array_and_also_children()
        {
            var source = JsonValue.Array(JsonValue.Array(), JsonValue.Array());

            var clone = (JsonArray)source.Clone();

            Assert.NotSame(source, clone);

            for (var i = 0; i < source.Count; i++)
            {
                Assert.NotSame(clone[i], source[i]);
            }
        }

        [Fact]
        public void Should_clone_object_and_also_children()
        {
            var source = JsonValue.Object().Add("1", JsonValue.Array()).Add("2", JsonValue.Array());

            var clone = (JsonObject)source.Clone();

            Assert.NotSame(source, clone);

            foreach (var (key, value) in clone)
            {
                Assert.NotSame(value, source[key]);
            }
        }

        [Fact]
        public void Should_create_arrays_in_different_ways()
        {
            var numbers = new[]
            {
                JsonValue.Array(1.0f, 2.0f),
                JsonValue.Array(JsonValue.Create(1.0f), JsonValue.Create(2.0f))
            };

            Assert.Single(numbers.Distinct());
            Assert.Single(numbers.Select(x => x.GetHashCode()).Distinct());
        }

        [Fact]
        public void Should_create_number_from_types()
        {
            var numbers = new[]
            {
                JsonValue.Create(12.0f),
                JsonValue.Create(12.0),
                JsonValue.Create(12L),
                JsonValue.Create(12),
                JsonValue.Create((object)12.0d),
                JsonValue.Create((double?)12.0d)
            };

            Assert.Single(numbers.Distinct());
            Assert.Single(numbers.Select(x => x.GetHashCode()).Distinct());
        }

        [Fact]
        public void Should_create_null_if_adding_null_to_array()
        {
            var array = JsonValue.Array();

            array.Add(null!);

            Assert.Same(JsonValue.Null, array[0]);
        }

        [Fact]
        public void Should_create_null_if_replacing_to_null_in_array()
        {
            var array = JsonValue.Array(1);

            array[0] = null!;

            Assert.Same(JsonValue.Null, array[0]);
        }

        [Fact]
        public void Should_create_null_if_adding_null_to_object()
        {
            var obj = JsonValue.Object();

            obj.Add("key", null!);

            Assert.Same(JsonValue.Null, obj["key"]);
        }

        [Fact]
        public void Should_create_null_if_replacing_to_null_object()
        {
            var obj = JsonValue.Object();

            obj["key"] = null!;

            Assert.Same(JsonValue.Null, obj["key"]);
        }

        [Fact]
        public void Should_remove_value_from_object()
        {
            var obj = JsonValue.Object().Add("key", 1);

            obj.Remove("key");

            Assert.False(obj.TryGetValue("key", out _));
            Assert.False(obj.ContainsKey("key"));
        }

        [Fact]
        public void Should_clear_values_from_object()
        {
            var obj = JsonValue.Object().Add("key", 1);

            obj.Clear();

            Assert.False(obj.TryGetValue("key", out _));
            Assert.False(obj.ContainsKey("key"));
        }

        [Fact]
        public void Should_provide_collection_values_from_object()
        {
            var obj = JsonValue.Object().Add("11", "44").Add("22", "88");

            var kvps = new[]
            {
                new KeyValuePair<string, IJsonValue>("11", JsonValue.Create("44")),
                new KeyValuePair<string, IJsonValue>("22", JsonValue.Create("88"))
            };

            Assert.Equal(2, obj.Count);

            Assert.Equal(new[] { "11", "22" }, obj.Keys);
            Assert.Equal(new[] { "44", "88" }, obj.Values.Select(x => x.ToString()));

            Assert.Equal(kvps, obj.ToArray());
            Assert.Equal(kvps, ((IEnumerable)obj).OfType<KeyValuePair<string, IJsonValue>>().ToArray());
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

            var found = json.TryGet("path", out var result);

            Assert.False(found);
            Assert.Null(result);
        }

        [Fact]
        public void Should_return_null_if_getting_value_by_path_segment_from_string()
        {
            var json = JsonValue.Create("string");

            var found = json.TryGet("path", out var result);

            Assert.False(found);
            Assert.Null(result);
        }

        [Fact]
        public void Should_return_null_if_getting_value_by_path_segment_from_boolean()
        {
            var json = JsonValue.True;

            var found = json.TryGet("path", out var result);

            Assert.False(found);
            Assert.Null(result);
        }

        [Fact]
        public void Should_return_null_if_getting_value_by_path_segment_from_number()
        {
            var json = JsonValue.Create(12);

            var found = json.TryGet("path", out var result);

            Assert.False(found);
            Assert.Null(result);
        }

        [Fact]
        public void Should_return_same_object_if_path_is_null()
        {
            var json = JsonValue.Object().Add("property", 12);

            var found = json.TryGetByPath((string?)null, out var result);

            Assert.False(found);
            Assert.Same(json, result);
        }

        [Fact]
        public void Should_return_same_object_if_path_is_empty()
        {
            var json = JsonValue.Object().Add("property", 12);

            var found = json.TryGetByPath(string.Empty, out var result);

            Assert.False(found);
            Assert.Same(json, result);
        }

        [Fact]
        public void Should_return_from_nested_array()
        {
            var json =
                JsonValue.Object()
                    .Add("property",
                        JsonValue.Array(
                            JsonValue.Create(12),
                            JsonValue.Object()
                                .Add("nested", 13)));

            var found = json.TryGetByPath("property[1].nested", out var result);

            Assert.True(found);
            Assert.Equal(JsonValue.Create(13), result);
        }

        [Fact]
        public void Should_return_null_if_property_not_found()
        {
            var json =
                JsonValue.Object()
                    .Add("property", 12);

            var found = json.TryGetByPath("notfound", out var result);

            Assert.False(found);
            Assert.Null(result);
        }

        [Fact]
        public void Should_return_null_if_out_of_index1()
        {
            var json = JsonValue.Array(12, 14);

            var found = json.TryGetByPath("-1", out var result);

            Assert.False(found);
            Assert.Null(result);
        }

        [Fact]
        public void Should_return_null_if_out_of_index2()
        {
            var json = JsonValue.Array(12, 14);

            var found = json.TryGetByPath("2", out var result);

            Assert.False(found);
            Assert.Null(result);
        }
    }
}
