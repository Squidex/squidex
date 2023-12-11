// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Domain.Apps.Core.Schemas;
using Squidex.Domain.Apps.Core.TestHelpers;
using Squidex.Infrastructure.Json.Objects;

namespace Squidex.Domain.Apps.Core.Operations.ValidateContent;

public class RichTextFieldTests : IClassFixture<TranslationsFixture>
{
    private readonly List<string> errors = [];

    [Fact]
    public void Should_instantiate_field()
    {
        var sut = Field(new RichTextFieldProperties());

        Assert.Equal("myRichText", sut.Name);
    }

    [Fact]
    public async Task Should_not_add_error_if_value_is_null()
    {
        var sut = Field(new RichTextFieldProperties());

        await sut.ValidateAsync(CreateValue(JsonValue.Null), errors);

        Assert.Empty(errors);
    }

    [Fact]
    public async Task Should_not_add_error_if_rich_text_is_valid()
    {
        var sut = Field(new RichTextFieldProperties());

        var value = JsonValue.Object().Add("type", "doc");

        await sut.ValidateAsync(CreateValue(value), errors);

        Assert.Empty(errors);
    }

    [Fact]
    public async Task Should_not_add_error_if_rich_text_is_invalid()
    {
        var sut = Field(new RichTextFieldProperties());

        var value = JsonValue.Object().Add("type", "unknown");

        await sut.ValidateAsync(CreateValue(value), errors);

        errors.Should().BeEquivalentTo(
            new[] { "Invalid rich text." });
    }

    [Fact]
    public async Task Should_add_error_if_json_is_required()
    {
        var sut = Field(new RichTextFieldProperties { IsRequired = true });

        await sut.ValidateAsync(CreateValue(JsonValue.Null), errors);

        errors.Should().BeEquivalentTo(
            new[] { "Field is required." });
    }

    private static JsonValue CreateValue(JsonValue v)
    {
        return v;
    }

    private static RootField<RichTextFieldProperties> Field(RichTextFieldProperties properties)
    {
        return Fields.RichText(1, "myRichText", Partitioning.Invariant, properties);
    }
}
