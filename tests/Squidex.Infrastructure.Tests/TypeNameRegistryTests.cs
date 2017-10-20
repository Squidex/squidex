// ==========================================================================
//  TypeNameRegistryTests.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System;
using System.Reflection;
using Squidex.Infrastructure.CQRS.Events;
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

        [EventType(nameof(MyAdded), 2)]
        public sealed class MyAdded
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
        public void Should_register_event_type_from_assembly()
        {
            sut.Map(typeof(TypeNameRegistryTests).GetTypeInfo().Assembly);

            Assert.Equal("MyAddedEventV2", sut.GetName<MyAdded>());
            Assert.Equal("MyAddedEventV2", sut.GetName(typeof(MyAdded)));

            Assert.Equal(typeof(MyAdded), sut.GetType("myAddedEventV2"));
            Assert.Equal(typeof(MyAdded), sut.GetType("MyAddedEventV2"));
        }

        [Fact]
        public void Should_register_fallback_name()
        {
            sut.Map(typeof(MyType));
            sut.MapObsolete(typeof(MyType), "my-old");

            Assert.Equal(typeof(MyType), sut.GetType("my"));
            Assert.Equal(typeof(MyType), sut.GetType("my-old"));
        }

        [Fact]
        public void Should_not_throw_exception_if_type_is_already_registered_with_same_name()
        {
            sut.Map(typeof(long), "long");
            sut.Map(typeof(long), "long");
        }

        [Fact]
        public void Should_throw_exception_if_type_is_already_registered()
        {
            sut.Map(typeof(long), "long");

            Assert.Throws<ArgumentException>(() => sut.Map(typeof(long), "longer"));
        }

        [Fact]
        public void Should_throw_exception_if_name_is_already_registered()
        {
            sut.Map(typeof(short), "short");

            Assert.Throws<ArgumentException>(() => sut.Map(typeof(byte), "short"));
        }

        [Fact]
        public void Should_throw_exception_if_obsolete_name_is_already_registered()
        {
            sut.MapObsolete(typeof(short), "short2");

            Assert.Throws<ArgumentException>(() => sut.MapObsolete(typeof(byte), "short2"));
        }

        [Fact]
        public void Should_throw_exception_if_name_is_not_supported()
        {
            Assert.Throws<TypeNameNotFoundException>(() => sut.GetType("unsupported"));
        }

        [Fact]
        public void Should_return_null_if_name_is_not_supported()
        {
            Assert.Null(sut.GetTypeOrNull("unsupported"));
        }

        [Fact]
        public void Should_throw_exception_if_type_is_not_supported()
        {
            Assert.Throws<TypeNameNotFoundException>(() => sut.GetName<Guid>());
        }

        [Fact]
        public void Should_return_null_if_type_is_not_supported()
        {
            Assert.Null(sut.GetNameOrNull<Guid>());
        }
    }
}
