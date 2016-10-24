// ==========================================================================
//  TypeNameRegistryTests.cs
//  PinkParrot Headless CMS
// ==========================================================================
//  Copyright (c) PinkParrot Group
//  All rights reserved.
// ==========================================================================

using System;
using System.Reflection;
using Xunit;

namespace PinkParrot.Infrastructure
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
            TypeNameRegistry.Map(typeof(int), "number");

            Assert.Equal("number", TypeNameRegistry.GetName<int>());
            Assert.Equal("number", TypeNameRegistry.GetName(typeof(int)));

            Assert.Equal(typeof(int), TypeNameRegistry.GetType("number"));
            Assert.Equal(typeof(int), TypeNameRegistry.GetType("Number"));
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
