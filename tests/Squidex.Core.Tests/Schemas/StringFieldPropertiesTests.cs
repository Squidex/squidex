// ==========================================================================
//  StringFieldPropertiesTests.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using FluentAssertions;
using Squidex.Core.Schemas;
using Squidex.Infrastructure;
using Xunit;

namespace Squidex.Core.Tests.Schemas
{
    public class StringFieldPropertiesTests
    {
        private readonly List<ValidationError> errors = new List<ValidationError>();

        [Fact]
        public void Should_add_error_if_min_greater_than_max()
        {
            var sut = new StringFieldProperties { MinLength = 10, MaxLength = 5 };

            sut.Validate(errors);

            errors.ShouldBeEquivalentTo(
                new List<ValidationError>
                {
                    new ValidationError("Max length must be greater than min length", "MinLength", "MaxLength")
                });
        }

        [Fact]
        public void Should_add_error_if_pattern_is_not_valid_regex()
        {
            var sut = new StringFieldProperties { Pattern = "[0-9{1}"};

            sut.Validate(errors);

            errors.ShouldBeEquivalentTo(
                new List<ValidationError>
                {
                    new ValidationError("Pattern is not a valid expression", "Pattern")
                });
        }

        [Fact]
        public void Should_set_or_freeze_sut()
        {
            var sut = new StringFieldProperties();

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
