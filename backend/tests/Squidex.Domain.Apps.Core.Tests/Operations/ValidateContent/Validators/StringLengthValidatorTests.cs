// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Text;
using Squidex.Domain.Apps.Core.TestHelpers;
using Squidex.Domain.Apps.Core.ValidateContent.Validators;

namespace Squidex.Domain.Apps.Core.Operations.ValidateContent.Validators;

public class StringLengthValidatorTests : IClassFixture<TranslationsFixture>
{
    private readonly List<string> errors = new List<string>();

    [Fact]
    public async Task Should_not_add_error_if_value_is_null()
    {
        var sut = new StringLengthValidator();

        await sut.ValidateAsync(null, errors);

        Assert.Empty(errors);
    }

    [Fact]
    public async Task Should_not_add_error_if_value_is_empty()
    {
        var sut = new StringLengthValidator();

        await sut.ValidateAsync(string.Empty, errors);

        Assert.Empty(errors);
    }

    [Theory]
    [InlineData(null, null)]
    [InlineData(1000, null)]
    [InlineData(1000, 2000)]
    [InlineData(null, 2000)]
    public async Task Should_not_add_error_if_value_is_within_range(int? min, int? max)
    {
        var sut = new StringLengthValidator(min, max);

        await sut.ValidateAsync(CreateString(1500), errors);

        Assert.Empty(errors);
    }

    [Theory]
    [InlineData(20, 10)]
    public void Should_throw_error_if_min_greater_than_max(int? min, int? max)
    {
        Assert.Throws<ArgumentException>(() => new StringLengthValidator(min, max));
    }

    [Fact]
    public async Task Should_add_error_if_value_has_not_exact_number_of_characters()
    {
        var sut = new StringLengthValidator(2000, 2000);

        await sut.ValidateAsync(CreateString(4), errors);

        errors.Should().BeEquivalentTo(
            new[] { "Must have exactly 2000 character(s)." });
    }

    [Fact]
    public async Task Should_add_error_if_value_is_smaller_than_min()
    {
        var sut = new StringLengthValidator(2000, null);

        await sut.ValidateAsync(CreateString(1500), errors);

        errors.Should().BeEquivalentTo(
            new[] { "Must have at least 2000 character(s)." });
    }

    [Fact]
    public async Task Should_add_error_if_value_is_greater_than_max()
    {
        var sut = new StringLengthValidator(null, 1000);

        await sut.ValidateAsync(CreateString(1500), errors);

        errors.Should().BeEquivalentTo(
            new[] { "Must not have more than 1000 character(s)." });
    }

    [Fact]
    public async Task Should_add_error_if_collection_count_is_not_in_range()
    {
        var sut = new StringLengthValidator(2000, 5000);

        await sut.ValidateAsync(CreateString(1), errors);

        errors.Should().BeEquivalentTo(
            new[] { "Must have between 2000 and 5000 character(s)." });
    }

    private static string CreateString(int size)
    {
        var sb = new StringBuilder();

        for (var i = 0; i < size; i++)
        {
            sb.Append('x');
        }

        return sb.ToString();
    }
}
