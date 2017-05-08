// ==========================================================================
//  LanguagesConfig.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Squidex.Infrastructure;

// ReSharper disable InvertIf

namespace Squidex.Core
{
    public sealed class LanguagesConfig : IEnumerable<LanguageConfig>
    {
        private readonly ImmutableDictionary<Language, LanguageConfig> languages;
        private readonly LanguageConfig master;

        public static readonly LanguagesConfig Empty = Create();
        public static readonly LanguagesConfig Invariant = Create(Language.Invariant);

        public LanguageConfig Master
        {
            get { return master; }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return languages.Values.GetEnumerator();
        }

        IEnumerator<LanguageConfig> IEnumerable<LanguageConfig>.GetEnumerator()
        {
            return languages.Values.GetEnumerator();
        }

        private LanguagesConfig(ImmutableDictionary<Language, LanguageConfig> languages, LanguageConfig master)
        {
            this.languages = ValidateLanguages(languages);

            this.master = master;
        }

        public static LanguagesConfig Create(ICollection<LanguageConfig> languageConfigs)
        {
            Guard.NotNull(languageConfigs, nameof(languageConfigs));

            var validated = ValidateLanguages(languageConfigs.ToImmutableDictionary(c => c.Language));

            return new LanguagesConfig(validated, languageConfigs.FirstOrDefault());
        }

        public static LanguagesConfig Create(params Language[] languages)
        {
            Guard.NotNull(languages, nameof(languages));

            var languageConfigs = languages.Select(l => new LanguageConfig(l)).ToList();

            return Create(languageConfigs);
        }

        public LanguagesConfig MakeMaster(Language language)
        {
            ThrowIfNotFound(language);

            return new LanguagesConfig(languages, languages[language]);
        }

        public LanguagesConfig Add(Language language)
        {
            ThrowIfFound(language, () => $"Cannot add language '{language.Iso2Code}'.");

            var newLanguages = languages.Add(language, new LanguageConfig(language));

            return new LanguagesConfig(newLanguages, master ?? newLanguages.Values.First());
        }

        public LanguagesConfig Update(Language language, bool isOptional, bool isMaster, IEnumerable<Language> fallback)
        {
            ThrowIfNotFound(language);

            if (isOptional)
            {
                ThrowIfMaster(language, isMaster, () => $"Cannot cannot make language '{language.Iso2Code}' optional");
            }

            var newLanguage = new LanguageConfig(language, isOptional, fallback);
            var newLanguages = ValidateLanguages(languages.SetItem(language, newLanguage));

            return new LanguagesConfig(newLanguages, isMaster ? newLanguage : master);
        }

        public LanguagesConfig Remove(Language language)
        {
            ThrowIfNotFound(language);
            ThrowIfMaster(language, false, () => $"Cannot remove language '{language.Iso2Code}'");

            var newLanguages = languages.Remove(language);

            foreach (var languageConfig in newLanguages.Values)
            {
                if (languageConfig.Fallback.Contains(language))
                {
                    newLanguages = 
                        newLanguages.SetItem(languageConfig.Language,
                            new LanguageConfig(
                                languageConfig.Language,
                                languageConfig.IsOptional,
                                languageConfig.Fallback.Remove(language)));
                }
            }

            return new LanguagesConfig(newLanguages, master);
        }

        public bool TryGetConfig(Language language, out LanguageConfig value)
        {
            return languages.TryGetValue(language, out value);
        }

        public bool Contains(Language language)
        {
            return language != null && languages.ContainsKey(language);
        }

        private static ImmutableDictionary<Language, LanguageConfig> ValidateLanguages(ImmutableDictionary<Language, LanguageConfig> languages)
        {
            var errors = new List<ValidationError>();

            foreach (var languageConfig in languages.Values)
            {
                foreach (var fallback in languageConfig.Fallback)
                {
                    if (!languages.ContainsKey(fallback))
                    {
                        var message = $"Config for language '{languageConfig.Language.Iso2Code}' contains unsupported fallback language '{fallback.Iso2Code}'";

                        errors.Add(new ValidationError(message));
                    }
                }
            }

            if (errors.Count > 0)
            {
                throw new ValidationException("Cannot configure language", errors);
            }

            return languages;
        }

        private void ThrowIfNotFound(Language language)
        {
            if (!Contains(language))
            {
                throw new DomainObjectNotFoundException(language, "Languages", typeof(LanguagesConfig));
            }
        }

        private void ThrowIfFound(Language language, Func<string> message)
        {
            if (Contains(language))
            {
                var error = new ValidationError("Language is already part of the app", "Language");

                throw new ValidationException(message(), error);
            }
        }

        private void ThrowIfMaster(Language language, bool isMaster, Func<string> message)
        {
            if (master?.Language == language || isMaster)
            {
                var error = new ValidationError("Language is the master language", "Language");

                throw new ValidationException(message(), error);
            }
        }
    }
}
