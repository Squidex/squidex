// ==========================================================================
//  SimpleCopierTests.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System;
using Xunit;

namespace Squidex.Infrastructure.Reflection
{
    public class SimpleCopierTests
    {
        public class Cloneable : ICloneable
        {
            public int Value { get; }

            public Cloneable(int value)
            {
                Value = value;
            }

            public object Clone()
            {
                return new Cloneable(Value);
            }
        }

        public class MyClass1Base
        {
            public int Value1 { get; set; }
        }

        public class MyClass1 : MyClass1Base
        {
            public int Value2 { get; set; }

            public Cloneable Cloneable { get; set; }
        }

        [Fact]
        public void Should_copy_class()
        {
            var value = new MyClass1
            {
                Value1 = 1,
                Value2 = 2,
                Cloneable = new Cloneable(4)
            };

            var copy = value.Copy();

            Assert.Equal(value.Value1, copy.Value1);
            Assert.Equal(value.Value2, copy.Value2);

            Assert.Equal(value.Cloneable.Value, copy.Cloneable.Value);
            Assert.NotSame(value.Cloneable, copy.Cloneable);
        }
    }
}
