// ==========================================================================
//  JsonLogWriterTests.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System;
using Newtonsoft.Json;
using Xunit;

namespace Squidex.Infrastructure.Log
{
    public class JsonLogWriterTests
    {
        private readonly IObjectWriter sut = new JsonLogWriter();

        [Fact]
        public void Should_write_boolean_property()
        {
            var result = sut.WriteProperty("property", true).ToString();

            Assert.Equal(@"{""property"":true}", result);
        }

        [Fact]
        public void Should_write_long_property()
        {
            var result = sut.WriteProperty("property", 120).ToString();

            Assert.Equal(@"{""property"":120}", result);
        }

        [Fact]
        public void Should_write_double_property()
        {
            var result = sut.WriteProperty("property", 1.5).ToString();

            Assert.Equal(@"{""property"":1.5}", result);
        }

        [Fact]
        public void Should_write_string_property()
        {
            var result = sut.WriteProperty("property", "my-string").ToString();

            Assert.Equal(@"{""property"":""my-string""}", result);
        }

        [Fact]
        public void Should_write_timespan_property()
        {
            var result = sut.WriteProperty("property", new TimeSpan(1, 40, 30, 20, 100)).ToString();

            Assert.Equal(@"{""property"":""2.16:30:20.1000000""}", result);
        }

        [Fact]
        public void Should_write_datetimeoffset_property()
        {
            var value = DateTimeOffset.UtcNow;
            var result = sut.WriteProperty("property", value).ToString();

            Assert.Equal($"{{\"property\":\"{value:o}\"}}", result);
        }

        [Fact]
        public void Should_write_date_property()
        {
            var value = DateTime.UtcNow;
            var result = sut.WriteProperty("property", value).ToString();

            Assert.Equal($"{{\"property\":\"{value:o}\"}}", result);
        }

        [Fact]
        public void Should_write_boolean_value()
        {
            var result = sut.WriteArray("property", a => a.WriteValue(true)).ToString();

            Assert.Equal(@"{""property"":[true]}", result);
        }

        [Fact]
        public void Should_write_long_value()
        {
            var result = sut.WriteArray("property", a => a.WriteValue(120)).ToString();

            Assert.Equal(@"{""property"":[120]}", result);
        }

        [Fact]
        public void Should_write_double_value()
        {
            var result = sut.WriteArray("property", a => a.WriteValue(1.5)).ToString();

            Assert.Equal(@"{""property"":[1.5]}", result);
        }

        [Fact]
        public void Should_write_string_value()
        {
            var result = sut.WriteArray("property", a => a.WriteValue("my-string")).ToString();

            Assert.Equal(@"{""property"":[""my-string""]}", result);
        }

        [Fact]
        public void Should_write_timespan_value()
        {
            var result = sut.WriteArray("property", a => a.WriteValue(new TimeSpan(1, 40, 30, 20, 100))).ToString();

            Assert.Equal(@"{""property"":[""2.16:30:20.1000000""]}", result);
        }

        [Fact]
        public void Should_write_object_in_array()
        {
            var result = sut.WriteArray("property1", a => a.WriteObject(b => b.WriteProperty("property2", 120))).ToString();

            Assert.Equal(@"{""property1"":[{""property2"":120}]}", result);
        }

        [Fact]
        public void Should_write_datetimeoffset_value()
        {
            var value = DateTimeOffset.UtcNow;
            var result = sut.WriteArray("property", a => a.WriteValue(value)).ToString();

            Assert.Equal($"{{\"property\":[\"{value:o}\"]}}", result);
        }

        [Fact]
        public void Should_write_date_value()
        {
            var value = DateTime.UtcNow;
            var result = sut.WriteArray("property", a => a.WriteValue(value)).ToString();

            Assert.Equal($"{{\"property\":[\"{value:o}\"]}}", result);
        }

        [Fact]
        public void Should_write_nested_object()
        {
            var result = sut.WriteObject("property", a => a.WriteProperty("nested", "my-string")).ToString();

            Assert.Equal(@"{""property"":{""nested"":""my-string""}}", result);
        }

        [Fact]
        public void Should_write_pretty_json()
        {
            IObjectWriter prettySut = new JsonLogWriter(Formatting.Indented);

            var result = prettySut.WriteProperty("property", 1.5).ToString();

            Assert.Equal(@"{NL  ""property"": 1.5NL}".Replace("NL", Environment.NewLine), result);
        }

        [Fact]
        public void Should_write_extra_line_after_object()
        {
            IObjectWriter prettySut = new JsonLogWriter(Formatting.None, true);

            var result = prettySut.WriteProperty("property", 1.5).ToString();

            Assert.Equal(@"{""property"":1.5}NL".Replace("NL", Environment.NewLine), result);
        }
    }
}
