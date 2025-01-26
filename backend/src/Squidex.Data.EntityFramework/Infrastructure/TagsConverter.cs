// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

namespace Squidex.Infrastructure;

public static class TagsConverter
{
    private const string Separator = "&&";

    public static string FormatFilter(string value)
    {
        return $"{Separator}{value}{Separator}";
    }

    public static string ToString(this ICollection<string> values)
    {
        if (values.Count == 0)
        {
            return string.Empty;
        }

        return $"{Separator}{string.Join(Separator, values)}{Separator}";
    }

    public static HashSet<string> ToSet(this string values)
    {
        if (string.IsNullOrEmpty(values))
        {
            return [];
        }

        return values.Split(Separator, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries).ToHashSet();
    }
}
