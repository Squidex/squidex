// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Domain.Apps.Core.Contents;
using Squidex.Domain.Apps.Core.TestHelpers;
using Squidex.Domain.Apps.Core.ValidateContent.Validators;
using Squidex.Infrastructure;

namespace Squidex.Domain.Apps.Core.Operations.ValidateContent.Validators;

public class UniqueValidatorTests : IClassFixture<TranslationsFixture>
{
    private readonly List<string> errors = new List<string>();

    [Fact]
    public async Task Should_not_check_uniqueness_if_localized_string()
    {
        var filter = string.Empty;

        var sut = new UniqueValidator(FoundDuplicates(DomainId.NewGuid(), f => filter = f));

        await sut.ValidateAsync(12.5, errors, updater: c => c.Nested("property").Nested("de"));

        Assert.Empty(errors);
        Assert.Empty(filter);
    }

    [Fact]
    public async Task Should_not_add_error_if_value_is_null()
    {
        var sut = new UniqueValidator(FoundDuplicates(DomainId.NewGuid()));

        await sut.ValidateAsync(null, errors);

        Assert.Empty(errors);
    }

    [Fact]
    public async Task Should_not_add_error_if_same_content_with_string_value_found()
    {
        var contentId = DomainId.NewGuid();

        var sut = new UniqueValidator(FoundDuplicates(contentId));

        await sut.ValidateAsync("hello", errors, contentId: contentId, updater: ctx => ctx.Nested("property").Nested("iv"));

        Assert.Empty(errors);
    }

    [Fact]
    public async Task Should_not_add_error_if_same_content_with_double_value_found()
    {
        var contentId = DomainId.NewGuid();

        var sut = new UniqueValidator(FoundDuplicates(contentId));

        await sut.ValidateAsync(12.5, errors, contentId: contentId, updater: ctx => ctx.Nested("property").Nested("iv"));

        Assert.Empty(errors);
    }

    [Fact]
    public async Task Should_add_error_if_other_content_with_string_value_found()
    {
        var filter = string.Empty;

        var sut = new UniqueValidator(FoundDuplicates(DomainId.NewGuid(), f => filter = f));

        await sut.ValidateAsync("hello", errors, updater: c => c.Nested("property").Nested("iv"));

        errors.Should().BeEquivalentTo(
            new[] { "property.iv: Another content with the same value exists." });

        Assert.Equal("Data.property.iv == 'hello'", filter);
    }

    [Fact]
    public async Task Should_add_error_if_other_content_with_double_value_found()
    {
        var filter = string.Empty;

        var sut = new UniqueValidator(FoundDuplicates(DomainId.NewGuid(), f => filter = f));

        await sut.ValidateAsync(12.5, errors, updater: c => c.Nested("property").Nested("iv"));

        errors.Should().BeEquivalentTo(
            new[] { "property.iv: Another content with the same value exists." });

        Assert.Equal("Data.property.iv == 12.5", filter);
    }

    private static CheckUniqueness FoundDuplicates(DomainId id, Action<string>? filter = null)
    {
        return filterNode =>
        {
            filter?.Invoke(filterNode.ToString());

            var foundIds = new List<ContentIdStatus>
            {
                new ContentIdStatus(id, id, Status.Draft)
            };

            return Task.FromResult<IReadOnlyList<ContentIdStatus>>(foundIds);
        };
    }
}
