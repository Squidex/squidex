// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Infrastructure.Reflection.Internal;

namespace Squidex.Infrastructure.Reflection;

public class PropertyAccessorTests
{
    public class TestClass
    {
        private int readWrite;
        private int readWriteVirtual;

        public int ReadWrite
        {
            get
            {
                return readWrite;
            }
            set
            {
                readWrite = value;
            }
        }

        public virtual int ReadWriteVirtual
        {
            get
            {
                return readWriteVirtual;
            }
            set
            {
                readWriteVirtual = value;
            }
        }

        public int ReadWriteAuto { get; set; }

        public int Read
        {
            get => readWrite;
        }

        public int Write
        {
            set => readWrite = value;
        }
    }

    private readonly TestClass target = new TestClass();

    [Fact]
    public void Should_set_read_write_property()
    {
        var sut = PropertyAccessor.CreateSetter<TestClass, int>(typeof(TestClass).GetProperty("ReadWrite")!);

        sut(target, 123);

        Assert.Equal(123, target.Read);
    }

    [Fact]
    public void Should_set_virtual_read_write_property()
    {
        var sut = PropertyAccessor.CreateSetter<TestClass, int>(typeof(TestClass).GetProperty("ReadWriteVirtual")!);

        sut(target, 123);

        Assert.Equal(123, target.ReadWriteVirtual);
    }

    [Fact]
    public void Should_set_auto_implemented_read_write_property()
    {
        var sut = PropertyAccessor.CreateSetter<TestClass, int>(typeof(TestClass).GetProperty("ReadWriteAuto")!);

        sut(target, 123);

        Assert.Equal(123, target.ReadWriteAuto);
    }

    [Fact]
    public void Should_set_write_property()
    {
        var sut = PropertyAccessor.CreateSetter<TestClass, int>(typeof(TestClass).GetProperty("Write")!);

        sut(target, 123);

        Assert.Equal(123, target.Read);
    }

    [Fact]
    public void Should_throw_exception_if_setting_readonly()
    {
        var sut = PropertyAccessor.CreateSetter<TestClass, int>(typeof(TestClass).GetProperty("Read")!);

        Assert.Throws<NotSupportedException>(() => sut(target, 123));
    }

    [Fact]
    public void Should_get_read_write_property()
    {
        var sut = PropertyAccessor.CreateGetter<TestClass, int>(typeof(TestClass).GetProperty("ReadWrite")!);

        target.Write = 123;

        Assert.Equal(123, sut(target));
    }

    [Fact]
    public void Should_get_virtual_read_write_property()
    {
        var sut = PropertyAccessor.CreateGetter<TestClass, int>(typeof(TestClass).GetProperty("ReadWriteVirtual")!);

        target.ReadWriteVirtual = 123;

        Assert.Equal(123, sut(target));
    }

    [Fact]
    public void Should_get_auto_implemented_read_write_property()
    {
        var sut = PropertyAccessor.CreateGetter<TestClass, int>(typeof(TestClass).GetProperty("ReadWriteAuto")!);

        target.ReadWriteAuto = 123;

        Assert.Equal(123, sut(target));
    }

    [Fact]
    public void Should_get_read_property()
    {
        var sut = PropertyAccessor.CreateGetter<TestClass, int>(typeof(TestClass).GetProperty("Read")!);

        target.Write = 123;

        Assert.Equal(123, sut(target));
    }

    [Fact]
    public void Should_throw_exception_if_getting_writeonly_property()
    {
        var sut = PropertyAccessor.CreateGetter<TestClass, int>(typeof(TestClass).GetProperty("Write")!);

        Assert.Throws<NotSupportedException>(() => sut(target));
    }
}
