// =========================================================================
//  LanguageTests.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System;
using System.Linq;
using Xunit;

namespace Squidex.Infrastructure
{
    public class LanguageTests
    {
        [Theory]
        [InlineData("")]
        [InlineData(" ")]
        public void Should_throw_if_getting_by_empty_key(string key)
        {
            Assert.Throws<ArgumentException>(() => Language.GetLanguage(key));
        }

        [Fact]
        public void Should_throw_if_getting_by_null_key()
        {
            Assert.Throws<ArgumentNullException>(() => Language.GetLanguage(null));
        }

        [Fact]
        public void Should_throw_if_getting_by_unsupported_language()
        {
            Assert.Throws<NotSupportedException>(() => Language.GetLanguage("xy"));
        }

        [Fact]
        public void Should_provide_all_languages()
        {
            Assert.True(Language.AllLanguages.Count() > 100);
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
        }
    }
}
