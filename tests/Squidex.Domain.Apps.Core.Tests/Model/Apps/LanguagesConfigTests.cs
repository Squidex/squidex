// ==========================================================================
//  LanguagesConfigTests.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using Squidex.Domain.Apps.Core.Apps;
using Squidex.Infrastructure;
using Xunit;

namespace Squidex.Domain.Apps.Core.Model.Apps
{
    public class LanguagesConfigTests
    {
        [Fact]
        public void Should_create_initial_config()
        {
            var config = LanguagesConfig.Build(Language.DE);

            config.OfType<LanguageConfig>().ToList().ShouldBeEquivalentTo(
                new List<LanguageConfig>
                {
                    new LanguageConfig(Language.DE)
                });

            Assert.Equal(Language.DE, config.Master.Language);

            Assert.Equal("de", ((IFieldPartitioning)config).Master.Key);
        }

        [Fact]
        public void Should_create_initial_config_with_multiple_languages()
        {
            var config = LanguagesConfig.Build(Language.DE, Language.EN, Language.IT);

            config.OfType<LanguageConfig>().ToList().ShouldBeEquivalentTo(
                new List<LanguageConfig>
                {
                    new LanguageConfig(Language.DE),
                    new LanguageConfig(Language.EN),
                    new LanguageConfig(Language.IT)
                });

            Assert.Equal(Language.DE, config.Master.Language);
            Assert.Equal(3, config.Count);
        }

        [Fact]
        public void Should_create_initial_config_with_configs()
        {
            var configs = new[]
            {
                new LanguageConfig(Language.DE),
                new LanguageConfig(Language.EN),
                new LanguageConfig(Language.IT)
            };
            var config = LanguagesConfig.Build(configs);

            config.OfType<LanguageConfig>().ToList().ShouldBeEquivalentTo(configs);

            Assert.Equal(configs[0], config.Master);
        }

        [Fact]
        public void Should_add_language()
        {
            var config = LanguagesConfig.Build(Language.DE);

            config.Set(new LanguageConfig(Language.IT));

            config.OfType<LanguageConfig>().ToList().ShouldBeEquivalentTo(
                new List<LanguageConfig>
                {
                    new LanguageConfig(Language.DE),
                    new LanguageConfig(Language.IT)
                });

            Assert.True(config.TryGetConfig(Language.IT, out var _));
            Assert.True(config.Contains(Language.IT));
        }

        [Fact]
        public void Should_make_first_language_to_master()
        {
            var config = LanguagesConfig.Build(Language.IT);

            Assert.Equal(Language.IT, config.Master.Language);
        }

        [Fact]
        public void Should_not_throw_exception_if_language_to_add_already_exists()
        {
            var config = LanguagesConfig.Build(Language.DE);

            config.Set(new LanguageConfig(Language.DE));
        }

        [Fact]
        public void Should_make_master_language()
        {
            var config = LanguagesConfig.Build(Language.DE);

            config.Set(new LanguageConfig(Language.UK));
            config.Set(new LanguageConfig(Language.IT));
            config.MakeMaster(Language.IT);

            Assert.Equal(Language.IT, config.Master.Language);
        }

        [Fact]
        public void Should_throw_exception_if_language_to_make_master_is_not_found()
        {
            var config = LanguagesConfig.Build(Language.DE);

            Assert.Throws<KeyNotFoundException>(() => config.MakeMaster(Language.EN));
        }

        [Fact]
        public void Should_not_throw_exception_if_language_is_already_master_language()
        {
            var config = LanguagesConfig.Build(Language.DE);

            config.MakeMaster(Language.DE);
        }

        [Fact]
        public void Should_remove_language()
        {
            var config = LanguagesConfig.Build(Language.DE, Language.IT, Language.RU);

            config.Remove(Language.IT);

            config.ToList().ShouldBeEquivalentTo(
                new List<LanguageConfig>
                {
                    new LanguageConfig(Language.DE),
                    new LanguageConfig(Language.RU)
                });
        }

        [Fact]
        public void Should_remove_fallbacks_when_removing_language()
        {
            var config =
                LanguagesConfig.Build(
                    new LanguageConfig(Language.DE),
                    new LanguageConfig(Language.IT, false, Language.RU, Language.IT),
                    new LanguageConfig(Language.RU, false, Language.DE, Language.IT));

            config.Remove(Language.IT);

            config.OfType<LanguageConfig>().ToList().ShouldBeEquivalentTo(
                new List<LanguageConfig>
                {
                    new LanguageConfig(Language.DE),
                    new LanguageConfig(Language.RU, false, Language.DE)
                });
        }

        [Fact]
        public void Should_not_throw_exception_if_language_to_remove_is_not_found()
        {
            var config = LanguagesConfig.Build(Language.DE);

            config.Remove(Language.EN);
        }

        [Fact]
        public void Should_update_language()
        {
            var config = LanguagesConfig.Build(Language.DE);

            config.Set(new LanguageConfig(Language.IT));
            config.Set(new LanguageConfig(Language.IT, true, Language.DE));

            config.OfType<LanguageConfig>().ToList().ShouldBeEquivalentTo(
                new List<LanguageConfig>
                {
                    new LanguageConfig(Language.DE),
                    new LanguageConfig(Language.IT, true, Language.DE)
                });
        }

        [Fact]
        public void Should_throw_exception_if_fallback_language_is_invalid()
        {
            var config = LanguagesConfig.Build(Language.DE);

            Assert.Throws<InvalidOperationException>(() => config.Set(new LanguageConfig(Language.DE, false, Language.EN)));
        }

        [Fact]
        public void Should_provide_enumerators()
        {
            var config = LanguagesConfig.Build(Language.DE);

            Assert.NotEmpty(config);

            Assert.NotNull(((IEnumerable)config).GetEnumerator());
            Assert.NotNull(((IEnumerable<IFieldPartitionItem>)config).GetEnumerator());
        }
    }
}
