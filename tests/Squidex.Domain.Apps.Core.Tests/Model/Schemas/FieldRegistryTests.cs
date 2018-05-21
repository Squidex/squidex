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

            public override T Accept<T>(IFieldVisitor<T> visitor, IField field)
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
        [InlineData(typeof(AssetsFieldProperties))]
        [InlineData(typeof(BooleanFieldProperties))]
        [InlineData(typeof(DateTimeFieldProperties))]
        [InlineData(typeof(GeolocationFieldProperties))]
        [InlineData(typeof(JsonFieldProperties))]
        [InlineData(typeof(NumberFieldProperties))]
        [InlineData(typeof(ReferencesFieldProperties))]
        [InlineData(typeof(StringFieldProperties))]
        [InlineData(typeof(TagsFieldProperties))]
        public void Should_create_field_by_properties(Type propertyType)
        {
            var properties = (FieldProperties)Activator.CreateInstance(propertyType);

            var field = sut.CreateField(1, "name", Partitioning.Invariant, properties);

            Assert.Equal(properties, field.RawProperties);
        }
    }
}
