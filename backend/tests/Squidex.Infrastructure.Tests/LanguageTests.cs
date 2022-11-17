// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Infrastructure.TestHelpers;

namespace Squidex.Infrastructure;

public class LanguageTests
{
    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    public void Should_throw_exception_if_getting_by_empty_key(string key)
    {
        Assert.Throws<ArgumentException>(() => Language.GetLanguage(key));
    }

    [Fact]
    public void Should_throw_exception_if_getting_by_null_key()
    {
        Assert.Throws<ArgumentNullException>(() => Language.GetLanguage(null!));
    }

    [Fact]
    public void Should_provide_custom_language()
    {
        var actual = Language.GetLanguage("xy");

        Assert.Equal("xy", actual.Iso2Code);
    }

    [Fact]
    public void Should_trim_custom_language()
    {
        var actual = Language.GetLanguage("xy ");

        Assert.Equal("xy", actual.Iso2Code);
    }

    [Fact]
    public void Should_provide_default_language()
    {
        var actual = Language.GetLanguage("de");

        Assert.Same(Language.DE, actual);
    }

    [Fact]
    public void Should_provide_all_languages()
    {
        Assert.True(Language.AllLanguages.Count > 100);
    }

    [Fact]
    public void Should_return_true_for_default_language()
    {
        Assert.True(Language.IsDefault("de"));
    }

    [Fact]
    public void Should_return_false_for_custom_language()
    {
        Assert.False(Language.IsDefault("xx"));
    }

    [Fact]
    public void Should_make_implicit_conversion_to_language()
    {
        Language language = "de"!;

        Assert.Equal(Language.DE, language);
    }

    [Fact]
    public void Should_make_implicit_conversion_to_string()
    {
        string iso2Code = Language.DE!;

        Assert.Equal("de", iso2Code);
    }

    [Theory]
    [InlineData("de", "German")]
    [InlineData("en", "English")]
    [InlineData("sv", "Swedish")]
    [InlineData("zh", "Chinese")]
    public void Should_provide_correct_english_name(string key, string englishName)
    {
        var language = Language.GetLanguage(key);

        Assert.Equal(key, language.Iso2Code);
        Assert.Equal(englishName, language.EnglishName);
        Assert.Equal(englishName, language.ToString());
    }

    [Theory]
    [InlineData("en", "en")]
    [InlineData("en ", "en")]
    [InlineData("EN", "en")]
    [InlineData("EN ", "en")]
    public void Should_parse_valid_languages(string input, string languageCode)
    {
        var language = Language.ParseOrNull(input);

        Assert.Equal(language, Language.GetLanguage(languageCode));
    }

    [Theory]
    [InlineData("en-US", "en")]
    [InlineData("en-GB", "en")]
    [InlineData("EN-US", "en")]
    [InlineData("EN-GB", "en")]
    public void Should_parse_lanuages_from_culture(string input, string languageCode)
    {
        var language = Language.ParseOrNull(input);

        Assert.Equal(language, Language.GetLanguage(languageCode));
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("xx")]
    [InlineData("invalid")]
    [InlineData(null)]
    public void Should_parse_invalid_languages(string input)
    {
        var language = Language.ParseOrNull(input);

        Assert.Null(language);
    }

    [Fact]
    public void Should_serialize_and_deserialize_null_language()
    {
        Language? value = null;

        var serialized = value.SerializeAndDeserialize();

        Assert.Equal(value, serialized);
    }

    [Fact]
    public void Should_serialize_and_deserialize_valid_language()
    {
        var value = Language.DE;

        var serialized = value.SerializeAndDeserialize();

        Assert.Equal(value, serialized);
    }
}
