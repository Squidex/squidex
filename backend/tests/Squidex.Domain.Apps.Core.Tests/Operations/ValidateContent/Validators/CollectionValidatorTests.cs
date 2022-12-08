// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Domain.Apps.Core.TestHelpers;
using Squidex.Domain.Apps.Core.ValidateContent.Validators;

namespace Squidex.Domain.Apps.Core.Operations.ValidateContent.Validators;

public class CollectionValidatorTests : IClassFixture<TranslationsFixture>
{
    private readonly List<string> errors = new List<string>();

    [Theory]
    [InlineData(20, 10)]
    public void Should_throw_error_if_min_greater_than_max(int? min, int? max)
    {
        Assert.Throws<ArgumentException>(() => new CollectionValidator(true, min, max));
    }

    [Fact]
    public async Task Should_not_add_error_if_value_is_valid()
    {
        var sut = new CollectionValidator(true, 1, 3);

        await sut.ValidateAsync(new List<int> { 1, 2 }, errors);

        Assert.Empty(errors);
    }

    [Fact]
    public async Task Should_not_add_error_if_optional()
    {
        var sut = new CollectionValidator(true, 1, 3);

        await sut.ValidateAsync(null, errors, updater: c => c.Optional(true));

        Assert.Empty(errors);
    }

    [Fact]
    public async Task Should_add_error_if_value_is_null()
    {
        var sut = new CollectionValidator(true, 1, 3);

        await sut.ValidateAsync(null, errors);

        errors.Should().BeEquivalentTo(
            new[] { "Field is required." });
    }

    [Fact]
    public async Task Should_add_error_if_collection_has_not_exact_number_of_items()
    {
        var sut = new CollectionValidator(true, 2, 2);

        await sut.ValidateAsync(new List<int> { 1 }, errors);

        errors.Should().BeEquivalentTo(
            new[] { "Must have exactly 2 item(s)." });
    }

    [Fact]
    public async Task Should_add_error_if_collection_has_too_few_items()
    {
        var sut = new CollectionValidator(true, 2, null);

        await sut.ValidateAsync(new List<int> { 1 }, errors);

        errors.Should().BeEquivalentTo(
            new[] { "Must have at least 2 item(s)." });
    }

    [Fact]
    public async Task Should_add_error_if_collection_has_too_many_items()
    {
        var sut = new CollectionValidator(true, null, 3);

        await sut.ValidateAsync(new List<int> { 1, 2, 3, 4 }, errors);

        errors.Should().BeEquivalentTo(
            new[] { "Must not have more than 3 item(s)." });
    }

    [Fact]
    public async Task Should_add_error_if_collection_count_is_not_in_range()
    {
        var sut = new CollectionValidator(true, 2, 5);

        await sut.ValidateAsync(new List<int> { 1 }, errors);

        errors.Should().BeEquivalentTo(
            new[] { "Must have between 2 and 5 item(s)." });
    }
}
