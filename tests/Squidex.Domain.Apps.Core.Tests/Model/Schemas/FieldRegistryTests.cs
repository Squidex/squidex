// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using Squidex.Domain.Apps.Core.Schemas;
using Squidex.Infrastructure;
using Xunit;

namespace Squidex.Domain.Apps.Core.Model.Schemas
{
    public class FieldRegistryTests
    {
        private readonly FieldRegistry sut = new FieldRegistry(new TypeNameRegistry());

        private sealed class InvalidProperties : FieldProperties
        {
            public override T Accept<T>(IFieldPropertiesVisitor<T> visitor)
            {
                return default(T);
            }

            public override Field CreateField(long id, string name, Partitioning partitioning)
            {
                return null;
            }
        }

        [Fact]
        public void Should_throw_exception_if_creating_field_and_field_is_not_registered()
        {
            Assert.Throws<InvalidOperationException>(() => sut.CreateField(1, "name", Partitioning.Invariant, new InvalidProperties()));
        }

        [Theory]
        [InlineData(
            typeof(AssetsFieldProperties),
            typeof(AssetsField))]
        [InlineData(
            typeof(BooleanFieldProperties),
            typeof(BooleanField))]
        [InlineData(
            typeof(DateTimeFieldProperties),
            typeof(DateTimeField))]
        [InlineData(
            typeof(GeolocationFieldProperties),
            typeof(GeolocationField))]
        [InlineData(
            typeof(JsonFieldProperties),
            typeof(JsonField))]
        [InlineData(
            typeof(NumberFieldProperties),
            typeof(NumberField))]
        [InlineData(
            typeof(ReferencesFieldProperties),
            typeof(ReferencesField))]
        [InlineData(
            typeof(StringFieldProperties),
            typeof(StringField))]
        [InlineData(
            typeof(TagsFieldProperties),
            typeof(TagsField))]
        public void Should_create_field_by_properties(Type propertyType, Type fieldType)
        {
            var properties = (FieldProperties)Activator.CreateInstance(propertyType);

            var field = sut.CreateField(1, "name", Partitioning.Invariant, properties);

            Assert.Equal(properties, field.RawProperties);
            Assert.Equal(fieldType, field.GetType());
        }
    }
}
