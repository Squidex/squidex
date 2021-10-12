// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Collections.Generic;
using System.Linq;
using Squidex.Domain.Apps.Core.Schemas;
using Xunit;

#pragma warning disable SA1310 // Field names must not contain underscore

namespace Squidex.Domain.Apps.Core.Model.Schemas
{
    public class SchemaFieldTests
    {
        public static readonly List<object[]> FieldProperties =
            typeof(Schema).Assembly.GetTypes()
                .Where(x => x.BaseType == typeof(FieldProperties))
                .Select(Activator.CreateInstance)
                .Select(x => new[] { x })
                .ToList()!;

        private readonly RootField<NumberFieldProperties> field_0 = Fields.Number(1, "myField", Partitioning.Invariant);

        [Fact]
        public void Should_instantiate_field()
        {
            Assert.Equal("myField", field_0.Name);
        }

        [Fact]
        public void Should_throw_exception_if_creating_field_with_invalid_name()
        {
            Assert.Throws<ArgumentException>(() => Fields.Number(1, string.Empty, Partitioning.Invariant));
        }

        [Fact]
        public void Should_hide_field()
        {
            var field_1 = field_0.Hide();
            var field_2 = field_1.Hide();

            Assert.False(field_0.IsHidden);
            Assert.True(field_2.IsHidden);
        }

        [Fact]
        public void Should_show_field()
        {
            var field_1 = field_0.Hide();
            var field_2 = field_1.Show();
            var field_3 = field_2.Show();

            Assert.True(field_1.IsHidden);
            Assert.False(field_3.IsHidden);
        }

        [Fact]
        public void Should_disable_field()
        {
            var field_1 = field_0.Disable();
            var field_2 = field_1.Disable();

            Assert.False(field_0.IsDisabled);
            Assert.True(field_2.IsDisabled);
        }

        [Fact]
        public void Should_enable_field()
        {
            var field_1 = field_0.Disable();
            var field_2 = field_1.Enable();
            var field_3 = field_2.Enable();

            Assert.True(field_1.IsDisabled);
            Assert.False(field_3.IsDisabled);
        }

        [Fact]
        public void Should_lock_field()
        {
            var field_1 = field_0.Lock();

            Assert.False(field_0.IsLocked);
            Assert.True(field_1.IsLocked);
        }

        [Fact]
        public void Should_update_field()
        {
            var field_1 = field_0.Update(new NumberFieldProperties { Hints = "my-hints" });

            Assert.Null(field_0.RawProperties.Hints);
            Assert.Equal("my-hints", field_1.RawProperties.Hints);
        }

        [Fact]
        public void Should_throw_exception_if_updating_with_invalid_properties_type()
        {
            Assert.Throws<ArgumentException>(() => field_0.Update(new StringFieldProperties()));
        }
    }
}
