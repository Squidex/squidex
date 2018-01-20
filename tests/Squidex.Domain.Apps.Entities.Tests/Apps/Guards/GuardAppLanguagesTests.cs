// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Collections.Generic;
using Squidex.Domain.Apps.Core.Apps;
using Squidex.Domain.Apps.Entities.Apps.Commands;
using Squidex.Infrastructure;
using Xunit;

#pragma warning disable SA1310 // Field names must not contain underscore

namespace Squidex.Domain.Apps.Entities.Apps.Guards
{
    public class GuardAppLanguagesTests
    {
        private readonly LanguagesConfig languages_0 = LanguagesConfig.Build(Language.DE);

        [Fact]
        public void CanAddLanguage_should_throw_exception_if_language_is_null()
        {
            var command = new AddLanguage();

            Assert.Throws<ValidationException>(() => GuardAppLanguages.CanAdd(languages_0, command));
        }

        [Fact]
        public void CanAddLanguage_should_throw_exception_if_language_already_added()
        {
            var command = new AddLanguage { Language = Language.DE };

            Assert.Throws<ValidationException>(() => GuardAppLanguages.CanAdd(languages_0, command));
        }

        [Fact]
        public void CanAddLanguage_should_not_throw_exception_if_language_valid()
        {
            var command = new AddLanguage { Language = Language.EN };

            GuardAppLanguages.CanAdd(languages_0, command);
        }

        [Fact]
        public void CanRemoveLanguage_should_throw_exception_if_language_is_null()
        {
            var command = new RemoveLanguage();

            Assert.Throws<ValidationException>(() => GuardAppLanguages.CanRemove(languages_0, command));
        }

        [Fact]
        public void CanRemoveLanguage_should_throw_exception_if_language_not_found()
        {
            var command = new RemoveLanguage { Language = Language.EN };

            Assert.Throws<DomainObjectNotFoundException>(() => GuardAppLanguages.CanRemove(languages_0, command));
        }

        [Fact]
        public void CanRemoveLanguage_should_throw_exception_if_language_is_master()
        {
            var command = new RemoveLanguage { Language = Language.DE };

            Assert.Throws<ValidationException>(() => GuardAppLanguages.CanRemove(languages_0, command));
        }

        [Fact]
        public void CanRemoveLanguage_should_not_throw_exception_if_language_is_valid()
        {
            var command = new RemoveLanguage { Language = Language.EN };

            var languages_1 = languages_0.Set(new LanguageConfig(Language.EN));

            GuardAppLanguages.CanRemove(languages_1, command);
        }

        [Fact]
        public void CanUpdateLanguage_should_throw_exception_if_language_is_null()
        {
            var command = new UpdateLanguage();

            var languages_1 = languages_0.Set(new LanguageConfig(Language.EN));

            Assert.Throws<ValidationException>(() => GuardAppLanguages.CanUpdate(languages_1, command));
        }

        [Fact]
        public void CanUpdateLanguage_should_throw_exception_if_language_is_optional_and_master()
        {
            var command = new UpdateLanguage { Language = Language.DE, IsOptional = true };

            var languages_1 = languages_0.Set(new LanguageConfig(Language.EN));

            Assert.Throws<ValidationException>(() => GuardAppLanguages.CanUpdate(languages_1, command));
        }

        [Fact]
        public void CanUpdateLanguage_should_throw_exception_if_language_has_invalid_fallback()
        {
            var command = new UpdateLanguage { Language = Language.DE, Fallback = new List<Language> { Language.IT } };

            var languages_1 = languages_0.Set(new LanguageConfig(Language.EN));

            Assert.Throws<ValidationException>(() => GuardAppLanguages.CanUpdate(languages_1, command));
        }

        [Fact]
        public void CanUpdateLanguage_should_throw_exception_if_not_found()
        {
            var command = new UpdateLanguage { Language = Language.IT };

            var languages_1 = languages_0.Set(new LanguageConfig(Language.EN));

            Assert.Throws<DomainObjectNotFoundException>(() => GuardAppLanguages.CanUpdate(languages_1, command));
        }

        [Fact]
        public void CanUpdateLanguage_should_not_throw_exception_if_language_is_valid()
        {
            var command = new UpdateLanguage { Language = Language.DE, Fallback = new List<Language> { Language.EN } };

            var languages_1 = languages_0.Set(new LanguageConfig(Language.EN));

            GuardAppLanguages.CanUpdate(languages_1, command);
        }
    }
}
