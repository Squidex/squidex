// ==========================================================================
//  LanguageTests.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System;
using System.Linq;
using Newtonsoft.Json;
using Squidex.Infrastructure.Json;
using Xunit;

namespace Squidex.Infrastructure
{
    public class LanguageTests
    {
        private static readonly JsonSerializerSettings serializerSettings = new JsonSerializerSettings();

        static LanguageTests()
        {
            serializerSettings.Converters.Add(new LanguageConverter());
            serializerSettings.NullValueHandling = NullValueHandling.Include;
        }

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

        [Fact]
        public void Should_serialize_and_deserialize_null_language()
        {
            var input = Tuple.Create<Language>(null);
            var json = JsonConvert.SerializeObject(input, serializerSettings);
            var output = JsonConvert.DeserializeObject<Tuple<Language>>(json, serializerSettings);

            Assert.Equal(output.Item1, input.Item1);
        }

        [Fact]
        public void Should_serialize_and_deserialize_valid_language()
        {
            var input = Tuple.Create(Language.DE);
            var json = JsonConvert.SerializeObject(input, serializerSettings);
            var output = JsonConvert.DeserializeObject<Tuple<Language>>(json, serializerSettings);

            Assert.Equal(output.Item1, input.Item1);
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

        [Theory]
        [InlineData("iv", "iv")]
        [InlineData("en", "en")]
        [InlineData("EN", "en")]
        [InlineData("en ", "en")]
        public void Should_parse_valid_languages(string input, string languageCode)
        {
            var language = Language.TryParse(input);
            
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
            var language = Language.TryParse(input);

            Assert.Null(language);
        }
    }
}
