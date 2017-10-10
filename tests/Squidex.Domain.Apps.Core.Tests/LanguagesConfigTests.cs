// ==========================================================================
//  LanguagesConfigTests.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System.Collections;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using Squidex.Infrastructure;
using Xunit;

namespace Squidex.Domain.Apps.Core
{
    public class LanguagesConfigTests
    {
        [Fact]
        public void Should_create_initial_config()
        {
            var config = LanguagesConfig.Create(Language.DE);

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
            var config = LanguagesConfig.Create(Language.DE, Language.EN, Language.IT);

            config.OfType<LanguageConfig>().ToList().ShouldBeEquivalentTo(
                new List<LanguageConfig>
                {
                    new LanguageConfig(Language.DE),
                    new LanguageConfig(Language.EN),
                    new LanguageConfig(Language.IT)
                });

            Assert.Equal(Language.DE, config.Master.Language);
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
            var config = LanguagesConfig.Create(configs);

            config.OfType<LanguageConfig>().ToList().ShouldBeEquivalentTo(configs);

            Assert.Equal(configs[0], config.Master);
        }

        [Fact]
        public void Should_add_language()
        {
            var config = LanguagesConfig.Create(Language.DE).Add(Language.IT);

            config.OfType<LanguageConfig>().ToList().ShouldBeEquivalentTo(
                new List<LanguageConfig>
                {
                    new LanguageConfig(Language.DE),
                    new LanguageConfig(Language.IT)
                });
        }

        [Fact]
        public void Should_make_first_language_to_master()
        {
            var config = LanguagesConfig.Empty.Add(Language.IT);

            Assert.Equal(Language.IT, config.Master.Language);
        }

        [Fact]
        public void Should_throw_exception_if_language_to_add_already_exists()
        {
            var config = LanguagesConfig.Create(Language.DE);

            Assert.Throws<ValidationException>(() => config.Add(Language.DE));
        }

        [Fact]
        public void Should_make_master_language()
        {
            var config = LanguagesConfig.Create(Language.DE).Add(Language.IT).MakeMaster(Language.IT);

            Assert.Equal(Language.IT, config.Master.Language);
        }

        [Fact]
        public void Should_throw_exception_if_language_to_make_master_is_not_found()
        {
            var config = LanguagesConfig.Create(Language.DE);

            Assert.Throws<DomainObjectNotFoundException>(() => config.MakeMaster(Language.EN));
        }

        [Fact]
        public void Should_not_throw_exception_if_language_is_already_master_language()
        {
            var config = LanguagesConfig.Create(Language.DE);

            config.MakeMaster(Language.DE);
        }

        [Fact]
        public void Should_remove_language()
        {
            var config = LanguagesConfig.Create(Language.DE).Add(Language.IT).Add(Language.RU).Remove(Language.IT);

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
                LanguagesConfig.Create(Language.DE)
                    .Add(Language.IT)
                    .Add(Language.RU)
                    .Update(Language.DE, false, false, new[] { Language.RU, Language.IT })
                    .Update(Language.RU, false, false, new[] { Language.DE, Language.IT })
                    .Remove(Language.IT);

            config.OfType<LanguageConfig>().ToList().ShouldBeEquivalentTo(
                new List<LanguageConfig>
                {
                    new LanguageConfig(Language.DE, false, Language.RU),
                    new LanguageConfig(Language.RU, false, Language.DE)
                });
        }

        [Fact]
        public void Should_throw_exception_if_language_to_remove_is_not_found()
        {
            var config = LanguagesConfig.Create(Language.DE);

            Assert.Throws<DomainObjectNotFoundException>(() => config.Remove(Language.EN));
        }

        [Fact]
        public void Should_throw_exception_if_language_to_remove_is_master_language()
        {
            var config = LanguagesConfig.Create(Language.DE);

            Assert.Throws<ValidationException>(() => config.Remove(Language.DE));
        }

        [Fact]
        public void Should_update_language()
        {
            var config = LanguagesConfig.Create(Language.DE).Add(Language.IT).Update(Language.IT, true, false, new[] { Language.DE });

            config.OfType<LanguageConfig>().ToList().ShouldBeEquivalentTo(
                new List<LanguageConfig>
                {
                    new LanguageConfig(Language.DE),
                    new LanguageConfig(Language.IT, true, Language.DE)
                });
        }

        [Fact]
        public void Should_also_set_make_master_when_updating_language()
        {
            var config = LanguagesConfig.Create(Language.DE).Add(Language.IT).Update(Language.IT, false, true, null);

            Assert.Equal(Language.IT, config.Master.Language);
        }

        [Fact]
        public void Should_throw_exception_if_language_to_update_is_not_found()
        {
            var config = LanguagesConfig.Create(Language.DE);

            Assert.Throws<DomainObjectNotFoundException>(() => config.Update(Language.EN, true, false, null));
        }

        [Fact]
        public void Should_throw_exception_if_fallback_language_is_invalid()
        {
            var config = LanguagesConfig.Create(Language.DE);

            Assert.Throws<ValidationException>(() => config.Update(Language.DE, false, false, new[] { Language.EN }));
        }

        [Fact]
        public void Should_throw_exception_if_language_to_make_optional_is_master_language()
        {
            var config = LanguagesConfig.Create(Language.DE);

            Assert.Throws<ValidationException>(() => config.Update(Language.DE, true, false, null));
        }

        [Fact]
        public void Should_throw_exception_if_language_to_make_optional_must_be_set_to_master()
        {
            var config = LanguagesConfig.Create(Language.DE).Add(Language.IT);

            Assert.Throws<ValidationException>(() => config.Update(Language.DE, true, true, null));
        }

        [Fact]
        public void Should_provide_enumerators()
        {
            var config = LanguagesConfig.Create();

            Assert.Empty(config);

            Assert.NotNull(((IEnumerable)config).GetEnumerator());
            Assert.NotNull(((IEnumerable<IFieldPartitionItem>)config).GetEnumerator());
        }
    }
}
