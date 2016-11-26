// ==========================================================================
//  TypeNameRegistryTests.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System;
using System.Reflection;
using Xunit;

namespace Squidex.Infrastructure
{
    public class TypeNameRegistryTests
    {
        [TypeName("my")]
        public sealed class MyType
        {
        }

        [Fact]
        public void Should_register_and_retrieve_types()
        {
            TypeNameRegistry.Map(typeof(int), "NumberField");

            Assert.Equal("NumberField", TypeNameRegistry.GetName<int>());
            Assert.Equal("NumberField", TypeNameRegistry.GetName(typeof(int)));

            Assert.Equal(typeof(int), TypeNameRegistry.GetType("NumberField"));
            Assert.Equal(typeof(int), TypeNameRegistry.GetType("NumberField"));
        }

        [Fact]
        public void Should_register_from_assembly()
        {
            TypeNameRegistry.Map(typeof(TypeNameRegistryTests).GetTypeInfo().Assembly);

            Assert.Equal("my", TypeNameRegistry.GetName<MyType>());
            Assert.Equal("my", TypeNameRegistry.GetName(typeof(MyType)));

            Assert.Equal(typeof(MyType), TypeNameRegistry.GetType("my"));
            Assert.Equal(typeof(MyType), TypeNameRegistry.GetType("My"));
        }

        [Fact]
        public void Should_not_throw_if_type_is_already_registered_with_same_name()
        {
            TypeNameRegistry.Map(typeof(long), "long");
            TypeNameRegistry.Map(typeof(long), "long");
        }

        [Fact]
        public void Should_throw_if_type_is_already_registered()
        {
            TypeNameRegistry.Map(typeof(long), "long");

            Assert.Throws<ArgumentException>(() => TypeNameRegistry.Map(typeof(long), "longer"));
        }

        [Fact]
        public void Should_throw_if_name_is_already_registered()
        {
            TypeNameRegistry.Map(typeof(short), "short");

            Assert.Throws<ArgumentException>(() => TypeNameRegistry.Map(typeof(byte), "short"));
        }

        [Fact]
        public void Should_throw_if_name_is_not_supported()
        {
            Assert.Throws<ArgumentException>(() => TypeNameRegistry.GetType("unsupported"));
        }

        [Fact]
        public void Should_throw_if_type_is_not_supported()
        {
            Assert.Throws<ArgumentException>(() => TypeNameRegistry.GetName<Guid>());
        }
    }
}
