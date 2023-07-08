// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

namespace Squidex.Domain.Apps.Entities;

public static class ContextHeaders
{
    public const string NoCacheKeys = "X-NoCacheKeys";
    public const string NoScripting = "X-NoScripting";
    public const string NoSlowTotal = "X-NoSlowTotal";
    public const string NoTotal = "X-NoTotal";

    public static bool ShouldSkipCacheKeys(this Context context)
    {
        return context.Headers.ContainsKey(NoCacheKeys);
    }

    public static ICloneBuilder WithoutCacheKeys(this ICloneBuilder builder, bool value = true)
    {
        return builder.WithBoolean(NoCacheKeys, value);
    }

    public static bool ShouldSkipScripting(this Context context)
    {
        return context.Headers.ContainsKey(NoScripting);
    }

    public static ICloneBuilder WithoutScripting(this ICloneBuilder builder, bool value = true)
    {
        return builder.WithBoolean(NoScripting, value);
    }

    public static bool ShouldSkipTotal(this Context context)
    {
        return context.Headers.ContainsKey(NoTotal);
    }

    public static ICloneBuilder WithoutTotal(this ICloneBuilder builder, bool value = true)
    {
        return builder.WithBoolean(NoTotal, value);
    }

    public static bool ShouldSkipSlowTotal(this Context context)
    {
        return context.Headers.ContainsKey(NoSlowTotal);
    }

    public static ICloneBuilder WithoutSlowTotal(this ICloneBuilder builder, bool value = true)
    {
        return builder.WithBoolean(NoSlowTotal, value);
    }

    public static ICloneBuilder WithBoolean(this ICloneBuilder builder, string key, bool value)
    {
        if (value)
        {
            builder.SetHeader(key, "1");
        }
        else
        {
            builder.Remove(key);
        }

        return builder;
    }

    public static ICloneBuilder WithStrings(this ICloneBuilder builder, string key, IEnumerable<string>? values)
    {
        if (values?.Any() == true)
        {
            builder.SetHeader(key, string.Join(",", values));
        }
        else
        {
            builder.Remove(key);
        }

        return builder;
    }
}
