// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Infrastructure;
using Squidex.Infrastructure.Collections;

#pragma warning disable SA1313 // Parameter names should begin with lower-case letter

namespace Squidex.Domain.Apps.Core.Apps;

public sealed record LanguageConfig(bool IsOptional = false, ReadonlyList<Language>? Fallbacks = null)
{
    public static readonly LanguageConfig Default = new LanguageConfig();

    public ReadonlyList<Language> Fallbacks { get; } = Fallbacks ?? ReadonlyList.Empty<Language>();

    internal LanguageConfig Cleanup(string self, IReadOnlyDictionary<string, LanguageConfig> allowed)
    {
        if (Fallbacks.Any(x => x.Iso2Code == self) || Fallbacks.Any(x => !allowed.ContainsKey(x)))
        {
            var cleaned = Fallbacks.Where(x => x.Iso2Code != self && allowed.ContainsKey(x.Iso2Code)).ToReadonlyList();

            return new LanguageConfig(IsOptional, cleaned);
        }

        return this;
    }
}
