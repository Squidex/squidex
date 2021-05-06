// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Collections.Generic;
using FluentAssertions;
using Squidex.Domain.Apps.Core.Apps;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Collections;
using Xunit;

#pragma warning disable SA1310 // Field names must not contain underscore
#pragma warning disable CA1806 // Do not ignore method results

namespace Squidex.Domain.Apps.Core.Model.Apps
{
    public class LanguagesConfigTests
    {
        private readonly LanguagesConfig config_0 = LanguagesConfig.English;

        [Fact]
        public void Should_make_contains_test()
        {
            Assert.True(config_0.Contains(Language.EN));

            Assert.False(config_0.Contains(Language.FR));
            Assert.False(config_0.Contains(null!));
        }

        [Fact]
        public void Should_provide_name()
        {
            Assert.Equal("English", config_0.GetName(Language.EN));

            Assert.Null(config_0.GetName(Language.FR));
            Assert.Null(config_0.GetName(null!));
        }

        [Fact]
        public void Should_make_master_test()
        {
            var config =
                LanguagesConfig.English
                    .Set(Language.DE)
                    .Set(Language.ES, true);

            Assert.True(config.IsMaster(Language.EN));

            Assert.False(config.IsMaster(Language.DE));
            Assert.False(config.IsMaster(Language.ES));
            Assert.False(config.IsMaster(Language.FR));
            Assert.False(config.IsMaster(null!));
        }

        [Fact]
        public void Should_make_optional_test()
        {
            var config =
                LanguagesConfig.English
                    .Set(Language.DE)
                    .Set(Language.ES, true);

            Assert.True(config.IsOptional(Language.ES));

            Assert.False(config.IsOptional(Language.EN));
            Assert.False(config.IsOptional(Language.DE));
            Assert.False(config.IsOptional(Language.FR));
            Assert.False(config.IsOptional(null!));
        }

        [Fact]
        public void Should_provide_priorities()
        {
            var config =
                LanguagesConfig.English
                    .Set(Language.DE)
                    .Set(Language.ES, true, Language.DE);

            Assert.Empty(config.GetPriorities(Language.FR));
            Assert.Empty(config.GetPriorities(null!));

            Assert.Equal(new string[] { Language.ES, Language.DE, Language.EN }, config.GetPriorities(Language.ES));
            Assert.Equal(new string[] { Language.DE, Language.EN }, config.GetPriorities(Language.DE));
            Assert.Equal(new string[] { Language.EN }, config.GetPriorities(Language.EN));
        }

        [Fact]
        public void Should_create_initial_config()
        {
            config_0.Languages.Should().BeEquivalentTo(
                new Dictionary<string, LanguageConfig>
                {
                    [Language.EN] = new LanguageConfig()
                });

            Assert.Equal(Language.EN, config_0.Master);
        }

        [Fact]
        public void Should_create_initial_config_0_with_multiple_languages()
        {
            var config =
                LanguagesConfig.English
                    .Set(Language.DE)
                    .Set(Language.ES, true)
                    .Set(Language.IT, true, Language.ES)
                    .MakeMaster(Language.DE);

            config.Languages.Should().BeEquivalentTo(
                new Dictionary<string, LanguageConfig>
                {
                    [Language.EN] = new LanguageConfig(),
                    [Language.DE] = new LanguageConfig(),
                    [Language.ES] = new LanguageConfig(true),
                    [Language.IT] = new LanguageConfig(true, ImmutableList.Create(Language.ES))
                });

            Assert.Equal(Language.DE, config.Master);
        }

        [Fact]
        public void Should_not_throw_exception_if_language_to_add_already_exists()
        {
            config_0.Set(Language.EN);
        }

        [Fact]
        public void Should_return_same_language_if_already_added()
        {
            var config_1 = config_0.Set(Language.EN);

            Assert.Same(config_1, config_0);
        }

        [Fact]
        public void Should_make_master_language()
        {
            var config =
                LanguagesConfig.English
                    .Set(Language.DE)
                    .Set(Language.IT, true, Language.ES)
                    .MakeMaster(Language.IT);

            config.Languages.Should().BeEquivalentTo(
                new Dictionary<string, LanguageConfig>
                {
                    [Language.EN] = new LanguageConfig(),
                    [Language.DE] = new LanguageConfig(),
                    [Language.IT] = new LanguageConfig()
                });

            Assert.Equal(Language.IT, config.Master);
        }

        [Fact]
        public void Should_return_same_languages_if_master_language_is_already_master()
        {
            var config_1 = config_0.Set(Language.DE);
            var config_2 = config_1.Set(Language.IT);
            var config_3 = config_2.MakeMaster(Language.IT);
            var config_4 = config_3.MakeMaster(Language.IT);

            Assert.Same(config_3, config_4);
        }

        [Fact]
        public void Should_keep_master_language_if_language_to_make_master_is_not_found()
        {
            var config_1 = config_0.Set(Language.DE);
            var config_2 = config_1.Set(Language.IT);
            var config_3 = config_2.MakeMaster(Language.IT);
            var config_4 = config_3.MakeMaster(Language.FR);

            Assert.Same(config_3, config_4);
            Assert.Equal(Language.IT, config_4.Master);
        }

        [Fact]
        public void Should_remove_language()
        {
            var config_1 = config_0.Set(Language.DE);
            var config_2 = config_1.Set(Language.IT);
            var config_3 = config_2.Remove(Language.DE);

            config_3.Languages.Should().BeEquivalentTo(
                new Dictionary<string, LanguageConfig>
                {
                    [Language.EN] = new LanguageConfig(),
                    [Language.IT] = new LanguageConfig()
                });

            Assert.Equal(Language.EN, config_3.Master);
        }

        [Fact]
        public void Should_remove_fallbacks_if_removing_language()
        {
            var config_1 = config_0.Set(Language.DE);
            var config_2 = config_1.Set(Language.IT, true, Language.UK);
            var config_3 = config_2.Remove(Language.DE);

            config_3.Languages.Should().BeEquivalentTo(
                new Dictionary<string, LanguageConfig>
                {
                    [Language.EN] = new LanguageConfig(),
                    [Language.IT] = new LanguageConfig(true)
                });

            Assert.Equal(Language.EN, config_3.Master);
        }

        [Fact]
        public void Should_same_languages_if_removing_single_language()
        {
            var config_1 = config_0.Remove(Language.EN);

            Assert.Same(config_0, config_1);
        }

        [Fact]
        public void Should_update_master_language_if_removed()
        {
            var config_1 = config_0.Set(Language.DE);
            var config_2 = config_1.Set(Language.IT);
            var config_3 = config_2.Remove(Language.EN);

            config_3.Languages.Should().BeEquivalentTo(
                new Dictionary<string, LanguageConfig>
                {
                    [Language.DE] = new LanguageConfig(),
                    [Language.IT] = new LanguageConfig()
                });

            Assert.Equal(Language.DE, config_3.Master);
        }

        [Fact]
        public void Should_return_same_languages_if_language_to_remove_is_not_found()
        {
            var config_1 = config_0.Remove(Language.IT);

            Assert.Equal(config_0, config_1);
        }

        [Fact]
        public void Should_update_language()
        {
            var config_1 = config_0.Set(Language.IT);
            var config_2 = config_1.Set(Language.IT, true, Language.EN);

            config_2.Languages.Should().BeEquivalentTo(
                new Dictionary<string, LanguageConfig>
                {
                    [Language.EN] = new LanguageConfig(),
                    [Language.IT] = new LanguageConfig(true, ImmutableList.Create(Language.EN))
                });

            Assert.Equal(Language.EN, config_2.Master);
        }

        [Fact]
        public void Should_eliminate_invalid_fallbacks_and_self()
        {
            var config_1 = config_0.Set(Language.IT);
            var config_2 = config_1.Set(Language.IT);
            var config_3 = config_2.Set(Language.IT, true, Language.EN, Language.IT, Language.DE);

            config_3.Languages.Should().BeEquivalentTo(
                new Dictionary<string, LanguageConfig>
                {
                    [Language.EN] = new LanguageConfig(),
                    [Language.IT] = new LanguageConfig(true, ImmutableList.Create(Language.EN))
                });

            Assert.Equal(Language.EN, config_2.Master);
        }
    }
}
