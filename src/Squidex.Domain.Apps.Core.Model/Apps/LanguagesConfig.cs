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

namespace Squidex.Domain.Apps.Core.Apps
{
    public sealed class LanguagesConfig : IFieldPartitioning
    {
        public static readonly LanguagesConfig Empty = new LanguagesConfig(ImmutableDictionary<Language, LanguageConfig>.Empty, null, false);
        public static readonly LanguagesConfig English = LanguagesConfig.Build(Language.EN);

        private readonly ImmutableDictionary<Language, LanguageConfig> languages;
        private readonly LanguageConfig master;

        public LanguageConfig Master
        {
            get { return master; }
        }

        IFieldPartitionItem IFieldPartitioning.Master
        {
            get { return master; }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return languages.Values.GetEnumerator();
        }

        IEnumerator<IFieldPartitionItem> IEnumerable<IFieldPartitionItem>.GetEnumerator()
        {
            return languages.Values.GetEnumerator();
        }

        public int Count
        {
            get { return languages.Count; }
        }

        private LanguagesConfig(ImmutableDictionary<Language, LanguageConfig> languages, LanguageConfig master, bool checkMaster = true)
        {
            if (checkMaster)
            {
                this.master = master ?? throw new InvalidOperationException("Config has no master language.");
            }

            foreach (var languageConfig in languages.Values)
            {
                foreach (var fallback in languageConfig.LanguageFallbacks)
                {
                    if (!languages.ContainsKey(fallback))
                    {
                        var message = $"Config for language '{languageConfig.Language.Iso2Code}' contains unsupported fallback language '{fallback.Iso2Code}'";

                        throw new InvalidOperationException(message);
                    }
                }
            }

            this.languages = languages;
        }

        public static LanguagesConfig Build(ICollection<LanguageConfig> configs)
        {
            Guard.NotNull(configs, nameof(configs));

            return new LanguagesConfig(configs.ToImmutableDictionary(x => x.Language), configs.FirstOrDefault());
        }

        public static LanguagesConfig Build(params LanguageConfig[] configs)
        {
            return Build(configs?.ToList());
        }

        public static LanguagesConfig Build(params Language[] languages)
        {
            return Build(languages?.Select(x => new LanguageConfig(x)).ToList());
        }

        public LanguagesConfig MakeMaster(Language language)
        {
            Guard.NotNull(language, nameof(language));

            return new LanguagesConfig(languages, languages[language]);
        }

        public LanguagesConfig Set(LanguageConfig config)
        {
            Guard.NotNull(config, nameof(config));

            return new LanguagesConfig(languages.SetItem(config.Language, config), Master?.Language == config.Language ? config : Master);
        }

        public LanguagesConfig Remove(Language language)
        {
            Guard.NotNull(language, nameof(language));

            var newLanguages =
                languages.Values.Where(x => x.Language != language)
                    .Select(config =>
                    {
                        return new LanguageConfig(
                            config.Language,
                            config.IsOptional,
                            config.LanguageFallbacks.Except(new[] { language }));
                    })
                    .ToImmutableDictionary(x => x.Language);

            var newMaster =
                Master.Language != language ?
                Master :
                newLanguages.Values.FirstOrDefault();

            return new LanguagesConfig(newLanguages, newMaster);
        }

        public bool Contains(Language language)
        {
            return language != null && languages.ContainsKey(language);
        }

        public bool TryGetConfig(Language language, out LanguageConfig config)
        {
            return languages.TryGetValue(language, out config);
        }

        public bool TryGetItem(string key, out IFieldPartitionItem item)
        {
            if (Language.IsValidLanguage(key) && languages.TryGetValue(key, out var value))
            {
                item = value;

                return true;
            }
            else
            {
                item = null;

                return false;
            }
        }

        public PartitionResolver ToResolver()
        {
            return partitioning =>
            {
                if (partitioning.Equals(Partitioning.Invariant))
                {
                    return InvariantPartitioning.Instance;
                }

                return this;
            };
        }
    }
}
