// ==========================================================================
//  SchemaFieldTests.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System;
using Squidex.Domain.Apps.Core.Schemas;
using Xunit;

namespace Squidex.Domain.Apps.Core.Model.Schemas
{
    public class SchemaFieldTests
    {
        private readonly NumberField sut = new NumberField(1, "my-field", Partitioning.Invariant);

        [Fact]
        public void Should_instantiate_field()
        {
            Assert.Equal("my-field", sut.Name);
        }

        [Fact]
        public void Should_throw_exception_if_creating_field_with_invalid_name()
        {
            Assert.Throws<ArgumentException>(() => new NumberField(1, string.Empty, Partitioning.Invariant));
        }

        [Fact]
        public void Should_hide_field()
        {
            sut.Hide();
            sut.Hide();

            Assert.True(sut.IsHidden);
        }

        [Fact]
        public void Should_show_field()
        {
            sut.Hide();
            sut.Show();
            sut.Show();

            Assert.False(sut.IsHidden);
        }

        [Fact]
        public void Should_disable_field()
        {
            sut.Disable();
            sut.Disable();

            Assert.True(sut.IsDisabled);
        }

        [Fact]
        public void Should_enable_field()
        {
            sut.Disable();
            sut.Enable();
            sut.Enable();

            Assert.False(sut.IsDisabled);
        }

        [Fact]
        public void Should_lock_field()
        {
            sut.Lock();

            Assert.True(sut.IsLocked);
        }

        [Fact]
        public void Should_update_field()
        {
            sut.Update(new NumberFieldProperties { Hints = "my-hints" });

            Assert.Equal("my-hints", sut.RawProperties.Hints);
        }

        [Fact]
        public void Should_throw_exception_if_updating_with_invalid_properties_type()
        {
            Assert.Throws<ArgumentException>(() => sut.Update(new StringFieldProperties()));
        }
    }
}
