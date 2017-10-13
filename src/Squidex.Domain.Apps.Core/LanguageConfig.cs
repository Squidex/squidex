// ==========================================================================
//  LanguageConfig.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Squidex.Infrastructure;

namespace Squidex.Domain.Apps.Core
{
    public sealed class LanguageConfig : IFieldPartitionItem
    {
        public bool IsOptional { get; }

        public Language Language { get; }

        public ImmutableList<Language> LanguageFallbacks { get; }

        public string Key
        {
            get { return Language.Iso2Code; }
        }

        public string Name
        {
            get { return Language.EnglishName; }
        }

        IEnumerable<string> IFieldPartitionItem.Fallback
        {
            get { return LanguageFallbacks.Select(x => x.Iso2Code); }
        }

        public LanguageConfig(Language language, bool isOptional, params Language[] fallback)
            : this(language, isOptional, fallback?.ToImmutableList())
        {
        }

        public LanguageConfig(Language language, bool isOptional, IEnumerable<Language> fallback)
            : this(language, isOptional, fallback?.ToImmutableList())
        {
        }

        public LanguageConfig(Language language, bool isOptional = false, ImmutableList<Language> fallback = null)
        {
            Guard.NotNull(language, nameof(language));

            IsOptional = isOptional;

            Language = language;
            LanguageFallbacks = fallback ?? ImmutableList<Language>.Empty;
        }
    }
}
