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
    public class ArrayfieldTests
    {
        private readonly ArrayField parent_0 = Fields.Array(100, "root", Partitioning.Invariant);

        [Fact]
        public void Should_add_field()
        {
            var field = CreateField(1);

            var parent_1 = parent_0.AddField(field);

            Assert.Empty(parent_0.Fields);
            Assert.Equal(field, parent_1.FieldsById[1]);
        }

        [Fact]
        public void Should_throw_exception_if_adding_field_with_name_that_already_exists()
        {
            var parent_1 = parent_0.AddField(CreateField(1));

            Assert.Throws<ArgumentException>(() => parent_1.AddNumber(2, "myField1"));
        }

        [Fact]
        public void Should_throw_exception_if_adding_field_with_id_that_already_exists()
        {
            var parent_1 = parent_0.AddField(CreateField(1));

            Assert.Throws<ArgumentException>(() => parent_1.AddNumber(1, "myField2"));
        }

        [Fact]
        public void Should_hide_field()
        {
            var parent_1 = parent_0.AddField(CreateField(1));

            var parent_2 = parent_1.UpdateField(1, f => f.Hide());
            var parent_3 = parent_2.UpdateField(1, f => f.Hide());

            Assert.False(parent_1.FieldsById[1].IsHidden);
            Assert.True(parent_2.FieldsById[1].IsHidden);
            Assert.True(parent_3.FieldsById[1].IsHidden);
            Assert.Same(parent_2, parent_3);
        }

        [Fact]
        public void Should_return_same_parent_if_field_to_hide_does_not_exist()
        {
            var parent_1 = parent_0.UpdateField(1, f => f.Hide());

            Assert.Same(parent_0, parent_1);
        }

        [Fact]
        public void Should_show_field()
        {
            var parent_1 = parent_0.AddField(CreateField(1));

            var parent_2 = parent_1.UpdateField(1, f => f.Hide());
            var parent_3 = parent_2.UpdateField(1, f => f.Show());
            var parent_4 = parent_3.UpdateField(1, f => f.Show());

            Assert.True(parent_2.FieldsById[1].IsHidden);
            Assert.False(parent_3.FieldsById[1].IsHidden);
            Assert.False(parent_4.FieldsById[1].IsHidden);
            Assert.Same(parent_3, parent_4);
        }

        [Fact]
        public void Should_return_same_parent_if_field_to_show_does_not_exist()
        {
            var parent_1 = parent_0.UpdateField(1, f => f.Show());

            Assert.Same(parent_0, parent_1);
        }

        [Fact]
        public void Should_disable_field()
        {
            var parent_1 = parent_0.AddField(CreateField(1));

            var parent_2 = parent_1.UpdateField(1, f => f.Disable());
            var parent_3 = parent_2.UpdateField(1, f => f.Disable());

            Assert.False(parent_1.FieldsById[1].IsDisabled);
            Assert.True(parent_2.FieldsById[1].IsDisabled);
            Assert.True(parent_3.FieldsById[1].IsDisabled);
            Assert.Same(parent_2, parent_3);
        }

        [Fact]
        public void Should_return_same_parent_if_field_to_disable_does_not_exist()
        {
            var parent_1 = parent_0.UpdateField(1, f => f.Disable());

            Assert.Same(parent_0, parent_1);
        }

        [Fact]
        public void Should_enable_field()
        {
            var parent_1 = parent_0.AddField(CreateField(1));

            var parent_2 = parent_1.UpdateField(1, f => f.Disable());
            var parent_3 = parent_2.UpdateField(1, f => f.Enable());
            var parent_4 = parent_3.UpdateField(1, f => f.Enable());

            Assert.True(parent_2.FieldsById[1].IsDisabled);
            Assert.False(parent_3.FieldsById[1].IsDisabled);
            Assert.False(parent_4.FieldsById[1].IsDisabled);
            Assert.Same(parent_3, parent_4);
        }

        [Fact]
        public void Should_return_same_parent_if_field_to_enable_does_not_exist()
        {
            var parent_1 = parent_0.UpdateField(1, f => f.Enable());

            Assert.Same(parent_0, parent_1);
        }

        [Fact]
        public void Should_update_field()
        {
            var properties1 = new NumberFieldProperties
            {
                MinValue = 10
            };
            var properties2 = new NumberFieldProperties
            {
                MinValue = 10
            };

            var parent_1 = parent_0.AddField(CreateField(1));
            var parent_2 = parent_1.UpdateField(1, f => f.Update(properties1));
            var parent_3 = parent_2.UpdateField(1, f => f.Update(properties2));

            Assert.NotSame(properties1, parent_1.FieldsById[1].RawProperties);
            Assert.Same(properties1, parent_2.FieldsById[1].RawProperties);
            Assert.Same(properties1, parent_3.FieldsById[1].RawProperties);
            Assert.Same(parent_2, parent_3);
        }

        [Fact]
        public void Should_throw_exception_if_updating_with_invalid_properties_type()
        {
            var parent_1 = parent_0.AddField(CreateField(1));

            Assert.Throws<ArgumentException>(() => parent_1.UpdateField(1, f => f.Update(new StringFieldProperties())));
        }

        [Fact]
        public void Should_return_same_parent_if_field_to_update_does_not_exist()
        {
            var parent_1 = parent_0.UpdateField(1, f => f.Update(new StringFieldProperties()));

            Assert.Same(parent_0, parent_1);
        }

        [Fact]
        public void Should_delete_field()
        {
            var parent_1 = parent_0.AddField(CreateField(1));
            var parent_2 = parent_1.DeleteField(1);
            var parent_3 = parent_2.DeleteField(1);

            Assert.Empty(parent_2.FieldsById);
            Assert.Empty(parent_3.FieldsById);
            Assert.Same(parent_2, parent_3);
        }

        [Fact]
        public void Should_return_same_parent_if_field_to_delete_does_not_exist()
        {
            var parent_1 = parent_0.DeleteField(1);

            Assert.Same(parent_0, parent_1);
        }

        [Fact]
        public void Should_reorder_fields()
        {
            var field1 = CreateField(1);
            var field2 = CreateField(2);
            var field3 = CreateField(3);

            var parent_1 = parent_0.AddField(field1);
            var parent_2 = parent_1.AddField(field2);
            var parent_3 = parent_2.AddField(field3);
            var parent_4 = parent_3.ReorderFields(new List<long> { 3, 2, 1 });
            var parent_5 = parent_4.ReorderFields(new List<long> { 3, 2, 1 });

            Assert.Equal(new List<NestedField> { field3, field2, field1 }, parent_4.Fields.ToList());
            Assert.Equal(new List<NestedField> { field3, field2, field1 }, parent_5.Fields.ToList());
            Assert.Same(parent_4, parent_5);
        }

        [Fact]
        public void Should_throw_exception_if_not_all_fields_are_covered_for_reordering()
        {
            var field1 = CreateField(1);
            var field2 = CreateField(2);

            var parent_1 = parent_0.AddField(field1);
            var parent_2 = parent_1.AddField(field2);

            Assert.Throws<ArgumentException>(() => parent_2.ReorderFields(new List<long> { 1 }));
        }

        [Fact]
        public void Should_throw_exception_if_field_to_reorder_does_not_exist()
        {
            var field1 = CreateField(1);
            var field2 = CreateField(2);

            var parent_1 = parent_0.AddField(field1);
            var parent_2 = parent_1.AddField(field2);

            Assert.Throws<ArgumentException>(() => parent_2.ReorderFields(new List<long> { 1, 4 }));
        }

        private static NestedField<NumberFieldProperties> CreateField(int id)
        {
            return Fields.Number(id, $"myField{id}");
        }
    }
}
