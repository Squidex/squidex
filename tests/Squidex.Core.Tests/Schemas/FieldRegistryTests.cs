// ==========================================================================
//  FieldRegistryTests.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System;
using System.Collections.Generic;
using Moq;
using Newtonsoft.Json.Linq;
using Squidex.Infrastructure;
using Xunit;

namespace Squidex.Core.Schemas
{
    public class FieldRegistryTests
    {
        private readonly FieldRegistry sut = new FieldRegistry(new TypeNameRegistry(), new Mock<IAssetTester>().Object);

        private sealed class InvalidProperties : FieldProperties
        {
            public override JToken GetDefaultValue()
            {
                return null;
            }

            protected override IEnumerable<ValidationError> ValidateCore()
            {
                yield break;
            }
        }

        [Fact]
        public void Should_throw_if_creating_field_and_field_is_not_registered()
        {
            Assert.Throws<InvalidOperationException>(() => sut.CreateField(1, "name", new InvalidProperties()));
        }

        [Fact]
        public void Should_create_field_by_properties()
        {
            var properties = new NumberFieldProperties();

            var field = sut.CreateField(1, "name", properties);

            Assert.Equal(properties, field.RawProperties);
        }

        [Fact]
        public void Should_throw_if_getting_by_type_and_field_is_not_registered()
        {
            Assert.Throws<InvalidOperationException>(() => sut.FindByPropertiesType(typeof(InvalidProperties)));
        }

        [Fact]
        public void Should_find_registration_by_properties_type()
        {
            var registry = sut.FindByPropertiesType(typeof(NumberFieldProperties));

            Assert.Equal(typeof(NumberFieldProperties), registry.PropertiesType);
        }

        [Fact]
        public void Should_throw_if_getting_by_name_and_field_is_not_registered()
        {
            Assert.Throws<DomainException>(() => sut.FindByTypeName("invalid"));
        }

        [Fact]
        public void Should_find_registration_by_name()
        {
            var registry = sut.FindByTypeName("NumberField");

            Assert.Equal(typeof(NumberFieldProperties), registry.PropertiesType);
        }
    }
}
