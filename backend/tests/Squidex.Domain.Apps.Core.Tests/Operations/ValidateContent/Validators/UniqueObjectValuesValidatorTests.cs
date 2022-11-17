// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Domain.Apps.Core.TestHelpers;
using Squidex.Domain.Apps.Core.ValidateContent.Validators;
using Squidex.Infrastructure.Json.Objects;

namespace Squidex.Domain.Apps.Core.Operations.ValidateContent.Validators;

public class UniqueObjectValuesValidatorTests : IClassFixture<TranslationsFixture>
{
    private readonly List<string> errors = new List<string>();

    [Fact]
    public async Task Should_not_add_errors_if_value_is_invalid()
    {
        var sut = new UniqueObjectValuesValidator(new[] { "myString" });

        await sut.ValidateAsync(1, errors);

        Assert.Empty(errors);
    }

    [Fact]
    public async Task Should_not_add_errors_if_value_is_null()
    {
        var sut = new UniqueObjectValuesValidator(new[] { "myString" });

        await sut.ValidateAsync(null, errors);

        Assert.Empty(errors);
    }

    [Fact]
    public async Task Should_not_add_error_if_objects_contain_not_duplicates()
    {
        var sut = new UniqueObjectValuesValidator(new[] { "myString" });

        await sut.ValidateAsync(new[]
        {
            new JsonObject()
                .Add("myString", "1"),
            new JsonObject()
                .Add("myString", "2")
        },
        errors);

        Assert.Empty(errors);
    }

    [Fact]
    public async Task Should_not_add_error_if_objects_contain_unchecked_duplicates()
    {
        var sut = new UniqueObjectValuesValidator(new[] { "myString" });

        await sut.ValidateAsync(new[]
        {
            new JsonObject()
                .Add("other", "1"),
            new JsonObject()
                .Add("other", "1")
        },
        errors);

        Assert.Empty(errors);
    }

    [Fact]
    public async Task Should_add_error_if_objects_contain_duplicates()
    {
        var sut = new UniqueObjectValuesValidator(new[] { "myString" });

        await sut.ValidateAsync(new[]
        {
            new JsonObject()
                .Add("myString", "1"),
            new JsonObject()
                .Add("myString", "1")
        },
        errors);

        errors.Should().BeEquivalentTo(
            new[] { "Must not contain items with duplicate 'myString' fields." });
    }

    [Fact]
    public async Task Should_add_errors_if_objects_contain_multiple_duplicates()
    {
        var sut = new UniqueObjectValuesValidator(new[] { "myString", "myNumber" });

        await sut.ValidateAsync(new[]
        {
            new JsonObject()
                .Add("myString", "1")
                .Add("myNumber", 1),
            new JsonObject()
                .Add("myString", "1")
                .Add("myNumber", 1)
        },
        errors);

        errors.Should().BeEquivalentTo(
            new[]
            {
                "Must not contain items with duplicate 'myString' fields.",
                "Must not contain items with duplicate 'myNumber' fields."
            });
    }
}
