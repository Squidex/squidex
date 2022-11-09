// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

namespace Squidex.Translator;

public static class Extensions
{
    public static bool IsPotentialText(this string text)
    {
        return !string.IsNullOrWhiteSpace(text) && text.Any(c => char.IsLetter(c));
    }

    public static bool IsPotentialMultiWordText(this string text)
    {
        return text.Contains(' ', StringComparison.Ordinal) && text.IsPotentialText();
    }
}
