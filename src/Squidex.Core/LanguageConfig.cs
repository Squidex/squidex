// ==========================================================================
//  LanguageConfig.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System.Collections.Generic;
using Squidex.Infrastructure;
using System.Collections.Immutable;

namespace Squidex.Core
{
    public sealed class LanguageConfig
    {
        public bool IsOptional { get; }

        public Language Language { get; }

        public ImmutableList<Language> Fallback { get; }

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

            Language = language;
            
            IsOptional = isOptional;

            Fallback = fallback ?? ImmutableList<Language>.Empty;
        }
    }
}
