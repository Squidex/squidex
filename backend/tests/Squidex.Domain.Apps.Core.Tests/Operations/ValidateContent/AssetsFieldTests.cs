// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Domain.Apps.Core.Schemas;
using Squidex.Domain.Apps.Core.TestHelpers;
using Squidex.Domain.Apps.Core.ValidateContent;
using Squidex.Domain.Apps.Core.ValidateContent.Validators;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Json.Objects;

namespace Squidex.Domain.Apps.Core.Operations.ValidateContent;

public class AssetsFieldTests : IClassFixture<TranslationsFixture>
{
    private readonly List<string> errors = new List<string>();
    private readonly DomainId asset1 = DomainId.NewGuid();
    private readonly DomainId asset2 = DomainId.NewGuid();
    private readonly IValidatorsFactory factory;

    private sealed class CustomFactory : IValidatorsFactory
    {
        public IEnumerable<IValidator> CreateValueValidators(ValidationContext context, IField field, ValidatorFactory createFieldValidator)
        {
            if (field is IField<AssetsFieldProperties> assets)
            {
                yield return new AssetsValidator(assets.Properties.IsRequired, assets.Properties, ids =>
                {
                    var actual = ids.Select(TestAssets.Document).ToList();

                    return Task.FromResult<IReadOnlyList<IAssetInfo>>(actual);
                });
            }
        }
    }

    public AssetsFieldTests()
    {
        factory = new CustomFactory();
    }

    [Fact]
    public void Should_instantiate_field()
    {
        var sut = Field(new AssetsFieldProperties());

        Assert.Equal("myAssets", sut.Name);
    }

    [Fact]
    public async Task Should_not_add_error_if_assets_are_valid()
    {
        var sut = Field(new AssetsFieldProperties
        {
            IsRequired = true,
            MinItems = 1,
            MaxItems = 3
        });

        await sut.ValidateAsync(CreateValue(asset1), errors, factory: factory);

        Assert.Empty(errors);
    }

    [Fact]
    public async Task Should_not_add_error_if_assets_are_null_and_valid()
    {
        var sut = Field(new AssetsFieldProperties());

        await sut.ValidateAsync(CreateValue(null), errors, factory: factory);

        Assert.Empty(errors);
    }

    [Fact]
    public async Task Should_not_add_error_if_number_of_assets_is_equal_to_min_and_max_items()
    {
        var sut = Field(new AssetsFieldProperties { MinItems = 2, MaxItems = 2 });

        await sut.ValidateAsync(CreateValue(asset1, asset2), errors, factory: factory);

        Assert.Empty(errors);
    }

    [Fact]
    public async Task Should_not_add_error_if_duplicate_values_are_allowed()
    {
        var sut = Field(new AssetsFieldProperties { AllowDuplicates = true });

        await sut.ValidateAsync(CreateValue(asset1, asset2), errors, factory: factory);

        Assert.Empty(errors);
    }

    [Fact]
    public async Task Should_add_error_if_assets_are_required_and_null()
    {
        var sut = Field(new AssetsFieldProperties { IsRequired = true });

        await sut.ValidateAsync(CreateValue(null), errors, factory: factory);

        errors.Should().BeEquivalentTo(
            new[] { "Field is required." });
    }

    [Fact]
    public async Task Should_add_error_if_assets_are_required_and_empty()
    {
        var sut = Field(new AssetsFieldProperties { IsRequired = true });

        await sut.ValidateAsync(CreateValue(), errors, factory: factory);

        errors.Should().BeEquivalentTo(
            new[] { "Field is required." });
    }

    [Fact]
    public async Task Should_add_error_if_value_has_not_enough_items()
    {
        var sut = Field(new AssetsFieldProperties { MinItems = 3 });

        await sut.ValidateAsync(CreateValue(asset1, asset2), errors, factory: factory);

        errors.Should().BeEquivalentTo(
            new[] { "Must have at least 3 item(s)." });
    }

    [Fact]
    public async Task Should_add_error_if_value_has_too_much_items()
    {
        var sut = Field(new AssetsFieldProperties { MaxItems = 1 });

        await sut.ValidateAsync(CreateValue(asset1, asset2), errors, factory: factory);

        errors.Should().BeEquivalentTo(
            new[] { "Must not have more than 1 item(s)." });
    }

    [Fact]
    public async Task Should_add_error_if_values_contains_duplicate()
    {
        var sut = Field(new AssetsFieldProperties());

        await sut.ValidateAsync(CreateValue(asset1, asset1), errors, factory: factory);

        errors.Should().BeEquivalentTo(
            new[] { "Must not contain duplicate values." });
    }

    private static JsonValue CreateValue(params DomainId[]? ids)
    {
        return ids == null ?
            JsonValue.Null :
            JsonValue.Array(ids.Select(x => (object)x.ToString()).ToArray());
    }

    private static RootField<AssetsFieldProperties> Field(AssetsFieldProperties properties)
    {
        return Fields.Assets(1, "myAssets", Partitioning.Invariant, properties);
    }
}
