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

public class RichTextFieldPropertiesTests : IClassFixture<TranslationsFixture>
{
    [Fact]
    public void Should_not_add_error()
    {
        var sut = new RichTextFieldProperties();

        var errors = FieldPropertiesValidator.Validate(sut).ToList();

        Assert.Empty(errors);
    }

    [Fact]
    public void Should_add_error_if_min_length_greater_than_max()
    {
        var sut = new RichTextFieldProperties { MinLength = 10, MaxLength = 5 };

        var errors = FieldPropertiesValidator.Validate(sut).ToList();

        errors.Should().BeEquivalentTo(
            new List<ValidationError>
            {
                new ValidationError("Max length must be greater or equal to min length.", "MinLength", "MaxLength")
            });
    }

    [Fact]
    public void Should_not_add_error_if_min_length_equal_to_max_length()
    {
        var sut = new RichTextFieldProperties { MinLength = 2, MaxLength = 2 };

        var errors = FieldPropertiesValidator.Validate(sut).ToList();

        Assert.Empty(errors);
    }

    [Fact]
    public void Should_add_error_if_min_characters_greater_than_max()
    {
        var sut = new RichTextFieldProperties { MinCharacters = 10, MaxCharacters = 5 };

        var errors = FieldPropertiesValidator.Validate(sut).ToList();

        errors.Should().BeEquivalentTo(
            new List<ValidationError>
            {
                new ValidationError("Max characters must be greater or equal to min characters.", "MinCharacters", "MaxCharacters")
            });
    }

    [Fact]
    public void Should_not_add_error_if_min_characters_equal_to_max_characters()
    {
        var sut = new RichTextFieldProperties { MinCharacters = 2, MaxCharacters = 2 };

        var errors = FieldPropertiesValidator.Validate(sut).ToList();

        Assert.Empty(errors);
    }

    [Fact]
    public void Should_add_error_if_min_words_greater_than_max()
    {
        var sut = new RichTextFieldProperties { MinWords = 10, MaxWords = 5 };

        var errors = FieldPropertiesValidator.Validate(sut).ToList();

        errors.Should().BeEquivalentTo(
            new List<ValidationError>
            {
                new ValidationError("Max words must be greater or equal to min words.", "MinWords", "MaxWords")
            });
    }

    [Fact]
    public void Should_not_add_error_if_min_words_equal_to_max_words()
    {
        var sut = new RichTextFieldProperties { MinWords = 2, MaxWords = 2 };

        var errors = FieldPropertiesValidator.Validate(sut).ToList();

        Assert.Empty(errors);
    }
}
