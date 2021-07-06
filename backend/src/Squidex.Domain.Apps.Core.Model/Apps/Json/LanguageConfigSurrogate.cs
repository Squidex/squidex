// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Linq;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Collections;

namespace Squidex.Domain.Apps.Core.Apps.Json
{
    public sealed class LanguageConfigSurrogate : ISurrogate<LanguageConfig>
    {
        public Language[]? Fallback { get; set; }

        public bool IsOptional { get; set; }

        public void FromSource(LanguageConfig source)
        {
            IsOptional = source.IsOptional;

            Fallback = source.Fallbacks.ToArray();
        }

        public LanguageConfig ToSource()
        {
            if (!IsOptional && (Fallback == null || Fallback.Length == 0))
            {
                return LanguageConfig.Default;
            }
            else
            {
                return new LanguageConfig(IsOptional, ImmutableList.Create(Fallback));
            }
        }
    }
}
