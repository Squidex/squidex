// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Diagnostics.Contracts;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Collections;

namespace Squidex.Domain.Apps.Core.Apps;

public sealed class LanguagesConfig : IFieldPartitioning
{
    public static readonly LanguagesConfig English = new (
        new Dictionary<string, LanguageConfig>
        {
            [Language.EN] = new LanguageConfig()
        },
        Language.EN);

    private readonly Dictionary<string, LanguageConfig> languages;
    private readonly string master;

    public string Master
    {
        get => master;
    }

    public IEnumerable<string> AllKeys
    {
        get => languages.Keys;
    }

    public IReadOnlyDictionary<string, LanguageConfig> Languages
    {
        get => languages;
    }

    public LanguagesConfig(Dictionary<string, LanguageConfig> languages, string master)
    {
        Guard.NotNull(languages);
        Guard.NotNullOrEmpty(master);

        Cleanup(languages, ref master);

        this.languages = languages;

        this.master = master;
    }

    [Pure]
    public LanguagesConfig MakeMaster(Language language)
    {
        Guard.NotNull(language);

        return Build(languages, language);
    }

    [Pure]
    public LanguagesConfig Set(Language language, bool isOptional = false, params Language[]? fallbacks)
    {
        Guard.NotNull(language);

        var newLanguages = new Dictionary<string, LanguageConfig>(languages)
        {
            [language] = new LanguageConfig(isOptional, ReadonlyList.Create(fallbacks))
        };

        return Build(newLanguages, master);
    }

    [Pure]
    public LanguagesConfig Remove(Language language)
    {
        Guard.NotNull(language);

        var newLanguages = new Dictionary<string, LanguageConfig>(languages);

        newLanguages.Remove(language);

        return Build(newLanguages, master);
    }

    private LanguagesConfig Build(Dictionary<string, LanguageConfig> newLanguages, string newMaster)
    {
        if (newLanguages.Count == 0)
        {
            return this;
        }

        Cleanup(newLanguages, ref newMaster);

        if (EqualLanguages(newLanguages) && Equals(newMaster, master))
        {
            return this;
        }

        return new LanguagesConfig(newLanguages, newMaster);
    }

    private bool EqualLanguages(Dictionary<string, LanguageConfig> newLanguages)
    {
        return newLanguages.EqualsDictionary(languages);
    }

    private void Cleanup(Dictionary<string, LanguageConfig> newLanguages, ref string newMaster)
    {
        if (!newLanguages.ContainsKey(newMaster))
        {
            if (newLanguages.ContainsKey(master))
            {
                newMaster = master;
            }
            else
            {
                newMaster = newLanguages.Keys.First();
            }
        }

        var masterConfig = newLanguages[newMaster];

        if (masterConfig.IsOptional || masterConfig.Fallbacks.Any())
        {
            newLanguages[newMaster] = LanguageConfig.Default;
        }

        foreach (var (key, config) in newLanguages.ToList())
        {
            newLanguages[key] = config.Cleanup(key, newLanguages);
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

    public bool IsMaster(string key)
    {
        return Equals(Master, key);
    }

    public string? GetName(string key)
    {
        if (key != null && languages.ContainsKey(key))
        {
            return Language.GetLanguage(key).EnglishName;
        }

        return null;
    }

    public bool IsOptional(string key)
    {
        if (key != null && languages.TryGetValue(key, out var value))
        {
            return value.IsOptional;
        }

        return false;
    }

    public IEnumerable<string> GetPriorities(string key)
    {
        if (key != null)
        {
            if (Equals(Master, key))
            {
                yield return key;
            }
            else if (languages.TryGetValue(key, out var config))
            {
                yield return key;

                foreach (var fallback in config.Fallbacks)
                {
                    yield return fallback;
                }

                if (config.Fallbacks.All(x => x.Iso2Code != Master))
                {
                    yield return Master;
                }
            }
        }
    }

    public bool Contains(string key)
    {
        return key != null && languages.ContainsKey(key);
    }

    public override string ToString()
    {
        return "language";
    }
}
