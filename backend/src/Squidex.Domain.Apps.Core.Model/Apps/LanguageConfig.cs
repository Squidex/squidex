// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Collections.Generic;
using System.Linq;
using Squidex.Infrastructure;

namespace Squidex.Domain.Apps.Core.Apps
{
    [Equals(DoNotAddEqualityOperators = true)]
    public sealed class LanguageConfig
    {
        public static readonly LanguageConfig Default = new LanguageConfig();

        private readonly Language[] fallbacks;

        public bool IsOptional { get; }

        public IEnumerable<Language> Fallbacks
        {
            get { return fallbacks; }
        }

        public LanguageConfig(bool isOptional = false, params Language[]? fallbacks)
        {
            IsOptional = isOptional;

            this.fallbacks = fallbacks ?? Array.Empty<Language>();
        }

        internal LanguageConfig Cleanup(string self, IReadOnlyDictionary<string, LanguageConfig> allowed)
        {
            if (fallbacks.Any(x => x.Iso2Code == self) || fallbacks.Any(x => !allowed.ContainsKey(x)))
            {
                var cleaned = Fallbacks.Where(x => x.Iso2Code != self && allowed.ContainsKey(x.Iso2Code)).ToArray();

                return new LanguageConfig(IsOptional, cleaned);
            }

            return this;
        }
    }
}
