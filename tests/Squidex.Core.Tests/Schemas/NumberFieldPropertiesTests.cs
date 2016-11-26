// ==========================================================================
//  NumberFieldPropertiesTests.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Reflection;
using Squidex.Core.Schemas;
using Squidex.Infrastructure;
using Xunit;
using FluentAssertions;
using System.Linq;

namespace Squidex.Core.Tests.Schemas
{
    public class NumberFieldPropertiesTests
    {
        private readonly List<ValidationError> errors = new List<ValidationError>();

        [Fact]
        public void Should_not_add_error_if_sut_are_valid()
        {
            var sut = new NumberFieldProperties
            {
                MinValue = 0,
                MaxValue = 100,
                DefaultValue = 5
            };

            sut.Validate(errors);

            Assert.Equal(0, errors.Count);
        }

        [Fact]
        public void Should_add_error_if_default_value_is_less_than_min()
        {
            var sut = new NumberFieldProperties { MinValue = 10, DefaultValue = 5 };

            sut.Validate(errors);

            errors.ShouldBeEquivalentTo(
                new List<ValidationError>
                {
                    new ValidationError("Default value must be greater than min value", "DefaultValue")
                });
        }

        [Fact]
        public void Should_add_error_if_default_value_is_greater_than_min()
        {
            var sut = new NumberFieldProperties { MaxValue = 0, DefaultValue = 5 };

            sut.Validate(errors);

            errors.ShouldBeEquivalentTo(
                new List<ValidationError>
                {
                    new ValidationError("Default value must be less than max value", "DefaultValue")
                });
        }

        [Fact]
        public void Should_add_error_if_min_greater_than_max()
        {
            var sut = new NumberFieldProperties { MinValue = 10, MaxValue = 5 };

            sut.Validate(errors);

            errors.ShouldBeEquivalentTo(
                new List<ValidationError>
                {
                    new ValidationError("Max value must be greater than min value", "MinValue", "MaxValue")
                });
        }

        [Fact]
        public void Should_add_error_if_allowed_values_and_max_is_specified()
        {
            var sut = new NumberFieldProperties { MaxValue = 10, AllowedValues = ImmutableList.Create<double>(4) };

            sut.Validate(errors);

            errors.ShouldBeEquivalentTo(
                new List<ValidationError>
                {
                    new ValidationError("Either or allowed values or range can be defined", "AllowedValues", "MinValue", "MaxValue")
                });
        }

        [Fact]
        public void Should_add_error_if_allowed_values_and_min_is_specified()
        {
            var sut = new NumberFieldProperties { MinValue = 10, AllowedValues = ImmutableList.Create<double>(4) };

            sut.Validate(errors);

            errors.ShouldBeEquivalentTo(
                new List<ValidationError>
                {
                    new ValidationError("Either or allowed values or range can be defined", "AllowedValues", "MinValue", "MaxValue")
                });
        }

        [Fact]
        public void Should_set_or_freeze_sut()
        {
            var sut = new NumberFieldProperties();

            foreach (var property in sut.GetType().GetRuntimeProperties().Where(x => x.Name != "IsFrozen"))
            {
                var value =
                    property.PropertyType.GetTypeInfo().IsValueType ?
                        Activator.CreateInstance(property.PropertyType) :
                        null;

                property.SetValue(sut, value);

                var result = property.GetValue(sut);

                Assert.Equal(value, result);
            }

            sut.Freeze();

            foreach (var property in sut.GetType().GetRuntimeProperties().Where(x => x.Name != "IsFrozen"))
            {
                var value =
                    property.PropertyType.GetTypeInfo().IsValueType ?
                        Activator.CreateInstance(property.PropertyType) :
                        null;

                Assert.Throws<InvalidOperationException>(() =>
                {
                    try
                    {
                        property.SetValue(sut, value);
                    }
                    catch (Exception e)
                    {
                        throw e.InnerException;
                    }
                });
            }
        }
    }
}
