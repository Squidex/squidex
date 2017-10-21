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

namespace Squidex.Domain.Apps.Core
{
    public sealed class LanguagesConfig : IFieldPartitioning
    {
        private State state;

        public LanguageConfig Master
        {
            get { return state.Master; }
        }

        public int Count
        {
            get { return state.Languages.Count; }
        }

        IFieldPartitionItem IFieldPartitioning.Master
        {
            get { return state.Master; }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return state.Languages.Values.GetEnumerator();
        }

        IEnumerator<IFieldPartitionItem> IEnumerable<IFieldPartitionItem>.GetEnumerator()
        {
            return state.Languages.Values.GetEnumerator();
        }

        private LanguagesConfig(ICollection<LanguageConfig> configs)
        {
            Guard.NotNull(configs, nameof(configs));

            state = new State(configs.ToImmutableDictionary(x => x.Language), configs.FirstOrDefault());
        }

        public static LanguagesConfig Build(params LanguageConfig[] configs)
        {
            Guard.NotNull(configs, nameof(configs));

            return new LanguagesConfig(configs);
        }

        public static LanguagesConfig Build(params Language[] languages)
        {
            Guard.NotNull(languages, nameof(languages));

            return new LanguagesConfig(languages.Select(x => new LanguageConfig(x, false)).ToList());
        }

        public void MakeMaster(Language language)
        {
            Guard.NotNull(language, nameof(language));

            state = new State(state.Languages, state.Languages[language]);
        }

        public void Set(LanguageConfig config)
        {
            Guard.NotNull(config, nameof(config));

            state = new State(state.Languages.SetItem(config.Language, config), state.Master?.Language == config.Language ? config : state.Master);
        }

        public void Remove(Language language)
        {
            Guard.NotNull(language, nameof(language));

            state = new State(
                state.Languages.Values.Where(x => x.Language != language)
                    .Select(config =>
                    {
                        return new LanguageConfig(
                            config.Language,
                            config.IsOptional,
                            config.LanguageFallbacks.Except(new[] { language }));
                    })
                    .ToImmutableDictionary(x => x.Language), state.Master.Language == language ? null : state.Master);
        }

        public bool Contains(Language language)
        {
            return language != null && state.Languages.ContainsKey(language);
        }

        public bool TryGetConfig(Language language, out LanguageConfig config)
        {
            return state.Languages.TryGetValue(language, out config);
        }

        public bool TryGetItem(string key, out IFieldPartitionItem item)
        {
            item = null;

            if (Language.IsValidLanguage(key) && state.Languages.TryGetValue(key, out var value))
            {
                item = value;

                return true;
            }

            return false;
        }

        private sealed class State
        {
            public ImmutableDictionary<Language, LanguageConfig> Languages { get; }

            public LanguageConfig Master { get; }

            public State(ImmutableDictionary<Language, LanguageConfig> languages, LanguageConfig master)
            {
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

                Languages = languages;

                if (master == null)
                {
                    throw new InvalidOperationException("Config has no master language.");
                }

                if (master.IsOptional)
                {
                    throw new InvalidOperationException("Config has an optional master language.");
                }

                this.Master = master;
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
