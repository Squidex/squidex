// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Domain.Apps.Core.TestHelpers;
using Squidex.Domain.Apps.Core.ValidateContent.Validators;

namespace Squidex.Domain.Apps.Core.Operations.ValidateContent.Validators;

public class RequiredValidatorTests : IClassFixture<TranslationsFixture>
{
    private readonly List<string> errors = new List<string>();

    [Fact]
    public async Task Should_not_add_error_if_value_is_valid()
    {
        var sut = new RequiredValidator();

        await sut.ValidateAsync(true, errors);

        Assert.Empty(errors);
    }

    [Fact]
    public async Task Should_not_add_error_for_empty_string()
    {
        var sut = new RequiredValidator();

        await sut.ValidateAsync(string.Empty, errors);

        Assert.Empty(errors);
    }

    [Fact]
    public async Task Should_not_add_error_if_optional()
    {
        var sut = new RequiredValidator();

        await sut.ValidateAsync(null, errors, updater: c => c.Optional(true));

        Assert.Empty(errors);
    }

    [Fact]
    public async Task Should_add_error_if_value_is_null()
    {
        var sut = new RequiredValidator();

        await sut.ValidateAsync(null, errors);

        errors.Should().BeEquivalentTo(
            new[] { "Field is required." });
    }
}
