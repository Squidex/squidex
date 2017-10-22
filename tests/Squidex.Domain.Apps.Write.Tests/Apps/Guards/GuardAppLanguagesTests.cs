// ==========================================================================
//  GuardAppLanguagesTests.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System.Collections.Generic;
using Squidex.Domain.Apps.Core;
using Squidex.Domain.Apps.Write.Apps.Commands;
using Squidex.Infrastructure;
using Xunit;

namespace Squidex.Domain.Apps.Write.Apps.Guards
{
    public class GuardAppLanguagesTests
    {
        [Fact]
        public void CanAddLanguage_should_throw_exception_if_language_is_null()
        {
            var command = new AddLanguage();

            var languages = LanguagesConfig.Build(Language.DE);

            Assert.Throws<ValidationException>(() => GuardAppLanguages.CanAdd(languages, command));
        }

        [Fact]
        public void CanAddLanguage_should_throw_exception_if_language_already_added()
        {
            var command = new AddLanguage { Language = Language.DE };

            var languages = LanguagesConfig.Build(Language.DE);

            Assert.Throws<ValidationException>(() => GuardAppLanguages.CanAdd(languages, command));
        }

        [Fact]
        public void CanAddLanguage_should_not_throw_exception_if_language_valid()
        {
            var command = new AddLanguage { Language = Language.EN };

            var languages = LanguagesConfig.Build(Language.DE);

            GuardAppLanguages.CanAdd(languages, command);
        }

        [Fact]
        public void CanRemoveLanguage_should_throw_exception_if_language_is_null()
        {
            var command = new RemoveLanguage();

            var languages = LanguagesConfig.Build(Language.DE);

            Assert.Throws<DomainObjectNotFoundException>(() => GuardAppLanguages.CanRemove(languages, command));
        }

        [Fact]
        public void CanRemoveLanguage_should_throw_exception_if_language_not_found()
        {
            var command = new RemoveLanguage { Language = Language.EN };

            var languages = LanguagesConfig.Build(Language.DE);

            Assert.Throws<DomainObjectNotFoundException>(() => GuardAppLanguages.CanRemove(languages, command));
        }

        [Fact]
        public void CanRemoveLanguage_should_throw_exception_if_language_is_master()
        {
            var command = new RemoveLanguage { Language = Language.DE };

            var languages = LanguagesConfig.Build(Language.DE);

            Assert.Throws<ValidationException>(() => GuardAppLanguages.CanRemove(languages, command));
        }

        [Fact]
        public void CanRemoveLanguage_should_not_throw_exception_if_language_is_valid()
        {
            var command = new RemoveLanguage { Language = Language.EN };

            var languages = LanguagesConfig.Build(Language.DE, Language.EN);

            GuardAppLanguages.CanRemove(languages, command);
        }

        [Fact]
        public void CanUpdateLanguage_should_throw_exception_if_language_is_optional_and_master()
        {
            var command = new UpdateLanguage { Language = Language.DE, IsOptional = true };

            var languages = LanguagesConfig.Build(Language.DE, Language.EN);

            Assert.Throws<ValidationException>(() => GuardAppLanguages.CanUpdate(languages, command));
        }

        [Fact]
        public void CanUpdateLanguage_should_throw_exception_if_language_has_invalid_fallback()
        {
            var command = new UpdateLanguage { Language = Language.DE, Fallback = new List<Language> { Language.IT } };

            var languages = LanguagesConfig.Build(Language.DE, Language.EN);

            Assert.Throws<ValidationException>(() => GuardAppLanguages.CanUpdate(languages, command));
        }

        [Fact]
        public void CanUpdateLanguage_should_throw_exception_if_not_found()
        {
            var command = new UpdateLanguage { Language = Language.IT };

            var languages = LanguagesConfig.Build(Language.DE, Language.EN);

            Assert.Throws<DomainObjectNotFoundException>(() => GuardAppLanguages.CanUpdate(languages, command));
        }

        [Fact]
        public void CanUpdateLanguage_should_not_throw_exception_if_language_is_valid()
        {
            var command = new UpdateLanguage { Language = Language.DE, Fallback = new List<Language> { Language.EN } };

            var languages = LanguagesConfig.Build(Language.DE, Language.EN);

            GuardAppLanguages.CanUpdate(languages, command);
        }
    }
}
