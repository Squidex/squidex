// ==========================================================================
//  PropertiesTypeAccessorTests.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System;
using System.Linq;
using Xunit;

namespace Squidex.Infrastructure.Reflection
{
    public class PropertiesTypeAccessorTests
    {
        public class TestClass
        {
            private int target;

            public int ReadWrite
            {
                get { return target; }
                set { target = value; }
            }

            public int Read
            {
                get { return target; }
            }

            public int Write
            {
                set { target = value; }
            }
        }

        private readonly TestClass target = new TestClass();
        private readonly PropertiesTypeAccessor accessor = PropertiesTypeAccessor.Create(typeof(TestClass));

        [Fact]
        public void Should_provide_properties()
        {
            var properties = accessor.Properties.Select(x => x.Name).ToArray();

            Assert.Equal(new[] { "ReadWrite", "Read", "Write" }, properties);
        }

        [Fact]
        public void Should_set_read_write_property()
        {
            accessor.SetValue(target, "ReadWrite", 123);

            Assert.Equal(123, target.Read);
        }

        [Fact]
        public void Should_set_write_property()
        {
            accessor.SetValue(target, "Write", 123);

            Assert.Equal(123, target.Read);
        }

        [Fact]
        public void Should_throw_exception_if_setting_unknown_property()
        {
            Assert.Throws<ArgumentException>(() => accessor.SetValue(target, "Unknown", 123));
        }

        [Fact]
        public void Should_throw_exception_if_setting_readonly()
        {
            Assert.Throws<NotSupportedException>(() => accessor.SetValue(target, "Read", 123));
        }

        [Fact]
        public void Should_get_read_write_property()
        {
            target.Write = 123;

            Assert.Equal(123, accessor.GetValue(target, "ReadWrite"));
        }

        [Fact]
        public void Should_get_read_property()
        {
            target.Write = 123;

            Assert.Equal(123, accessor.GetValue(target, "Read"));
        }

        [Fact]
        public void Should_throw_exception_if_getting_unknown_property()
        {
            Assert.Throws<ArgumentException>(() => accessor.GetValue(target, "Unknown"));
        }

        [Fact]
        public void Should_throw_exception_if_getting_readonly_property()
        {
            Assert.Throws<NotSupportedException>(() => accessor.GetValue(target, "Write"));
        }
    }
}
