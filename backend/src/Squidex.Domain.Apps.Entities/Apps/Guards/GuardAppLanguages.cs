// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Domain.Apps.Core.Apps;
using Squidex.Domain.Apps.Entities.Apps.Commands;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Validation;

namespace Squidex.Domain.Apps.Entities.Apps.Guards
{
    public static class GuardAppLanguages
    {
        public static void CanAdd(LanguagesConfig languages, AddLanguage command)
        {
            Guard.NotNull(command);

            Validate.It(() => "Cannot add language.", e =>
            {
                var language = command.Language;

                if (language == null)
                {
                    e(Not.Defined("Language code"), nameof(command.Language));
                }
                else if (languages.Contains(language))
                {
                    e("Language has already been added.");
                }
            });
        }

        public static void CanRemove(LanguagesConfig languages, RemoveLanguage command)
        {
            Guard.NotNull(command);

            Validate.It(() => "Cannot remove language.", e =>
            {
                var language = command.Language;

                if (language == null)
                {
                    e(Not.Defined("Language code"), nameof(command.Language));
                }
                else
                {
                    EnsureConfigExists(languages, language);

                    if (languages.IsMaster(language))
                    {
                        e("Master language cannot be removed.");
                    }
                }
            });
        }

        public static void CanUpdate(LanguagesConfig languages, UpdateLanguage command)
        {
            Guard.NotNull(command);

            Validate.It(() => "Cannot update language.", e =>
            {
                var language = command.Language;

                if (language == null)
                {
                    e(Not.Defined("Language code"), nameof(command.Language));
                }
                else
                {
                    EnsureConfigExists(languages, language);

                    if (languages.IsMaster(language) || command.IsMaster)
                    {
                        if (command.IsOptional)
                        {
                            e("Master language cannot be made optional.", nameof(command.IsMaster));
                        }

                        if (command.Fallback?.Count > 0)
                        {
                            e("Master language cannot have fallback languages.", nameof(command.Fallback));
                        }
                    }

                    if (command.Fallback == null)
                    {
                        return;
                    }

                    foreach (var fallback in command.Fallback)
                    {
                        if (!languages.Contains(fallback))
                        {
                            e($"App does not have fallback language '{fallback}'.", nameof(command.Fallback));
                        }
                    }
                }
            });
        }

        private static void EnsureConfigExists(LanguagesConfig languages, Language language)
        {
            if (!languages.Contains(language))
            {
                throw new DomainObjectNotFoundException(language, "Languages", typeof(IAppEntity));
            }
        }
    }
}
