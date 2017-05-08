// ==========================================================================
//  TypeNameRegistryTests.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System;
using System.Collections.Generic;
using System.Reflection;
using Xunit;

namespace Squidex.Infrastructure
{
    public class TypeNameRegistryTests
    {
        private readonly TypeNameRegistry sut = new TypeNameRegistry();

        [TypeName("my")]
        public sealed class MyType
        {
        }

        [Fact]
        public void Should_register_and_retrieve_types()
        {
            sut.Map(typeof(int), "NumberField");

            Assert.Equal("NumberField", sut.GetName<int>());
            Assert.Equal("NumberField", sut.GetName(typeof(int)));

            Assert.Equal(typeof(int), sut.GetType("NumberField"));
            Assert.Equal(typeof(int), sut.GetType("NumberField"));
        }

        [Fact]
        public void Should_register_from_attribute()
        {
            sut.Map(typeof(MyType));

            Assert.Equal("my", sut.GetName<MyType>());
            Assert.Equal("my", sut.GetName(typeof(MyType)));

            Assert.Equal(typeof(MyType), sut.GetType("my"));
            Assert.Equal(typeof(MyType), sut.GetType("My"));
        }

        [Fact]
        public void Should_register_from_assembly()
        {
            sut.Map(typeof(TypeNameRegistryTests).GetTypeInfo().Assembly);

            Assert.Equal("my", sut.GetName<MyType>());
            Assert.Equal("my", sut.GetName(typeof(MyType)));

            Assert.Equal(typeof(MyType), sut.GetType("my"));
            Assert.Equal(typeof(MyType), sut.GetType("My"));
        }

        [Fact]
        public void Should_not_throw_if_type_is_already_registered_with_same_name()
        {
            sut.Map(typeof(long), "long");
            sut.Map(typeof(long), "long");
        }

        [Fact]
        public void Should_throw_if_type_is_already_registered()
        {
            sut.Map(typeof(long), "long");

            Assert.Throws<ArgumentException>(() => sut.Map(typeof(long), "longer"));
        }

        [Fact]
        public void Should_throw_if_name_is_already_registered()
        {
            sut.Map(typeof(short), "short");

            Assert.Throws<ArgumentException>(() => sut.Map(typeof(byte), "short"));
        }

        [Fact]
        public void Should_throw_if_name_is_not_supported()
        {
            Assert.Throws<TypeNameNotFoundException>(() => sut.GetType("unsupported"));
        }

        [Fact]
        public void Should_throw_if_type_is_not_supported()
        {
            Assert.Throws<TypeNameNotFoundException>(() => sut.GetName<Guid>());
        }
    }
}
