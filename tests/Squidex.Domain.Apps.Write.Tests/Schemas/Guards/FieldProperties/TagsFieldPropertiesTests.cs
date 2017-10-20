// ==========================================================================
//  TagsFieldPropertiesTests.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using Squidex.Domain.Apps.Core.Schemas;
using Squidex.Infrastructure;
using Xunit;

namespace Squidex.Domain.Apps.Write.Schemas.Guards.FieldProperties
{
    public class TagsFieldPropertiesTests
    {
        [Fact]
        public void Should_add_error_if_min_greater_than_max()
        {
            var sut = new TagsFieldProperties { MinItems = 10, MaxItems = 5 };

            var errors = FieldPropertiesValidator.Validate(sut).ToList();

            errors.ShouldBeEquivalentTo(
                new List<ValidationError>
                {
                    new ValidationError("Max items must be greater than min items.", "MinItems", "MaxItems")
                });
        }
    }
}
