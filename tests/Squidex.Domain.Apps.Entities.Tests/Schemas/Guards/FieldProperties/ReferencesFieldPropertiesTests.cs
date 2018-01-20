﻿// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using Squidex.Domain.Apps.Core.Schemas;
using Squidex.Infrastructure;
using Xunit;

namespace Squidex.Domain.Apps.Entities.Schemas.Guards.FieldProperties
{
    public class ReferencesFieldPropertiesTests
    {
        [Fact]
        public void Should_add_error_if_min_greater_than_max()
        {
            var sut = new ReferencesFieldProperties { MinItems = 10, MaxItems = 5 };

            var errors = FieldPropertiesValidator.Validate(sut).ToList();

            errors.ShouldBeEquivalentTo(
                new List<ValidationError>
                {
                    new ValidationError("Max items must be greater than min items.", "MinItems", "MaxItems")
                });
        }
    }
}
