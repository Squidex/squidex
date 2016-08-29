// ==========================================================================
//  PropertiesBagTests.cs
//  PinkParrot Headless CMS
// ==========================================================================
//  Copyright (c) PinkParrot Group
//  All rights reserved.
// ==========================================================================

using System;
using System.Globalization;
using System.Linq;
using NodaTime;
using Xunit;

// ReSharper disable PossibleInvalidOperationException
// ReSharper disable UnusedParameter.Local

namespace PinkParrot.Infrastructure
{
    public class PropertiesBagTest
    {
        private readonly CultureInfo c = CultureInfo.InvariantCulture;
        private readonly PropertiesBag bag = new PropertiesBag();

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
            Assert.Equal(new[] { "Key1", "Key2" }, bag.PropertyNames.ToArray());
            Assert.Equal(new[] { "Key1", "Key2" }, bag.Properties.Keys.ToArray());
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
        public void Should_throw_when_setting_value_with_invalid_type()
        {
            Assert.Throws<ArgumentException>(() => bag.Set("Key", (byte)1));
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
            var time = SystemClock.Instance.GetCurrentInstant();

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

            AssertBoolean();
        }

        [Fact]
        public void Should_convert_from_boolean_string()
        {
            bag.Set("Key", "true");

            AssertBoolean();
        }

        [Fact]
        public void Should_convert_boolean_from_number()
        {
            bag.Set("Key", 1);

            AssertBoolean();
        }

        [Fact]
        public void Should_throw_when_converting_instant_to_number()
        {
            bag.Set("Key", SystemClock.Instance.GetCurrentInstant());

            Assert.Throws<InvalidCastException>(() => bag["Key"].ToGuid(CultureInfo.InvariantCulture));
        }

        [Fact]
        public void Should_return_default_when_property_value_is_null()
        {
            bag.Set("Key", null);

            Assert.Equal(null, bag["Key"].ToString());

            Assert.Equal(0f, bag["Key"].ToSingle(CultureInfo.CurrentCulture));
            Assert.Equal(0d, bag["Key"].ToDouble(CultureInfo.CurrentCulture));
            Assert.Equal(0L, bag["Key"].ToInt64(CultureInfo.CurrentCulture));
            Assert.Equal(0,  bag["Key"].ToInt32(CultureInfo.CurrentCulture));

            Assert.Equal(false, bag["Key"].ToBoolean(CultureInfo.CurrentCulture));

            Assert.Equal(new Guid(), bag["Key"].ToGuid(CultureInfo.CurrentCulture));

            Assert.Equal(new Instant(), bag["Key"].ToInstant(CultureInfo.CurrentCulture));
        }

        private void AssertBoolean()
        {
            Assert.True(bag["Key"].ToBoolean(c));
            Assert.True(bag["Key"].ToNullableBoolean(c));
        }

        private void AssertInstant(Instant expected)
        {
            Assert.Equal(expected.ToUnixTimeSeconds(), bag["Key"].ToInstant(c).ToUnixTimeSeconds());
            Assert.Equal(expected.ToUnixTimeSeconds(), bag["Key"].ToNullableInstant(c).Value.ToUnixTimeSeconds());
        }

        private void AssertGuid(Guid expected)
        {
            Assert.Equal(expected, bag["Key"].ToGuid(c));
            Assert.Equal(expected, bag["Key"].ToNullableGuid(c));
        }

        private void AssertNumber()
        {
            Assert.Equal(123, bag["Key"].ToInt32(c));
            Assert.Equal(123, bag["Key"].ToNullableInt32(c));
            Assert.Equal(123L, bag["Key"].ToInt64(c));
            Assert.Equal(123L, bag["Key"].ToNullableInt64(c));
            Assert.Equal(123f, bag["Key"].ToSingle(c));
            Assert.Equal(123f, bag["Key"].ToNullableSingle(c));
            Assert.Equal(123d, bag["Key"].ToDouble(c));
            Assert.Equal(123d, bag["Key"].ToNullableDouble(c));

            Assert.True(bag["Key"].ToBoolean(c));
        }
    }
}
