﻿// ==========================================================================
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
                if (command.Language == null)
                {
                    e(Not.Defined("Language code"), nameof(command.Language));
                }
                else if (languages.Contains(command.Language))
                {
                    e("Language has already been added.");
                }
            });
        }

        public static void CanRemove(LanguagesConfig languages, RemoveLanguage command)
        {
            Guard.NotNull(command);

            var config = GetConfigOrThrow(languages, command.Language);

            Validate.It(() => "Cannot remove language.", e =>
            {
                if (command.Language == null)
                {
                    e(Not.Defined("Language code"), nameof(command.Language));
                }

                if (languages.Master == config)
                {
                    e("Master language cannot be removed.");
                }
            });
        }

        public static void CanUpdate(LanguagesConfig languages, UpdateLanguage command)
        {
            Guard.NotNull(command);

            var config = GetConfigOrThrow(languages, command.Language);

            Validate.It(() => "Cannot update language.", e =>
            {
                if (command.Language == null)
                {
                    e(Not.Defined("Language code"), nameof(command.Language));
                }

                if ((languages.Master == config || command.IsMaster) && command.IsOptional)
                {
                    e("Master language cannot be made optional.", nameof(command.IsMaster));
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
            });
        }

        private static LanguageConfig? GetConfigOrThrow(LanguagesConfig languages, Language language)
        {
            if (language == null)
            {
                return null;
            }

            if (!languages.TryGetConfig(language, out var languageConfig))
            {
                throw new DomainObjectNotFoundException(language, "Languages", typeof(IAppEntity));
            }

            return languageConfig;
        }
    }
}
