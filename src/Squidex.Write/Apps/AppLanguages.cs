// ==========================================================================
//  AppLanguages.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System;
using System.Collections.Generic;
using Squidex.Infrastructure;

// ReSharper disable InvertIf

namespace Squidex.Write.Apps
{
    public class AppLanguages
    {
        private readonly HashSet<Language> languages = new HashSet<Language>();
        private Language masterLanguage;

        public void Add(Language language)
        {
            Func<string> message = () => "Cannot add language";

            ThrowIfFound(language, message);

            languages.Add(language);
        }

        public void Remove(Language language)
        {
            Func<string> message = () => "Cannot remove language";

            ThrowIfNotFound(language);
            ThrowIfMasterLanguage(language, message);

            languages.Remove(language);
        }

        public void SetMasterLanguage(Language language)
        {
            Func<string> message = () => "Cannot set master language";

            ThrowIfNotFound(language);
            ThrowIfMasterLanguage(language, message);

            masterLanguage = language;
        }

        private void ThrowIfNotFound(Language language)
        {
            if (!languages.Contains(language))
            {
                throw new DomainObjectNotFoundException(language.Iso2Code, "Languages", typeof(AppDomainObject));
            }
        }

        private void ThrowIfFound(Language language, Func<string> message)
        {
            if (languages.Contains(language))
            {
                var error = new ValidationError("Language is already part of the app", "Language");

                throw new ValidationException(message(), error);
            }
        }

        private void ThrowIfMasterLanguage(Language language, Func<string> message)
        {
            if (masterLanguage != null && masterLanguage.Equals(language))
            {
                var error = new ValidationError("Language is the master language", "Language");

                throw new ValidationException(message(), error);
            }
        }
    }
}
