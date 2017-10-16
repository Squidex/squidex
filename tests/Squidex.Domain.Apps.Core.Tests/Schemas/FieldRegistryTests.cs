// ==========================================================================
//  FieldRegistryTests.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System;
using Newtonsoft.Json.Linq;
using Squidex.Infrastructure;
using Xunit;

namespace Squidex.Domain.Apps.Core.Schemas
{
    public class FieldRegistryTests
    {
        private readonly FieldRegistry sut = new FieldRegistry(new TypeNameRegistry());

        private sealed class InvalidProperties : FieldProperties
        {
            public override JToken GetDefaultValue()
            {
                return null;
            }

            public override T Accept<T>(IFieldPropertiesVisitor<T> visitor)
            {
                return default(T);
            }
        }

        [Fact]
        public void Should_throw_exception_if_creating_field_and_field_is_not_registered()
        {
            Assert.Throws<InvalidOperationException>(() => sut.CreateField(1, "name", Partitioning.Invariant, new InvalidProperties()));
        }

        [Fact]
        public void Should_create_field_by_properties()
        {
            var properties = new NumberFieldProperties();

            var field = sut.CreateField(1, "name", Partitioning.Invariant,  properties);

            Assert.Equal(properties, field.RawProperties);
        }
    }
}
