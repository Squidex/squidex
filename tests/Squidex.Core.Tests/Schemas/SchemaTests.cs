// ==========================================================================
//  SchemaTests.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
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
        public void Should_publish_schema()
        {
            sut = sut.Publish();

            Assert.True(sut.IsPublished);
        }

        [Fact]
        public void Should_throw_if_schema_is_already_published()
        {
            sut = sut.Publish();

            Assert.Throws<DomainException>(() => sut.Publish());
        }

        [Fact]
        public void Should_unpublish_schema()
        {
            sut = sut.Publish();
            sut = sut.Unpublish();

            Assert.False(sut.IsPublished);
        }

        [Fact]
        public void Should_throw_if_schema_is_not_published()
        {
            Assert.Throws<DomainException>(() => sut.Unpublish());
        }

        [Fact]
        public void Should_build_schema()
        {
            var schema = 
                Schema.Create("user", new SchemaProperties { Hints = "The User" })
                    .AddOrUpdateField(new StringField(1, "firstName", 
                        new StringFieldProperties { Label = "FirstName", IsLocalizable = true, IsRequired = true, AllowedValues = new [] { "1", "2" }.ToImmutableList() }))
                    .AddOrUpdateField(new StringField(2, "lastName",  
                        new StringFieldProperties { Hints = "Last Name" }))
                    .AddOrUpdateField(new BooleanField(3, "admin", 
                        new BooleanFieldProperties()))
                    .AddOrUpdateField(new NumberField(4, "age",
                        new NumberFieldProperties()));

            var json = schema.BuildSchema(new HashSet<Language>(new [] { Language.GetLanguage("de"), Language.GetLanguage("en") })).ToJson();

            Assert.NotNull(json);
        }

        private NumberField AddField()
        {
            var field = new NumberField(1, "my-field", new NumberFieldProperties());

            sut = sut.AddOrUpdateField(field);

            return field;
        }
    }
}
