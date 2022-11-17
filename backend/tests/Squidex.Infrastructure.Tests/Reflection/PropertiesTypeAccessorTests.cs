// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Infrastructure.Reflection.Internal;

namespace Squidex.Infrastructure.Reflection;

public class PropertiesTypeAccessorTests
{
    public class TestClass
    {
        private int target;

        public int ReadWrite
        {
            get
            {
                return target;
            }
            set
            {
                target = value;
            }
        }

        public int Read
        {
            get => target;
        }

        public int Write
        {
            set => target = value;
        }
    }

    private readonly TestClass target = new TestClass();

    [Fact]
    public void Should_set_read_write_property()
    {
        var sut = new PropertyAccessor(typeof(TestClass).GetProperty("ReadWrite")!);

        sut.Set(target, 123);

        Assert.Equal(123, target.Read);
    }

    [Fact]
    public void Should_set_write_property()
    {
        var accessor = new PropertyAccessor(typeof(TestClass).GetProperty("Write")!);

        accessor.Set(target, 123);

        Assert.Equal(123, target.Read);
    }

    [Fact]
    public void Should_throw_exception_if_setting_readonly()
    {
        var sut = new PropertyAccessor(typeof(TestClass).GetProperty("Read")!);

        Assert.Throws<NotSupportedException>(() => sut.Set(target, 123));
    }

    [Fact]
    public void Should_get_read_write_property()
    {
        var sut = new PropertyAccessor(typeof(TestClass).GetProperty("ReadWrite")!);

        target.Write = 123;

        Assert.Equal(123, sut.Get(target));
    }

    [Fact]
    public void Should_get_read_property()
    {
        var sut = new PropertyAccessor(typeof(TestClass).GetProperty("Read")!);

        target.Write = 123;

        Assert.Equal(123, sut.Get(target));
    }

    [Fact]
    public void Should_throw_exception_if_getting_writeonly_property()
    {
        var sut = new PropertyAccessor(typeof(TestClass).GetProperty("Write")!);

        Assert.Throws<NotSupportedException>(() => sut.Get(target));
    }
}
