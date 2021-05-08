// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Collections.Generic;
using System.Linq;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Collections;

namespace Squidex.Domain.Apps.Core.Apps
{
    public sealed record LanguageConfig
    {
        public static readonly LanguageConfig Default = new LanguageConfig();

        public bool IsOptional { get; }

        public ImmutableList<Language> Fallbacks { get; } = ImmutableList.Empty<Language>();

        public LanguageConfig(bool isOptional = false, ImmutableList<Language>? fallbacks = null)
        {
            IsOptional = isOptional;

            if (fallbacks != null)
            {
                Fallbacks = fallbacks;
            }
        }

        internal LanguageConfig Cleanup(string self, IReadOnlyDictionary<string, LanguageConfig> allowed)
        {
            if (Fallbacks.Any(x => x.Iso2Code == self) || Fallbacks.Any(x => !allowed.ContainsKey(x)))
            {
                var cleaned = Fallbacks.Where(x => x.Iso2Code != self && allowed.ContainsKey(x.Iso2Code)).ToImmutableList();

                return new LanguageConfig(IsOptional, cleaned);
            }

            return this;
        }
    }
}
