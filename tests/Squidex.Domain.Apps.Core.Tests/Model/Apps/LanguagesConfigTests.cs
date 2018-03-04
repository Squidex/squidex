// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
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
        public void Should_create_initial_config_0()
        {
            var config_0 = LanguagesConfig.Build(Language.DE);

            config_0.OfType<LanguageConfig>().ToList().ShouldBeEquivalentTo(
                new List<LanguageConfig>
                {
                    new LanguageConfig(Language.DE)
                });

            Assert.Equal(Language.DE, config_0.Master.Language);

            Assert.Equal("de", ((IFieldPartitioning)config_0).Master.Key);
        }

        [Fact]
        public void Should_create_initial_config_0_with_multiple_languages()
        {
            var config_0 = LanguagesConfig.Build(Language.DE, Language.EN, Language.IT);

            config_0.OfType<LanguageConfig>().ToList().ShouldBeEquivalentTo(
                new List<LanguageConfig>
                {
                    new LanguageConfig(Language.DE),
                    new LanguageConfig(Language.EN),
                    new LanguageConfig(Language.IT)
                });

            config_0.TryGetConfig(Language.DE, out var master);

            Assert.Equal(Language.DE, config_0.Master.Language);
            Assert.Equal(3, config_0.Count);

            Assert.Same(master, config_0.Master);
        }

        [Fact]
        public void Should_create_initial_config_0_with_config_0s()
        {
            var configs = new[]
            {
                new LanguageConfig(Language.DE),
                new LanguageConfig(Language.EN),
                new LanguageConfig(Language.IT)
            };
            var config_0 = LanguagesConfig.Build(configs);

            config_0.OfType<LanguageConfig>().ToList().ShouldBeEquivalentTo(configs);

            Assert.Equal(configs[0], config_0.Master);
            Assert.Same(configs[0], config_0.Master);
        }

        [Fact]
        public void Should_add_language()
        {
            var config_0 = LanguagesConfig.Build(Language.DE);
            var config_1 = config_0.Set(new LanguageConfig(Language.IT));

            config_1.OfType<LanguageConfig>().ToList().ShouldBeEquivalentTo(
                new List<LanguageConfig>
                {
                    new LanguageConfig(Language.DE),
                    new LanguageConfig(Language.IT)
                });

            Assert.True(config_1.TryGetConfig(Language.IT, out var _));
            Assert.True(config_1.Contains(Language.IT));
        }

        [Fact]
        public void Should_make_first_language_to_master()
        {
            var config_0 = LanguagesConfig.Build(Language.IT);

            Assert.Equal(Language.IT, config_0.Master.Language);
        }

        [Fact]
        public void Should_not_throw_exception_if_language_to_add_already_exists()
        {
            var config_0 = LanguagesConfig.Build(Language.DE);

            config_0.Set(new LanguageConfig(Language.DE));
        }

        [Fact]
        public void Should_make_master_language()
        {
            var config_0 = LanguagesConfig.Build(Language.DE);

            var config_1 = config_0.Set(new LanguageConfig(Language.UK));
            var config_2 = config_1.Set(new LanguageConfig(Language.IT));
            var config_3 = config_2.MakeMaster(Language.IT);

            Assert.Equal(Language.IT, config_3.Master.Language);
        }

        [Fact]
        public void Should_throw_exception_if_language_to_make_master_is_not_found()
        {
            var config_0 = LanguagesConfig.Build(Language.DE);

            Assert.Throws<KeyNotFoundException>(() => config_0.MakeMaster(Language.EN));
        }

        [Fact]
        public void Should_not_throw_exception_if_language_is_already_master_language()
        {
            var config_0 = LanguagesConfig.Build(Language.DE);

            config_0.MakeMaster(Language.DE);
        }

        [Fact]
        public void Should_remove_language()
        {
            var config_0 = LanguagesConfig.Build(Language.DE, Language.IT, Language.RU);
            var config_1 = config_0.Remove(Language.IT);

            Assert.Same(config_1.Master, config_1.OfType<LanguageConfig>().FirstOrDefault(x => x.Language == Language.DE));

            config_1.ToList().ShouldBeEquivalentTo(
                new List<LanguageConfig>
                {
                    new LanguageConfig(Language.DE),
                    new LanguageConfig(Language.RU)
                });
        }

        [Fact]
        public void Should_remove_fallbacks_when_removing_language()
        {
            var config_0 =
                LanguagesConfig.Build(
                    new LanguageConfig(Language.DE),
                    new LanguageConfig(Language.IT, false, Language.RU, Language.IT),
                    new LanguageConfig(Language.RU, false, Language.DE, Language.IT));
            var config_1 = config_0.Remove(Language.IT);

            config_1.OfType<LanguageConfig>().ToList().ShouldBeEquivalentTo(
                new List<LanguageConfig>
                {
                    new LanguageConfig(Language.DE),
                    new LanguageConfig(Language.RU, false, Language.DE)
                });
        }

        [Fact]
        public void Should_not_throw_exception_if_language_to_remove_is_not_found()
        {
            var config_0 = LanguagesConfig.Build(Language.DE);

            config_0.Remove(Language.EN);
        }

        [Fact]
        public void Should_throw_exception_if_language_to_remove_is_master()
        {
            var config_0 = LanguagesConfig.Build(Language.DE);

            Assert.Throws<InvalidOperationException>(() => config_0.Remove(Language.DE));
        }

        [Fact]
        public void Should_update_language()
        {
            var config_0 = LanguagesConfig.Build(Language.DE);
            var config_1 = config_0.Set(new LanguageConfig(Language.IT));
            var config_2 = config_1.Set(new LanguageConfig(Language.IT, true, Language.DE));

            config_2.OfType<LanguageConfig>().ToList().ShouldBeEquivalentTo(
                new List<LanguageConfig>
                {
                    new LanguageConfig(Language.DE),
                    new LanguageConfig(Language.IT, true, Language.DE)
                });
        }

        [Fact]
        public void Should_throw_exception_if_fallback_language_is_invalid()
        {
            var config_0 = LanguagesConfig.Build(Language.DE);

            Assert.Throws<InvalidOperationException>(() => config_0.Set(new LanguageConfig(Language.DE, false, Language.EN)));
        }

        [Fact]
        public void Should_provide_enumerators()
        {
            var config_0 = LanguagesConfig.Build(Language.DE);

            Assert.NotEmpty(config_0);

            Assert.NotNull(((IEnumerable)config_0).GetEnumerator());
            Assert.NotNull(((IEnumerable<IFieldPartitionItem>)config_0).GetEnumerator());
        }
    }
}
