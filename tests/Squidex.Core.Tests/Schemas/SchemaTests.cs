// ==========================================================================
//  SchemaTests.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FluentAssertions;
using Squidex.Infrastructure;
using Xunit;

namespace Squidex.Core.Schemas
{
    public class SchemaTests
    {
        private Schema sut = Schema.Create("my-name", new SchemaProperties());

        private sealed class InvalidProperties : FieldProperties
        {
            protected override IEnumerable<ValidationError> ValidateCore()
            {
                yield break;
            }
        }

        [Fact]
        public void Should_instantiate_field()
        {
            var properties = new SchemaProperties { Hints = "my-hint", Label = "my-label" };

            var schema = Schema.Create("my-name", properties);

            Assert.Equal("my-name", schema.Name);
            Assert.Equal(properties, schema.Properties);

            Assert.True(properties.IsFrozen);
        }

        [Fact]
        public void Should_throw_if_creating_schema_with_invalid_name()
        {
            Assert.Throws<ValidationException>(() => Schema.Create("Invalid Name", new SchemaProperties()));
        }

        [Fact]
        public void Should_update_schema()
        {
            var properties = new SchemaProperties { Hints = "my-hint", Label = "my-label" };

            sut = sut.Update(properties);
            
            Assert.Equal(properties, sut.Properties);

            Assert.True(properties.IsFrozen);
        }
        
        [Fact]
        public void Should_add_field()
        {
            var field = AddField();

            Assert.Equal(field, sut.Fields[1]);
        }

        [Fact]
        public void Should_throw_if_adding_field_with_name_that_already_exists()
        {
            AddField();

            Assert.Throws<ValidationException>(() => sut.AddOrUpdateField(new NumberField(2, "my-field", new NumberFieldProperties())));
        }

        [Fact]
        public void Should_hide_field()
        {
            AddField();

            sut = sut.HideField(1);
            sut = sut.HideField(1);

            Assert.True(sut.Fields[1].IsHidden);
        }

        [Fact]
        public void Should_throw_if_field_to_hide_does_not_exist()
        {
            Assert.Throws<DomainObjectNotFoundException>(() => sut.HideField(1));
        }

        [Fact]
        public void Should_show_field()
        {
            AddField();

            sut = sut.HideField(1);
            sut = sut.ShowField(1);
            sut = sut.ShowField(1);

            Assert.False(sut.Fields[1].IsHidden);
        }

        [Fact]
        public void Should_throw_if_field_to_show_does_not_exist()
        {
            Assert.Throws<DomainObjectNotFoundException>(() => sut.ShowField(2));
        }

        [Fact]
        public void Should_disable_field()
        {
            AddField();

            sut = sut.DisableField(1);
            sut = sut.DisableField(1);

            Assert.True(sut.Fields[1].IsDisabled);
        }

        [Fact]
        public void Should_throw_if_field_to_disable_does_not_exist()
        {
            Assert.Throws<DomainObjectNotFoundException>(() => sut.DisableField(1));
        }

        [Fact]
        public void Should_enable_field()
        {
            AddField();

            sut = sut.DisableField(1);
            sut = sut.EnableField(1);
            sut = sut.EnableField(1);

            Assert.False(sut.Fields[1].IsDisabled);
        }

        [Fact]
        public void Should_throw_if_field_to_enable_does_not_exist()
        {
            Assert.Throws<DomainObjectNotFoundException>(() => sut.EnableField(1));
        }

        [Fact]
        public void Should_rename_field()
        {
            AddField();

            sut = sut.RenameField(1, "new-name");

            Assert.Equal("new-name", sut.Fields[1].Name);
        }

        [Fact]
        public void Should_throw_if_new_field_already_exists()
        {
            AddField();

            sut = sut.AddOrUpdateField(new NumberField(2, "other-field", new NumberFieldProperties()));

            Assert.Throws<ValidationException>(() => sut.RenameField(2, "my-field"));
        }

        [Fact]
        public void Should_throw_if_new_field_name_is_not_valid()
        {
            AddField();

            Assert.Throws<ValidationException>(() => sut.RenameField(1, "new name"));
        }

        [Fact]
        public void Should_throw_if_field_to_rename_does_not_exist()
        {
            Assert.Throws<DomainObjectNotFoundException>(() => sut.RenameField(1, "new-name"));
        }

        [Fact]
        public void Should_delete_field()
        {
            AddField();

            sut = sut.DeleteField(1);

            Assert.Equal(0, sut.Fields.Count);
        }

        [Fact]
        public void Should_not_throw_if_field_to_delete_does_not_exist()
        {
            sut.DeleteField(1);
        }

        [Fact]
        public void Should_update_field()
        {
            AddField();

            sut = sut.UpdateField(1, new NumberFieldProperties { Hints = "my-hints" });

            Assert.Equal("my-hints", sut.Fields[1].RawProperties.Hints);
        }

        [Fact]
        public void Should_throw_if_updating_field_with_invalid_property_type()
        {
            AddField();

            Assert.Throws<ArgumentException>(() => sut.UpdateField(1, new InvalidProperties()));
        }

        [Fact]
        public void Should_throw_if_field_to_update_does_not_exist()
        {
            Assert.Throws<DomainObjectNotFoundException>(() => sut.UpdateField(1, new NumberFieldProperties()));
        }

        [Fact]
        public async Task Should_add_error_if_validating_bag_with_unknown_field()
        {
            var errors = new List<ValidationError>();
            var bag = new PropertiesBag().Set("unknown", 123);

            await sut.ValidateAsync(bag, errors);

            errors.ShouldBeEquivalentTo(
                new List<ValidationError>
                {
                    new ValidationError("unknown is not a known field", "unknown")
                });
        }

        [Fact]
        public async Task Should_add_error_if_validating_bag_with_invalid_field()
        {
            sut = sut.AddOrUpdateField(new NumberField(1, "my-field", new NumberFieldProperties { MaxValue = 100 }));

            var errors = new List<ValidationError>();
            var bag = new PropertiesBag().Set("my-field", 123);

            await sut.ValidateAsync(bag, errors);

            errors.ShouldBeEquivalentTo(
                new List<ValidationError>
                {
                    new ValidationError("my-field must be less than '100'", "my-field")
                });
        }

        [Fact]
        public async Task Should_add_error_if_required_field_is_not_in_bag()
        {
            sut = sut.AddOrUpdateField(new NumberField(1, "my-field", new NumberFieldProperties { IsRequired = true }));

            var errors = new List<ValidationError>();
            var bag = new PropertiesBag();

            await sut.ValidateAsync(bag, errors);

            errors.ShouldBeEquivalentTo(
                new List<ValidationError>
                {
                    new ValidationError("my-field is required", "my-field")
                });
        }

        private NumberField AddField()
        {
            var field = new NumberField(1, "my-field", new NumberFieldProperties());

            sut = sut.AddOrUpdateField(field);

            return field;
        }
    }
}
