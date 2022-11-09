// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Domain.Apps.Core.Apps;
using Squidex.Domain.Apps.Entities.Apps.Commands;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Translations;
using Squidex.Infrastructure.Validation;

namespace Squidex.Domain.Apps.Entities.Apps.DomainObject.Guards;

public static class GuardAppLanguages
{
    public static void CanAdd(AddLanguage command, IAppEntity app)
    {
        Guard.NotNull(command);

        Validate.It(e =>
        {
            var languages = app.Languages;
            var language = command.Language;

            if (language == null)
            {
                e(Not.Defined(nameof(command.Language)), nameof(command.Language));
            }
            else if (app.Languages.Contains(command.Language))
            {
                e(T.Get("apps.languages.languageAlreadyAdded"));
            }
        });
    }

    public static void CanRemove(RemoveLanguage command, IAppEntity app)
    {
        Guard.NotNull(command);

        Validate.It(e =>
        {
            var languages = app.Languages;
            var language = command.Language;

            if (language == null)
            {
                e(Not.Defined(nameof(command.Language)), nameof(command.Language));
            }
            else
            {
                CheckLanguageExists(languages, language);

                if (languages.IsMaster(language))
                {
                    e(T.Get("apps.languages.masterLanguageNotRemovable"));
                }
            }
        });
    }

    public static void CanUpdate(UpdateLanguage command, IAppEntity app)
    {
        Guard.NotNull(command);

        Validate.It(e =>
        {
            var languages = app.Languages;
            var language = command.Language;

            if (language == null)
            {
                e(Not.Defined(nameof(command.Language)), nameof(command.Language));
            }
            else
            {
                CheckLanguageExists(languages, language);

                if (languages.IsMaster(language) || command.IsMaster)
                {
                    if (command.IsOptional)
                    {
                        e(T.Get("apps.languages.masterLanguageNotOptional"), nameof(command.IsMaster));
                    }

                    if (command.Fallback?.Length > 0)
                    {
                        e(T.Get("apps.languages.masterLanguageNoFallbacks"), nameof(command.Fallback));
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
                        e(T.Get("apps.languages.fallbackNotFound", new { fallback }), nameof(command.Fallback));
                    }
                }
            }
        });
    }

    private static void CheckLanguageExists(LanguagesConfig languages, Language language)
    {
        if (!languages.Contains(language))
        {
            throw new DomainObjectNotFoundException(language);
        }
    }
}
