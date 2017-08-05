// ==========================================================================
//  PropertiesBagTests.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System;
using System.Globalization;
using System.Linq;
using Microsoft.CSharp.RuntimeBinder;
using Newtonsoft.Json;
using NodaTime;
using Squidex.Infrastructure.Json;
using Xunit;

// ReSharper disable PossibleInvalidOperationException
// ReSharper disable UnusedParameter.Local

namespace Squidex.Infrastructure
{
    public class PropertiesBagTest
    {
        private readonly CultureInfo c = CultureInfo.InvariantCulture;
        private readonly PropertiesBag bag = new PropertiesBag();
        private readonly dynamic dynamicBag;

        public PropertiesBagTest()
        {
            dynamicBag = bag;
        }

        [Fact]
        public void Should_serialize_and_deserialize_empty_bag()
        {
            var serializerSettings = new JsonSerializerSettings();

            serializerSettings.Converters.Add(new PropertiesBagConverter());

            var content = JsonConvert.SerializeObject(bag, serializerSettings);
            var output  = JsonConvert.DeserializeObject<PropertiesBag>(content, serializerSettings);

            Assert.Equal(bag.Count, output.Count);
        }

        [Fact]
        public void Should_serialize_and_deserialize()
        {
            var time = Instant.FromUnixTimeSeconds(SystemClock.Instance.GetCurrentInstant().ToUnixTimeSeconds());

            bag.Set("Key1", time);
            bag.Set("Key2", "MyString");
            bag.Set("Key3", 123L);
            bag.Set("Key4", true);
            bag.Set("Key5", Guid.NewGuid());

            var serializerSettings = new JsonSerializerSettings();

            serializerSettings.Converters.Add(new PropertiesBagConverter());

            var content = JsonConvert.SerializeObject(bag, serializerSettings);
            var response = JsonConvert.DeserializeObject<PropertiesBag>(content, serializerSettings);

            foreach (var kvp in response.Properties.Take(4))
            {
                Assert.Equal(kvp.Value.RawValue, bag[kvp.Key].RawValue);
            }

            Assert.Equal(bag["Key5"].ToGuid(c), response["Key5"].ToGuid(c));
        }

        [Fact]
        public void Should_return_false_when_renaming_unknown_property()
        {
            Assert.False(bag.Rename("OldKey", "NewKey"));
        }

        [Fact]
        public void Should_throw_when_renaming_to_existing_property()
        {
            bag.Set("NewKey", 1);

            Assert.Throws<ArgumentException>(() => bag.Rename("OldKey", "NewKey"));
        }

        [Fact]
        public void Should_throw_when_renaming_to_same_key()
        {
            Assert.Throws<ArgumentException>(() => bag.Rename("SameKey", "SameKey"));
        }

        [Fact]
        public void Should_provide_property_with_new_name_after_rename()
        {
            bag.Set("OldKey", 123);

            Assert.True(bag.Rename("OldKey", "NewKey"));
            Assert.True(bag.Contains("NewKey"));

            Assert.Equal(1, bag.Count);
            Assert.Equal(123, bag["NewKey"].ToInt32(c));

            Assert.False(bag.Contains("OldKey"));
        }

        [Fact]
        public void Should_calculate_count_correctly()
        {
            bag.Set("Key1", 1);
            bag.Set("Key2", 1);

            Assert.Equal(2, bag.Count);
        }

        [Fact]
        public void Should_calculate_keys_correctly()
        {
            bag.Set("Key1", 1);
            bag.Set("Key2", 1);

            Assert.Equal(new[] { "Key1", "Key2" }, bag.PropertyNames.ToArray());
            Assert.Equal(new[] { "Key1", "Key2" }, bag.Properties.Keys.ToArray());
            Assert.Equal(new[] { "Key1", "Key2" }, bag.GetDynamicMemberNames().ToArray());
        }

        [Fact]
        public void Should_return_correct_value_when_contains_check()
        {
            Assert.False(bag.Contains("Key"));

            bag.Set("Key", 1);

            Assert.True(bag.Contains("Key"));
            Assert.True(bag.Contains("KEY"));
        }

        [Fact]
        public void Should_returne_false_when_property_to_rename_does_not_exist()
        {
            Assert.False(bag.Remove("NOTFOUND"));
        }

        [Fact]
        public void Should_ignore_casing_when_returning()
        {
            bag.Set("Key", 1);

            Assert.True(bag.Remove("KEY"));
            Assert.False(bag.Contains("KEY"));
        }

        [Fact]
        public void Should_set_value_as_dynamic()
        {
            dynamicBag.Key = 456;

            Assert.Equal(456, (int)dynamicBag.Key);
        }

        [Fact]
        public void Should_throw_when_setting_value_with_invalid_type_dynamically()
        {
            Assert.Throws<InvalidOperationException>(() => dynamicBag.Key = (byte)123);
        }

        [Fact]
        public void Should_throw_when_setting_value_with_invalid_type()
        {
            Assert.Throws<InvalidOperationException>(() => bag.Set("Key", (byte)1));
        }

        [Fact]
        public void Should_return_false_when_making_contains_check()
        {
            Assert.False(dynamicBag.Contains("Key"));
        }

        [Fact]
        public void Should_provide_default_value_if_not_exists()
        {
            Assert.Equal(0, (int)dynamicBag.Key);
        }

        [Fact]
        public void Should_throw_when_parsing_failed()
        {
            bag.Set("Key", "abc");

            Assert.Throws<InvalidCastException>(() => bag["Key"].ToInt32(CultureInfo.InvariantCulture));
        }

        [Fact]
        public void Should_return_false_when_converter_does_not_exist()
        {
            bag.Set("Key", "abc");

            Assert.Throws<RuntimeBinderException>(() => (TimeSpan)dynamicBag.Key);
        }

        [Fact]
        public void Should_convert_string_to_numbers()
        {
            bag.Set("Key", 123);

            AssertNumber();
        }

        [Fact]
        public void Should_convert_int_to_numbers()
        {
            bag.Set("Key", 123);

            AssertNumber();
        }

        [Fact]
        public void Should_convert_long_to_numbers()
        {
            bag.Set("Key", 123L);

            AssertNumber();
        }

        [Fact]
        public void Should_throw_when_casting_from_large_long()
        {
            bag.Set("Key", long.MaxValue);

            Assert.Throws<InvalidCastException>(() => bag["Key"].ToInt32(c));
        }

        [Fact]
        public void Should_convert_float_to_number()
        {
            bag.Set("Key", 123f);

            AssertNumber();
        }

        [Fact]
        public void Should_convert_double_to_number()
        {
            bag.Set("Key", 123d);

            AssertNumber();
        }

        [Fact]
        public void Should_throw_when_casting_from_large_doule()
        {
            bag.Set("Key", double.MaxValue);

            Assert.Equal(float.PositiveInfinity, bag["Key"].ToSingle(c));
        }

        [Fact]
        public void Should_convert_from_instant_value()
        {
            var time = SystemClock.Instance.GetCurrentInstant();

            bag.Set("Key", time);

            AssertInstant(time);
        }

        [Fact]
        public void Should_convert_from_instant_string()
        {
            var time = Instant.FromUnixTimeSeconds(SystemClock.Instance.GetCurrentInstant().ToUnixTimeSeconds());

            bag.Set("Key", time.ToString());

            AssertInstant(time);
        }

        [Fact]
        public void Should_convert_from_guid_value()
        {
            var id = new Guid();

            bag.Set("Key", id);

            AssertGuid(id);
        }

        [Fact]
        public void Should_convert_from_guid_string()
        {
            var id = new Guid();

            bag.Set("Key", id.ToString());

            AssertGuid(id);
        }

        [Fact]
        public void Should_convert_from_boolean_value()
        {
            bag.Set("Key", true);

            AssertBoolean(true);
        }

        [Fact]
        public void Should_convert_from_boolean_string()
        {
            bag.Set("Key", "true");

            AssertBoolean(true);
        }

        [Fact]
        public void Should_convert_boolean_from_number()
        {
            bag.Set("Key", 1);

            AssertBoolean(true);
        }

        [Fact]
        public void Should_convert_boolean_to_truthy_number_string()
        {
            bag.Set("Key", "1");

            AssertBoolean(true);
        }

        [Fact]
        public void Should_convert_boolean_to_falsy_number_string()
        {
            bag.Set("Key", "0");

            AssertBoolean(false);
        }

        [Fact]
        public void Should_provide_value_as_string()
        {
            bag.Set("Key", "Foo");

            AssertString("Foo");
        }

        [Fact]
        public void Should_provide_null()
        {
            bag.Set("Key", null);

            AssertNull();
        }

        [Fact]
        public void Should_throw_when_converting_instant_to_number()
        {
            bag.Set("Key", SystemClock.Instance.GetCurrentInstant());

            Assert.Throws<InvalidCastException>(() => bag["Key"].ToGuid(CultureInfo.InvariantCulture));
        }

        private void AssertNumber()
        {
            AssertInt32(123);
            AssertInt64(123);
            AssertSingle(123);
            AssertDouble(123);
        }

        private void AssertString(string expected)
        {
            Assert.Equal(expected, bag["Key"].ToString());

            Assert.Equal(expected, (string)dynamicBag.Key);
        }

        private void AssertNull()
        {
            Assert.Null(bag["Key"].ToString());
            Assert.Null(bag["Key"].RawValue);
        }

        private void AssertBoolean(bool expected)
        {
            Assert.Equal(expected, bag["Key"].ToBoolean(c));
            Assert.Equal(expected, bag["Key"].ToNullableBoolean(c));

            Assert.Equal(expected, (bool)dynamicBag.Key);
            Assert.Equal(expected, (bool?)dynamicBag.Key);
        }

        private void AssertInstant(Instant expected)
        {
            Assert.Equal(expected, bag["Key"].ToInstant(c));
            Assert.Equal(expected, bag["Key"].ToNullableInstant(c).Value);

            Assert.Equal(expected, (Instant)dynamicBag.Key);
            Assert.Equal(expected, (Instant?)dynamicBag.Key);
        }

        private void AssertGuid(Guid expected)
        {
            Assert.Equal(expected, bag["Key"].ToGuid(c));
            Assert.Equal(expected, bag["Key"].ToNullableGuid(c));

            Assert.Equal(expected, (Guid)dynamicBag.Key);
            Assert.Equal(expected, (Guid?)dynamicBag.Key);
        }

        private void AssertDouble(double expected)
        {
            Assert.Equal(expected, bag["Key"].ToDouble(c));
            Assert.Equal(expected, bag["Key"].ToNullableDouble(c));

            Assert.Equal(expected, (double)dynamicBag.Key);
            Assert.Equal(expected, (double?)dynamicBag.Key);
        }

        private void AssertSingle(float expected)
        {
            Assert.Equal(expected, bag["Key"].ToSingle(c));
            Assert.Equal(expected, bag["Key"].ToNullableSingle(c));

            Assert.Equal(expected, (float)dynamicBag.Key);
            Assert.Equal(expected, (float?)dynamicBag.Key);
        }

        private void AssertInt64(long expected)
        {
            Assert.Equal(expected, bag["Key"].ToInt64(c));
            Assert.Equal(expected, bag["Key"].ToNullableInt64(c));

            Assert.Equal(expected, (long)dynamicBag.Key);
            Assert.Equal(expected, (long?)dynamicBag.Key);
        }

        private void AssertInt32(int expected)
        {
            Assert.Equal(expected, bag["Key"].ToInt32(c));
            Assert.Equal(expected, bag["Key"].ToNullableInt32(c));

            Assert.Equal(expected, (int)dynamicBag.Key);
            Assert.Equal(expected, (int?)dynamicBag.Key);
        }
    }
}