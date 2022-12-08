// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Domain.Apps.Core.Schemas;
using Squidex.Domain.Apps.Core.TestHelpers;
using Squidex.Infrastructure.Validation;

namespace Squidex.Domain.Apps.Entities.Schemas.DomainObject.Guards.FieldProperties;

public class AssetsFieldPropertiesTests : IClassFixture<TranslationsFixture>
{
    [Fact]
    public void Should_add_error_if_min_items_greater_than_max_items()
    {
        var sut = new AssetsFieldProperties { MinItems = 10, MaxItems = 5 };

        var errors = FieldPropertiesValidator.Validate(sut).ToList();

        errors.Should().BeEquivalentTo(
            new List<ValidationError>
            {
                new ValidationError("Max items must be greater or equal to min items.", "MinItems", "MaxItems")
            });
    }

    [Fact]
    public void Should_not_add_error_if_min_equals_to_max_items()
    {
        var sut = new AssetsFieldProperties { MinItems = 2, MaxItems = 2 };

        var errors = FieldPropertiesValidator.Validate(sut).ToList();

        Assert.Empty(errors);
    }

    [Fact]
    public void Should_add_error_if_min_width_greater_than_max_width()
    {
        var sut = new AssetsFieldProperties { MinWidth = 10, MaxWidth = 5 };

        var errors = FieldPropertiesValidator.Validate(sut).ToList();

        errors.Should().BeEquivalentTo(
            new List<ValidationError>
            {
                new ValidationError("Max width must be greater or equal to min width.", "MinWidth", "MaxWidth")
            });
    }

    [Fact]
    public void Should_not_add_error_if_min_width_equals_to_max_width()
    {
        var sut = new AssetsFieldProperties { MinWidth = 2, MaxWidth = 2 };

        var errors = FieldPropertiesValidator.Validate(sut).ToList();

        Assert.Empty(errors);
    }

    [Fact]
    public void Should_add_error_if_min_height_greater_than_max_height()
    {
        var sut = new AssetsFieldProperties { MinHeight = 10, MaxHeight = 5 };

        var errors = FieldPropertiesValidator.Validate(sut).ToList();

        errors.Should().BeEquivalentTo(
            new List<ValidationError>
            {
                new ValidationError("Max height must be greater or equal to min height.", "MinHeight", "MaxHeight")
            });
    }

    [Fact]
    public void Should_not_add_error_if_min_height_equals_to_max_height()
    {
        var sut = new AssetsFieldProperties { MinHeight = 2, MaxHeight = 2 };

        var errors = FieldPropertiesValidator.Validate(sut).ToList();

        Assert.Empty(errors);
    }

    [Fact]
    public void Should_add_error_if_min_size_greater_than_max_size()
    {
        var sut = new AssetsFieldProperties { MinSize = 10, MaxSize = 5 };

        var errors = FieldPropertiesValidator.Validate(sut).ToList();

        errors.Should().BeEquivalentTo(
            new List<ValidationError>
            {
                new ValidationError("Max size must be greater than min size.", "MinSize", "MaxSize")
            });
    }

    [Fact]
    public void Should_add_error_if_only_aspect_width_is_defined()
    {
        var sut = new AssetsFieldProperties { AspectWidth = 10 };

        var errors = FieldPropertiesValidator.Validate(sut).ToList();

        errors.Should().BeEquivalentTo(
            new List<ValidationError>
            {
                new ValidationError("If aspect width or aspect height is used both must be defined.", "AspectWidth", "AspectHeight")
            });
    }

    [Fact]
    public void Should_add_error_if_only_aspect_height_is_defined()
    {
        var sut = new AssetsFieldProperties { AspectHeight = 10 };

        var errors = FieldPropertiesValidator.Validate(sut).ToList();

        errors.Should().BeEquivalentTo(
            new List<ValidationError>
            {
                new ValidationError("If aspect width or aspect height is used both must be defined.", "AspectWidth", "AspectHeight")
            });
    }
}
