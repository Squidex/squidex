// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

namespace Squidex.Web;

public static class ETagUtils
{
    public static string ToWeakEtag(string? etag)
    {
        return $"W/{etag}";
    }

    public static bool IsStrongEtag(string etag)
    {
        return !IsWeakEtag(etag.AsSpan());
    }

    public static bool IsWeakEtag(string etag)
    {
        return IsWeakEtag(etag.AsSpan());
    }

    public static bool IsWeakEtag(ReadOnlySpan<char> etag)
    {
        return etag.StartsWith("W/", StringComparison.OrdinalIgnoreCase);
    }

    public static bool IsSameEtag(string lhs, string rhs)
    {
        return IsSameEtag(lhs.AsSpan(), rhs.AsSpan());
    }

    public static bool IsSameEtag(ReadOnlySpan<char> lhs, ReadOnlySpan<char> rhs)
    {
        var isMatch = lhs.Equals(rhs, StringComparison.Ordinal);

        if (isMatch)
        {
            return true;
        }

        if (lhs.StartsWith("W/", StringComparison.OrdinalIgnoreCase))
        {
            lhs = lhs[2..];
        }

        if (rhs.StartsWith("W/", StringComparison.OrdinalIgnoreCase))
        {
            rhs = rhs[2..];
        }

        return lhs.Equals(rhs, StringComparison.Ordinal);
    }
}
