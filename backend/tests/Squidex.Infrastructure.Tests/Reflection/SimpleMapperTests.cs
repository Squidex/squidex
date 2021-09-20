// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Diagnostics;
using Xunit;

namespace Squidex.Infrastructure.Reflection
{
    public class SimpleMapperTests
    {
        public class Class1Base<T1>
        {
            public T1 P1 { get; set; }
        }

        public class Class1<T1, T2> : Class1Base<T1>
        {
            public T2 P2 { get; set; }
        }

        public class Class2Base<T2>
        {
            public T2 P2 { get; set; }
        }

        public class Class2<T2, T3> : Class2Base<T2>
        {
            public T3 P3 { get; set; }
        }

        public class Readonly<T>
        {
            public T P1 { get; }
        }

        public class Writeonly<T>
        {
#pragma warning disable MA0041 // Make property static
            public T P1
#pragma warning restore MA0041 // Make property static
            {
                set => Debug.WriteLine(value);
            }
        }

        [Fact]
        public void Should_throw_exception_if_mapping_with_null_source()
        {
            Assert.Throws<ArgumentNullException>(() => SimpleMapper.Map((Class2<int, int>?)null!, new Class2<int, int>()));
        }

        [Fact]
        public void Should_throw_exception_if_mapping_with_null_target()
        {
            Assert.Throws<ArgumentNullException>(() => SimpleMapper.Map(new Class2<int, int>(), (Class2<int, int>?)null!));
        }

        [Fact]
        public void Should_map_to_same_type()
        {
            var obj1 = new Class1<int, int>
            {
                P1 = 6,
                P2 = 8
            };
            var obj2 = SimpleMapper.Map(obj1, new Class2<int, int>());

            Assert.Equal(8, obj2.P2);
            Assert.Equal(0, obj2.P3);
        }

        [Fact]
        public void Should_map_all_properties()
        {
            var obj1 = new Class1<int, int>
            {
                P1 = 6,
                P2 = 8
            };
            var obj2 = SimpleMapper.Map(obj1, new Class1<int, int>());

            Assert.Equal(6, obj2.P1);
            Assert.Equal(8, obj2.P2);
        }

        [Fact]
        public void Should_map_to_convertible_type()
        {
            var obj1 = new Class1<long, long>
            {
                P1 = 6,
                P2 = 8
            };
            var obj2 = SimpleMapper.Map(obj1, new Class2<int, int>());

            Assert.Equal(8, obj2.P2);
            Assert.Equal(0, obj2.P3);
        }

        [Fact]
        public void Should_map_nullables()
        {
            var obj1 = new Class1<bool?, bool?>
            {
                P1 = true,
                P2 = true
            };
            var obj2 = SimpleMapper.Map(obj1, new Class2<bool, bool>());

            Assert.True(obj2.P2);
            Assert.False(obj2.P3);
        }

        [Fact]
        public void Should_map_if_convertible_is_null()
        {
            var obj1 = new Class1<int?, int?>
            {
                P1 = null,
                P2 = null
            };
            var obj2 = SimpleMapper.Map(obj1, new Class1<int, int>());

            Assert.Equal(0, obj2.P1);
            Assert.Equal(0, obj2.P2);
        }

        [Fact]
        public void Should_convert_to_string()
        {
            var obj1 = new Class1<RefToken, RefToken>
            {
                P1 = RefToken.User("1"),
                P2 = RefToken.User("2")
            };
            var obj2 = SimpleMapper.Map(obj1, new Class2<string, string>());

            Assert.Equal("subject:2", obj2.P2);
            Assert.Null(obj2.P3);
        }

        [Fact]
        public void Should_return_default_if_conversion_failed()
        {
            var obj1 = new Class1<long, long>
            {
                P1 = long.MaxValue,
                P2 = long.MaxValue
            };
            var obj2 = SimpleMapper.Map(obj1, new Class2<int, int>());

            Assert.Equal(0, obj2.P2);
            Assert.Equal(0, obj2.P3);
        }

        [Fact]
        public void Should_ignore_write_only()
        {
            var obj1 = new Writeonly<int>();
            var obj2 = SimpleMapper.Map(obj1, new Class1<int, int>());

            Assert.Equal(0, obj2.P1);
        }

        [Fact]
        public void Should_ignore_read_only()
        {
            var obj1 = new Class1<int, int> { P1 = 10 };
            var obj2 = SimpleMapper.Map(obj1, new Readonly<int>());

            Assert.Equal(0, obj2.P1);
        }
    }
}
