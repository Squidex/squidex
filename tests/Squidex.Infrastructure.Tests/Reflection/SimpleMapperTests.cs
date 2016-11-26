// ==========================================================================
//  SimpleMapperTests.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System;
using Xunit;
// ReSharper disable UnusedParameter.Local

namespace Squidex.Infrastructure.Reflection
{
    public class SimpleMapperTests
    {
        public class Class1Base
        {
            public string MappedString { get; set; }

            public string MappedNull { get; set; }

            public long MappedNumber { get; set; }

            public Guid MappedGuid { get; set; }

            public long WrongType1 { get; set; }

            public long WrongType2 { get; set; }
        }

        public class Class1 : Class1Base
        {
            public string UnmappedString { get; set; }
        }

        public class Class2Base
        {
            public string MappedString { get; protected set; }

            public int MappedNull { get; set; }

            public int MappedNumber { get; set; }

            public string MappedGuid { get; set; }
        }

        public class Class2 : Class2Base
        {
            public string UnmappedString
            {
                get { return "Value"; }
            }

            public DateTime WrongType1 { get; set; }

            public TimeSpan WrongType2 { get; set; }
        }

        [Fact]
        public void Should_throw_if_mapping_with_null_source()
        {
            Assert.Throws<ArgumentNullException>(() => SimpleMapper.Map((Class1)null, new Class2()));
        }

        [Fact]
        public void Should_throw_if_mapping_with_null_target()
        {
            Assert.Throws<ArgumentNullException>(() => SimpleMapper.Map(new Class1(), (Class2)null));
        }

        [Fact]
        public void Should_to_type()
        {
            var class1 = new Class1
            {
                UnmappedString = Guid.NewGuid().ToString(),
                MappedString = Guid.NewGuid().ToString(),
                MappedNumber = 123,
                MappedGuid = Guid.NewGuid()
            };

            var class2 = SimpleMapper.Map<Class1, Class2>(class1);

            AssertObjectEquality(class1, class2);
        }

        [Fact]
        public void Should_map_between_types()
        {
            var class1 = new Class1
            {
                UnmappedString = Guid.NewGuid().ToString(),
                MappedString = Guid.NewGuid().ToString(),
                MappedNumber = 123,
                MappedGuid = Guid.NewGuid()
            };
            var class2 = new Class2();

            SimpleMapper.Map(class1, class2);

            AssertObjectEquality(class1, class2);
        }

        private static void AssertObjectEquality(Class1 class1, Class2 class2)
        {
            Assert.Equal(class1.MappedString, class2.MappedString);
            Assert.Equal(class1.MappedNumber, class2.MappedNumber);
            Assert.Equal(class1.MappedGuid.ToString(), class2.MappedGuid);
            Assert.Equal(class1.WrongType1, 0L);
            Assert.Equal(class1.WrongType2, 0L);
            Assert.NotEqual(class1.UnmappedString, class2.UnmappedString);
        }
    }
}
