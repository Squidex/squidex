// ==========================================================================
//  GuardAppLanguagesTests.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System.Collections.Generic;
using Squidex.Domain.Apps.Core.Apps;
using Squidex.Domain.Apps.Write.Apps.Commands;
using Squidex.Infrastructure;
using Xunit;

namespace Squidex.Domain.Apps.Write.Apps.Guards
{
    public class GuardAppLanguagesTests
    {
        private readonly LanguagesConfig languages = LanguagesConfig.Build(Language.DE);

        [Fact]
        public void CanAddLanguage_should_throw_exception_if_language_is_null()
        {
            var command = new AddLanguage();

            Assert.Throws<ValidationException>(() => GuardAppLanguages.CanAdd(languages, command));
        }

        [Fact]
        public void CanAddLanguage_should_throw_exception_if_language_already_added()
        {
            var command = new AddLanguage { Language = Language.DE };

            Assert.Throws<ValidationException>(() => GuardAppLanguages.CanAdd(languages, command));
        }

        [Fact]
        public void CanAddLanguage_should_not_throw_exception_if_language_valid()
        {
            var command = new AddLanguage { Language = Language.EN };

            GuardAppLanguages.CanAdd(languages, command);
        }

        [Fact]
        public void CanRemoveLanguage_should_throw_exception_if_language_is_null()
        {
            var command = new RemoveLanguage();

            Assert.Throws<ValidationException>(() => GuardAppLanguages.CanRemove(languages, command));
        }

        [Fact]
        public void CanRemoveLanguage_should_throw_exception_if_language_not_found()
        {
            var command = new RemoveLanguage { Language = Language.EN };

            Assert.Throws<DomainObjectNotFoundException>(() => GuardAppLanguages.CanRemove(languages, command));
        }

        [Fact]
        public void CanRemoveLanguage_should_throw_exception_if_language_is_master()
        {
            var command = new RemoveLanguage { Language = Language.DE };

            Assert.Throws<ValidationException>(() => GuardAppLanguages.CanRemove(languages, command));
        }

        [Fact]
        public void CanRemoveLanguage_should_not_throw_exception_if_language_is_valid()
        {
            var command = new RemoveLanguage { Language = Language.EN };

            languages.Set(new LanguageConfig(Language.EN));

            GuardAppLanguages.CanRemove(languages, command);
        }

        [Fact]
        public void CanUpdateLanguage_should_throw_exception_if_language_is_null()
        {
            var command = new UpdateLanguage();

            languages.Set(new LanguageConfig(Language.EN));

            Assert.Throws<ValidationException>(() => GuardAppLanguages.CanUpdate(languages, command));
        }

        [Fact]
        public void CanUpdateLanguage_should_throw_exception_if_language_is_optional_and_master()
        {
            var command = new UpdateLanguage { Language = Language.DE, IsOptional = true };

            languages.Set(new LanguageConfig(Language.EN));

            Assert.Throws<ValidationException>(() => GuardAppLanguages.CanUpdate(languages, command));
        }

        [Fact]
        public void CanUpdateLanguage_should_throw_exception_if_language_has_invalid_fallback()
        {
            var command = new UpdateLanguage { Language = Language.DE, Fallback = new List<Language> { Language.IT } };

            languages.Set(new LanguageConfig(Language.EN));

            Assert.Throws<ValidationException>(() => GuardAppLanguages.CanUpdate(languages, command));
        }

        [Fact]
        public void CanUpdateLanguage_should_throw_exception_if_not_found()
        {
            var command = new UpdateLanguage { Language = Language.IT };

            languages.Set(new LanguageConfig(Language.EN));

            Assert.Throws<DomainObjectNotFoundException>(() => GuardAppLanguages.CanUpdate(languages, command));
        }

        [Fact]
        public void CanUpdateLanguage_should_not_throw_exception_if_language_is_valid()
        {
            var command = new UpdateLanguage { Language = Language.DE, Fallback = new List<Language> { Language.EN } };

            languages.Set(new LanguageConfig(Language.EN));

            GuardAppLanguages.CanUpdate(languages, command);
        }
    }
}
