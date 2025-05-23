﻿// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Domain.Apps.Core.Schemas;
using Squidex.Domain.Apps.Core.TestHelpers;
using Squidex.Infrastructure.Validation;

namespace Squidex.Domain.Apps.Entities.Schemas.DomainObject.Guards.FieldProperties;

public class ComponentsFieldPropertiesTests : IClassFixture<TranslationsFixture>
{
    [Fact]
    public void Should_add_error_if_min_items_greater_than_max_items()
    {
        var sut = new ComponentsFieldProperties { MinItems = 10, MaxItems = 5 };

        var errors = FieldPropertiesValidator.Validate(sut).ToList();

        errors.Should().BeEquivalentTo(
            [
                new ValidationError("Max items must be greater or equal to min items.", "MinItems", "MaxItems"),
            ]);
    }

    [Fact]
    public void Should_not_add_error_if_min_items_equals_to_max_items()
    {
        var sut = new ComponentsFieldProperties { MinItems = 2, MaxItems = 2 };

        var errors = FieldPropertiesValidator.Validate(sut).ToList();

        Assert.Empty(errors);
    }
}
