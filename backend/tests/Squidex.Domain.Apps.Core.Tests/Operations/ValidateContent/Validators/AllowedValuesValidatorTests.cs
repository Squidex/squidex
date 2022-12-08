// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Domain.Apps.Core.TestHelpers;
using Squidex.Domain.Apps.Core.ValidateContent.Validators;

namespace Squidex.Domain.Apps.Core.Operations.ValidateContent.Validators;

public class AllowedValuesValidatorTests : IClassFixture<TranslationsFixture>
{
    private readonly List<string> errors = new List<string>();

    [Fact]
    public async Task Should_not_add_error_if_value_null()
    {
        var sut = new AllowedValuesValidator<int>(100, 200);

        await sut.ValidateAsync(null, errors);

        Assert.Empty(errors);
    }

    [Fact]
    public async Task Should_not_add_error_if_value_is_allowed()
    {
        var sut = new AllowedValuesValidator<int>(100, 200);

        await sut.ValidateAsync(100, errors);

        Assert.Empty(errors);
    }

    [Fact]
    public async Task Should_add_error_if_value_is_not_allowed()
    {
        var sut = new AllowedValuesValidator<int>(100, 200);

        await sut.ValidateAsync(50, errors);

        errors.Should().BeEquivalentTo(
            new[] { "Not an allowed value." });
    }
}