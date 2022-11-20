// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

namespace Squidex.Infrastructure;

public class StringExtensionsTests
{
    [Theory]
    [InlineData(null, false)]
    [InlineData("", false)]
    [InlineData("me", false)]
    [InlineData("me@@web.com", false)]
    [InlineData("me@web.com", true)]
    [InlineData("Me@web.com", true)]
    public void Should_check_email(string email, bool isEmail)
    {
        Assert.Equal(isEmail, email.IsEmail());
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    public void Should_provide_fallback_if_invalid(string value)
    {
        Assert.Equal("fallback", value.Or("fallback"));
    }

    [Fact]
    public void Should_provide_value()
    {
        const string value = "value";

        Assert.Equal(value, value.Or("fallback"));
    }

    [Fact]
    public void Should_join_non_empty_if_all_are_valid()
    {
        var actual = StringExtensions.JoinNonEmpty("_", "1", "2", "3");

        Assert.Equal("1_2_3", actual);
    }

    [Fact]
    public void Should_join_non_empty_if_first_invalid()
    {
        var actual = StringExtensions.JoinNonEmpty("_", null, "2", "3");

        Assert.Equal("2_3", actual);
    }

    [Fact]
    public void Should_join_non_empty_if_middle_invalid()
    {
        var actual = StringExtensions.JoinNonEmpty("_", "1", null, "3");

        Assert.Equal("1_3", actual);
    }

    [Fact]
    public void Should_join_non_empty_if_last_invalid()
    {
        var actual = StringExtensions.JoinNonEmpty("_", "1", "2", null);

        Assert.Equal("1_2", actual);
    }

    [Fact]
    public void Should_escape_json()
    {
        var actual = StringExtensions.JsonEscape("Hello \"World\"");

        Assert.Equal("Hello \\\"World\\\"", actual);
    }

    [Fact]
    public void Should_calculate_hex_code_from_empty_array()
    {
        var actual = Array.Empty<byte>().ToHexString();

        Assert.Equal(string.Empty, actual);
    }

    [Fact]
    public void Should_calculate_hex_code_from_byte_array()
    {
        var actual = new byte[] { 0x00, 0x01, 0xFF, 0x1A, 0x2B, 0x3C }.ToHexString();

        Assert.Equal("0001FF1A2B3C", actual);
    }

    [Theory]
    [InlineData("", "")]
    [InlineData(" ", "")]
    [InlineData("-", "")]
    [InlineData("--", "")]
    [InlineData("text1 ", "text1")]
    [InlineData("texts-", "texts")]
    [InlineData(" 1text", "1text")]
    [InlineData("-atext", "atext")]
    [InlineData("a-text", "a-text")]
    public void Should_trim_non_digits_or_letters(string input, string output)
    {
        var result = input.TrimNonLetterOrDigit();

        Assert.Equal(output, result);
    }
}
