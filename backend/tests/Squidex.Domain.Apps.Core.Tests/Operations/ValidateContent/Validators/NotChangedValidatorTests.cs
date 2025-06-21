// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Domain.Apps.Core.Contents;
using Squidex.Domain.Apps.Core.Schemas;
using Squidex.Domain.Apps.Core.TestHelpers;
using Squidex.Domain.Apps.Core.ValidateContent.Validators;

namespace Squidex.Domain.Apps.Core.Operations.ValidateContent.Validators;

public class NotChangedValidatorTests : IClassFixture<TranslationsFixture>
{
    private readonly IRootField field =
        Fields.String(1, "myField", Partitioning.Invariant,
            new StringFieldProperties { IsCreateOnly = true });

    private readonly List<string> errors = [];

    [Fact]
    public async Task Should_not_add_error_if_value_is_wrong_type()
    {
        var sut = new NotChangedValidator(field, []);

        await sut.ValidateAsync(true, errors);

        Assert.Empty(errors);
    }

    [Fact]
    public async Task Should_not_add_error_if_values_are_the_same()
    {
        var previousData =
            new ContentData()
                .AddField("myField",
                    new ContentFieldData()
                        .AddInvariant("Value1"));

        var newData =
            new ContentFieldData()
                .AddInvariant("Value1");

        var sut = new NotChangedValidator(field, previousData);

        await sut.ValidateAsync(newData, errors);

        Assert.Empty(errors);
    }

    [Fact]
    public async Task Should_add_error_if_values_differ()
    {
        var previousData =
            new ContentData()
                .AddField("myField",
                    new ContentFieldData()
                        .AddInvariant("Value1"));

        var newData =
            new ContentFieldData()
                .AddInvariant("Value2");

        var sut = new NotChangedValidator(field, previousData);

        await sut.ValidateAsync(newData, errors);

        errors.Should().BeEquivalentTo(
            ["iv: Field cannot be changed after creation."]);
    }

    [Fact]
    public async Task Should_add_error_if_values_differ_as_new_data_does_not_have_field()
    {
        var previousData =
            new ContentData()
                .AddField("myField",
                    new ContentFieldData()
                        .AddInvariant("Value1"));

        var newData =
            new ContentFieldData();

        var sut = new NotChangedValidator(field, previousData);

        await sut.ValidateAsync(newData, errors);

        errors.Should().BeEquivalentTo(
            ["iv: Field cannot be changed after creation."]);
    }

    [Fact]
    public async Task Should_add_error_if_values_differ_as_new_data_is_null()
    {
        var previousData =
            new ContentData()
                .AddField("myField",
                    new ContentFieldData()
                        .AddInvariant("Value1"));

        var sut = new NotChangedValidator(field, previousData);

        await sut.ValidateAsync(null, errors);

        errors.Should().BeEquivalentTo(
            ["iv: Field cannot be changed after creation."]);
    }

    [Fact]
    public async Task Should_add_error_if_values_differ_as_previous_data_does_not_have_field()
    {
        var newData =
            new ContentFieldData()
                .AddInvariant("Value2");

        var sut = new NotChangedValidator(field, []);

        await sut.ValidateAsync(newData, errors);

        errors.Should().BeEquivalentTo(
            ["iv: Field cannot be changed after creation."]);
    }

    [Fact]
    public async Task Should_add_error_if_values_differ_as_previous_data_does_not_have_value()
    {
        var previousData =
            new ContentData()
                .AddField("myField", []);

        var newData =
            new ContentFieldData()
                .AddInvariant("Value2");

        var sut = new NotChangedValidator(field, []);

        await sut.ValidateAsync(newData, errors);

        errors.Should().BeEquivalentTo(
            ["iv: Field cannot be changed after creation."]);
    }
}
