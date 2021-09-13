// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using Xunit;

namespace Squidex.Infrastructure
{
    public sealed class LanguagesInitializerTests
    {
        [Fact]
        public async Task Should_add_custom_languages()
        {
            var options = Options.Create(new LanguagesOptions
            {
                ["en-NO"] = "English (Norwegian)"
            });

            var sut = new LanguagesInitializer(options);

            await sut.InitializeAsync(default);

            Assert.Equal("English (Norwegian)", Language.GetLanguage("en-NO").EnglishName);
        }

        [Fact]
        public async Task Should_not_add_invalid_languages()
        {
            var options = Options.Create(new LanguagesOptions
            {
                ["en-Error"] = null!
            });

            var sut = new LanguagesInitializer(options);

            await sut.InitializeAsync(default);

            Assert.False(Language.TryGetLanguage("en-Error", out _));
        }

        [Fact]
        public async Task Should_not_override_existing_languages()
        {
            var options = Options.Create(new LanguagesOptions
            {
                ["de"] = "German (Germany)"
            });

            var sut = new LanguagesInitializer(options);

            await sut.InitializeAsync(default);

            Assert.Equal("German", Language.GetLanguage("de").EnglishName);
        }
    }
}
