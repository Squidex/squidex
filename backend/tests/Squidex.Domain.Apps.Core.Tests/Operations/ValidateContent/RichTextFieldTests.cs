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

        await sut.ValidateAsync(CreateValue(null), errors);

        Assert.Empty(errors);
    }

    [Fact]
    public async Task Should_not_add_error_if_rich_text_is_valid()
    {
        var sut = Field(new RichTextFieldProperties());

        await sut.ValidateAsync(CreateValue("text"), errors);

        Assert.Empty(errors);
    }

    [Fact]
    public async Task Should_not_add_error_if_rich_text_is_invalid()
    {
        var sut = Field(new RichTextFieldProperties());

        await sut.ValidateAsync(CreateValue(string.Empty, "unknown"), errors);

        errors.Should().BeEquivalentTo(
            new[] { "Invalid rich text." });
    }

    [Fact]
    public async Task Should_add_error_if_string_is_required_but_null()
    {
        var sut = Field(new RichTextFieldProperties { IsRequired = true });

        await sut.ValidateAsync(CreateValue(null), errors);

        errors.Should().BeEquivalentTo(
            new[] { "Field is required." });
    }

    [Fact]
    public async Task Should_add_error_if_string_is_required_but_empty()
    {
        var sut = Field(new RichTextFieldProperties { IsRequired = true });

        await sut.ValidateAsync(CreateValue(string.Empty), errors);

        errors.Should().BeEquivalentTo(
            new[] { "Field is required." });
    }

    [Fact]
    public async Task Should_add_error_if_string_is_shorter_than_min_length()
    {
        var sut = Field(new RichTextFieldProperties { MinLength = 10 });

        await sut.ValidateAsync(CreateValue("123"), errors);

        errors.Should().BeEquivalentTo(
            new[] { "Must have at least 10 character(s)." });
    }

    [Fact]
    public async Task Should_add_error_if_string_is_longer_than_max_length()
    {
        var sut = Field(new RichTextFieldProperties { MaxLength = 5 });

        await sut.ValidateAsync(CreateValue("12345678"), errors);

        errors.Should().BeEquivalentTo(
            new[] { "Must not have more than 5 character(s)." });
    }

    [Fact]
    public async Task Should_add_error_if_string_is_shorter_than_min_characters()
    {
        var sut = Field(new RichTextFieldProperties { MinCharacters = 10 });

        await sut.ValidateAsync(CreateValue("123"), errors);

        errors.Should().BeEquivalentTo(
            new[] { "Must have at least 10 text character(s)." });
    }

    [Fact]
    public async Task Should_add_error_if_string_is_longer_than_max_characters()
    {
        var sut = Field(new RichTextFieldProperties { MaxCharacters = 5 });

        await sut.ValidateAsync(CreateValue("12345678"), errors);

        errors.Should().BeEquivalentTo(
            new[] { "Must not have more than 5 text character(s)." });
    }

    [Fact]
    public async Task Should_add_error_if_string_is_shorter_than_min_words()
    {
        var sut = Field(new RichTextFieldProperties { MinWords = 10 });

        await sut.ValidateAsync(CreateValue("word1 word2 word3"), errors);

        errors.Should().BeEquivalentTo(
            new[] { "Must have at least 10 word(s)." });
    }

    [Fact]
    public async Task Should_add_error_if_string_is_longer_than_max_words()
    {
        var sut = Field(new RichTextFieldProperties { MaxWords = 5 });

        await sut.ValidateAsync(CreateValue("word1 word2 word3 word4 word5 word6"), errors);

        errors.Should().BeEquivalentTo(
            new[] { "Must not have more than 5 word(s)." });
    }

    private static JsonValue CreateValue(string? v, string? type = null)
    {
        if (v == null)
        {
            return default;
        }

        return JsonValue.Object()
            .Add("type", "doc")
            .Add("content", JsonValue.Array(
                JsonValue.Object()
                    .Add("type", "paragraph")
                    .Add("content", JsonValue.Array(
                        JsonValue.Object()
                            .Add("type", type ?? "text")
                            .Add("text", v)))));
    }

    private static RootField<RichTextFieldProperties> Field(RichTextFieldProperties properties)
    {
        return Fields.RichText(1, "myRichText", Partitioning.Invariant, properties);
    }
}
