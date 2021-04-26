// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions;
using Squidex.Domain.Apps.Core.TestHelpers;
using Squidex.Domain.Apps.Core.ValidateContent.Validators;
using Xunit;

namespace Squidex.Domain.Apps.Core.Operations.ValidateContent.Validators
{
    public class StringTextValidatorTests : IClassFixture<TranslationsFixture>
    {
        private readonly List<string> errors = new List<string>();

        [Theory]
        [InlineData(20, 10)]
        public void Should_throw_error_if_min_characters_greater_than_max(int? min, int? max)
        {
            Assert.Throws<ArgumentException>(() => new StringTextValidator(minCharacters: min, maxCharacters: max));
        }

        [Theory]
        [InlineData(20, 10)]
        public void Should_throw_error_if_min_words_greater_than_max(int? min, int? max)
        {
            Assert.Throws<ArgumentException>(() => new StringTextValidator(minWords: min, maxWords: max));
        }

        [Fact]
        public async Task Should_not_add_error_if_value_is_null()
        {
            var sut = new StringTextValidator(minCharacters: 5);

            await sut.ValidateAsync(null, errors);

            Assert.Empty(errors);
        }

        [Fact]
        public async Task Should_not_add_error_if_value_is_empty()
        {
            var sut = new StringTextValidator(minCharacters: 5);

            await sut.ValidateAsync(string.Empty, errors);

            Assert.Empty(errors);
        }

        [Theory]
        [InlineData(null, null)]
        [InlineData(1000, null)]
        [InlineData(1000, 2000)]
        [InlineData(null, 2000)]
        public async Task Should_not_add_error_if_value_is_within_character_range(int? min, int? max)
        {
            var sut = new StringTextValidator(minCharacters: min, maxCharacters: max);

            await sut.ValidateAsync(CreateString(1500), errors);

            Assert.Empty(errors);
        }

        [Fact]
        public async Task Should_add_error_if_value_has_not_exact_number_of_characters()
        {
            var sut = new StringTextValidator(minCharacters: 5, maxCharacters: 5);

            await sut.ValidateAsync(CreateString(4), errors);

            errors.Should().BeEquivalentTo(
                new[] { "Must have exactly 5 text character(s)." });
        }

        [Fact]
        public async Task Should_add_error_if_value_is_smaller_than_min_characters()
        {
            var sut = new StringTextValidator(minCharacters: 2000);

            await sut.ValidateAsync(CreateString(1500), errors);

            errors.Should().BeEquivalentTo(
                new[] { "Must have at least 2000 text character(s)." });
        }

        [Fact]
        public async Task Should_add_error_if_value_is_greater_than_max_characters()
        {
            var sut = new StringTextValidator(maxCharacters: 1000);

            await sut.ValidateAsync(CreateString(1500), errors);

            errors.Should().BeEquivalentTo(
                new[] { "Must not have more than 1000 text character(s)." });
        }

        [Fact]
        public async Task Should_add_error_if_collection_count_is_not_in_character_range()
        {
            var sut = new StringTextValidator(minCharacters: 2000, maxCharacters: 5000);

            await sut.ValidateAsync(CreateString(1), errors);

            errors.Should().BeEquivalentTo(
                new[] { "Must have between 2000 and 5000 text character(s)." });
        }

        [Theory]
        [InlineData(null, null, 1000)]
        [InlineData(1000, null, 1000)]
        [InlineData(1000, 2000, 1000)]
        [InlineData(null, 2000, 1000)]
        public async Task Should_not_add_error_if_value_is_within_word_range(int? min, int? max, int length)
        {
            var sut = new StringTextValidator(minWords: min, maxWords: max);

            await sut.ValidateAsync(CreateSentence(length), errors);

            Assert.Empty(errors);
        }

        [Fact]
        public async Task Should_add_error_if_value_has_not_exact_number_of_words()
        {
            var sut = new StringTextValidator(minWords: 5, maxWords: 5);

            await sut.ValidateAsync(CreateSentence(4), errors);

            errors.Should().BeEquivalentTo(
                new[] { "Must have exactly 5 word(s)." });
        }

        [Fact]
        public async Task Should_add_error_if_value_is_smaller_than_min_words()
        {
            var sut = new StringTextValidator(minWords: 2000);

            await sut.ValidateAsync(CreateSentence(1500), errors);

            errors.Should().BeEquivalentTo(
                new[] { "Must have at least 2000 word(s)." });
        }

        [Fact]
        public async Task Should_add_error_if_value_is_greater_than_max_words()
        {
            var sut = new StringTextValidator(maxWords: 1000);

            await sut.ValidateAsync(CreateSentence(1500), errors);

            errors.Should().BeEquivalentTo(
                new[] { "Must not have more than 1000 word(s)." });
        }

        [Fact]
        public async Task Should_add_error_if_collection_count_is_not_in_word_range()
        {
            var sut = new StringTextValidator(minWords: 2000, maxWords: 5000);

            await sut.ValidateAsync(CreateSentence(1), errors);

            errors.Should().BeEquivalentTo(
                new[] { "Must have between 2000 and 5000 word(s)." });
        }

        private static string CreateString(int size)
        {
            var sb = new StringBuilder();

            for (var i = 0; i < size; i++)
            {
                sb.Append('x');
            }

            return sb.ToString();
        }

        private static string CreateSentence(int size)
        {
            var sb = new StringBuilder();

            for (var i = 0; i < size; i++)
            {
                sb.Append('x');
                sb.Append(' ');
            }

            return sb.ToString();
        }
    }
}
