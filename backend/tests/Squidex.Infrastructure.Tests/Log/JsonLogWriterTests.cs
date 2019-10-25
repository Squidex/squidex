// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using NodaTime;
using Xunit;

namespace Squidex.Infrastructure.Log
{
    public class JsonLogWriterTests
    {
        private readonly IObjectWriter sut = JsonLogWriterFactory.Default().Create();

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
        public void Should_write_duration_property()
        {
            var result = sut.WriteProperty("property", new TimeSpan(2, 16, 30, 20, 100)).ToString();

            Assert.Equal(@"{""property"":""2.16:30:20.1000000""}", result);
        }

        [Fact]
        public void Should_write_date_property()
        {
            var value = Instant.FromUtc(2012, 11, 10, 9, 8, 45);

            var result = sut.WriteProperty("property", value).ToString();

            Assert.Equal(@"{""property"":""2012-11-10T09:08:45Z""}", result);
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
        public void Should_write_duration_value()
        {
            var result = sut.WriteArray("property", a => a.WriteValue(new TimeSpan(2, 16, 30, 20, 100))).ToString();

            Assert.Equal(@"{""property"":[""2.16:30:20.1000000""]}", result);
        }

        [Fact]
        public void Should_write_object_in_array()
        {
            var result = sut.WriteArray("property1", a => a.WriteObject(b => b.WriteProperty("property2", 120))).ToString();

            Assert.Equal(@"{""property1"":[{""property2"":120}]}", result);
        }

        [Fact]
        public void Should_write_object_in_array_with_context()
        {
            var result = sut.WriteArray("property1", a => a.WriteObject(13, (ctx, b) => b.WriteProperty("property2", 13))).ToString();

            Assert.Equal(@"{""property1"":[{""property2"":13}]}", result);
        }

        [Fact]
        public void Should_write_array_value_with_context()
        {
            var result = sut.WriteArray("property", 13, (ctx, a) => a.WriteValue(ctx)).ToString();

            Assert.Equal(@"{""property"":[13]}", result);
        }

        [Fact]
        public void Should_write_date_value()
        {
            var value = Instant.FromUtc(2012, 11, 10, 9, 8, 45);

            var result = sut.WriteArray("property", a => a.WriteValue(value)).ToString();

            Assert.Equal(@"{""property"":[""2012-11-10T09:08:45Z""]}", result);
        }

        [Fact]
        public void Should_write_nested_object()
        {
            var result = sut.WriteObject("property", o => o.WriteProperty("nested", "my-string")).ToString();

            Assert.Equal(@"{""property"":{""nested"":""my-string""}}", result);
        }

        [Fact]
        public void Should_write_nested_object_with_context()
        {
            var result = sut.WriteObject("property", 13, (ctx, o) => o.WriteProperty("nested", ctx)).ToString();

            Assert.Equal(@"{""property"":{""nested"":13}}", result);
        }

        [Fact]
        public void Should_write_pretty_json()
        {
            var prettySut = new JsonLogWriterFactory(true).Create();

            var result = prettySut.WriteProperty("property", 1.5).ToString();

            Assert.Equal(@"{NL  ""property"": 1.5NL}".Replace("NL", Environment.NewLine), result);
        }

        [Fact]
        public void Should_write_extra_line_after_object()
        {
            var prettySut = new JsonLogWriterFactory(false, true).Create();

            var result = prettySut.WriteProperty("property", 1.5).ToString();

            Assert.Equal(@"{""property"":1.5}NL".Replace("NL", Environment.NewLine), result);
        }
    }
}
