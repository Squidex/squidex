// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Collections.Generic;
using System.Linq;
using Squidex.Infrastructure;

namespace Squidex.Domain.Apps.Core.Apps
{
    public sealed class LanguageConfig : IFieldPartitionItem
    {
        private readonly Language language;
        private readonly Language[] languageFallbacks;
        private readonly bool isOptional;

        public bool IsOptional
        {
            get { return isOptional; }
        }

        public Language Language
        {
            get { return language; }
        }

        public IEnumerable<Language> LanguageFallbacks
        {
            get { return languageFallbacks; }
        }

        string IFieldPartitionItem.Key
        {
            get { return language.Iso2Code; }
        }

        string IFieldPartitionItem.Name
        {
            get { return language.EnglishName; }
        }

        IEnumerable<string> IFieldPartitionItem.Fallback
        {
            get { return LanguageFallbacks.Select(x => x.Iso2Code); }
        }

        public LanguageConfig(Language language, bool isOptional = false, IEnumerable<Language> fallback = null)
            : this(language, isOptional, fallback?.ToArray())
        {
        }

        public LanguageConfig(Language language, bool isOptional = false, params Language[] fallback)
        {
            Guard.NotNull(language, nameof(language));

            this.isOptional = isOptional;

            this.language = language;
            this.languageFallbacks = fallback ?? new Language[0];
        }
    }
}
