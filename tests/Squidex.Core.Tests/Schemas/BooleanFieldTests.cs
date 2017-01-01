// ==========================================================================
//  BooleanFieldTests.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System.Collections.Generic;
using System.Threading.Tasks;
using FluentAssertions;
using Squidex.Infrastructure;
using Xunit;

namespace Squidex.Core.Schemas
{
    public class BooleanFieldTests
    {
        private readonly List<string> errors = new List<string>();

        [Fact]
        public void Should_instantiate_field()
        {
            var sut = new BooleanField(1, "name", new BooleanFieldProperties());

            Assert.Equal("name", sut.Name);
        }

        [Fact]
        public void Should_clone_object()
        {
            var sut = new BooleanField(1, "name", new BooleanFieldProperties());

            Assert.NotEqual(sut, sut.Enable());
        }

        [Fact]
        public async Task Should_add_errors_if_boolean_is_required()
        {
            var sut = new BooleanField(1, "name", new BooleanFieldProperties { Label = "Name", IsRequired = true });

            await sut.ValidateAsync(CreateValue(null), errors);

            errors.ShouldBeEquivalentTo(
                new[] { "Name is required" });
        }

        [Fact]
        public async Task Should_add_errors_if_value_is_not_valid()
        {
            var sut = new BooleanField(1, "name", new BooleanFieldProperties { Label = "Name" });

            await sut.ValidateAsync(CreateValue("Invalid"), errors);

            errors.ShouldBeEquivalentTo(
                new[] { "Name is not a valid value" });
        }

        private static PropertyValue CreateValue(object v)
        {
            var bag = new PropertiesBag().Set("value", v);

            return bag["value"];
        }
    }
}
