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

public class ReferencesFieldPropertiesTests : IClassFixture<TranslationsFixture>
{
    [Fact]
    public void Should_add_error_if_min_items_greater_than_max_items()
    {
        var sut = new ReferencesFieldProperties { MinItems = 10, MaxItems = 5 };

        var errors = FieldPropertiesValidator.Validate(sut).ToList();

        errors.Should().BeEquivalentTo(
            new List<ValidationError>
            {
                new ValidationError("Max items must be greater or equal to min items.", "MinItems", "MaxItems")
            });
    }

    [Fact]
    public void Should_add_error_if_editor_is_not_valid()
    {
        var sut = new ReferencesFieldProperties { Editor = (ReferencesFieldEditor)123 };

        var errors = FieldPropertiesValidator.Validate(sut).ToList();

        errors.Should().BeEquivalentTo(
            new List<ValidationError>
            {
                new ValidationError("Editor is not a valid value.", "Editor")
            });
    }

    [Fact]
    public void Should_add_error_if_resolving_references_with_more_than_one_max_items()
    {
        var sut = new ReferencesFieldProperties { ResolveReference = true, MaxItems = 2 };

        var errors = FieldPropertiesValidator.Validate(sut).ToList();

        errors.Should().BeEquivalentTo(
            new List<ValidationError>
            {
                new ValidationError("Can only resolve references when MaxItems is 1.", "ResolveReference", "MaxItems")
            });
    }

    [Fact]
    public void Should_not_add_error_if_min_items_greater_equals_to_max_items()
    {
        var sut = new ReferencesFieldProperties { MinItems = 2, MaxItems = 2 };

        var errors = FieldPropertiesValidator.Validate(sut).ToList();

        Assert.Empty(errors);
    }
}
