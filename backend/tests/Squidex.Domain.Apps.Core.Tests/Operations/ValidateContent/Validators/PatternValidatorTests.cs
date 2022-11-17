// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Domain.Apps.Core.TestHelpers;
using Squidex.Domain.Apps.Core.ValidateContent.Validators;

namespace Squidex.Domain.Apps.Core.Operations.ValidateContent.Validators;

public class PatternValidatorTests : IClassFixture<TranslationsFixture>
{
    private readonly List<string> errors = new List<string>();

    [Fact]
    public async Task Should_not_add_error_if_value_is_valid()
    {
        var sut = new PatternValidator("[a-z]{3}:[0-9]{2}");

        await sut.ValidateAsync("abc:12", errors);

        Assert.Empty(errors);
    }

    [Fact]
    public async Task Should_not_add_error_if_value_is_null()
    {
        var sut = new PatternValidator("[a-z]{3}:[0-9]{2}");

        await sut.ValidateAsync(null, errors);

        Assert.Empty(errors);
    }

    [Fact]
    public async Task Should_not_add_error_if_value_is_empty()
    {
        var sut = new PatternValidator("[a-z]{3}:[0-9]{2}");

        await sut.ValidateAsync(string.Empty, errors);

        Assert.Empty(errors);
    }

    [Fact]
    public async Task Should_add_error_with_default_message_if_value_is_not_valid()
    {
        var sut = new PatternValidator("[a-z]{3}:[0-9]{2}");

        await sut.ValidateAsync("foo", errors);

        errors.Should().BeEquivalentTo(
            new[] { "Must follow the pattern." });
    }

    [Fact]
    public async Task Should_add_error_with_custom_message_if_value_is_not_valid()
    {
        var sut = new PatternValidator("[a-z]{3}:[0-9]{2}", "Custom Error Message.");

        await sut.ValidateAsync("foo", errors);

        errors.Should().BeEquivalentTo(
            new[] { "Custom Error Message." });
    }

    [Fact]
    public async Task Should_add_timeout_error_if_regex_is_too_slow()
    {
        var sut = new PatternValidator(@"^(https?:\/\/)?([\da-z\.-]+)\.([a-z\.]{2,6})([\/\w \.-]*)*\/?$", capture: true);

        await sut.ValidateAsync("https://archiverbx.blob.core.windows.net/static/C:/Users/USR/Documents/Projects/PROJ/static/images/full/1234567890.jpg", errors);

        errors.Should().BeEquivalentTo(
            new[] { "Regex is too slow." });
    }
}
