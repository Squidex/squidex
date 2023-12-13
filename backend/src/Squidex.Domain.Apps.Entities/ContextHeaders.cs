// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Globalization;

namespace Squidex.Domain.Apps.Entities;

public static class ContextHeaders
{
    private static readonly char[] Separators = [',', ';'];

    public const string KeyBatchSize = "X-BatchSize";
    public const string KeyNoCacheKeys = "X-NoCacheKeys";
    public const string KeyNoScripting = "X-NoScripting";
    public const string KeyNoSlowTotal = "X-NoSlowTotal";
    public const string KeyNoTotal = "X-NoTotal";

    public static int BatchSize(this Context context)
    {
        return context.AsNumber(KeyBatchSize);
    }

    public static ICloneBuilder WithBatchSize(this ICloneBuilder builder, int value)
    {
        return builder.WithNumber(KeyBatchSize, value);
    }

    public static bool NoCacheKeys(this Context context)
    {
        return context.AsBoolean(KeyNoCacheKeys);
    }

    public static ICloneBuilder WithNoCacheKeys(this ICloneBuilder builder, bool value = true)
    {
        return builder.WithBoolean(KeyNoCacheKeys, value);
    }

    public static bool NoScripting(this Context context)
    {
        return context.AsBoolean(KeyNoScripting);
    }

    public static ICloneBuilder WithNoScripting(this ICloneBuilder builder, bool value = true)
    {
        return builder.WithBoolean(KeyNoScripting, value);
    }

    public static bool NoTotal(this Context context)
    {
        return context.AsBoolean(KeyNoTotal);
    }

    public static ICloneBuilder WithNoTotal(this ICloneBuilder builder, bool value = true)
    {
        return builder.WithBoolean(KeyNoTotal, value);
    }

    public static bool NoSlowTotal(this Context context)
    {
        return context.AsBoolean(KeyNoSlowTotal);
    }

    public static ICloneBuilder WithNoSlowTotal(this ICloneBuilder builder, bool value = true)
    {
        return builder.WithBoolean(KeyNoSlowTotal, value);
    }

    public static ICloneBuilder WithNumber(this ICloneBuilder builder, string key, int value)
    {
        if (value != 0)
        {
            builder.SetHeader(key, value.ToString(CultureInfo.InvariantCulture));
        }
        else
        {
            builder.Remove(key);
        }

        return builder;
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
            builder.SetHeader(key, string.Join(',', values));
        }
        else
        {
            builder.Remove(key);
        }

        return builder;
    }

    public static bool AsBoolean(this Context context, string key)
    {
        return context.Headers.ContainsKey(key);
    }

    public static int AsNumber(this Context context, string key)
    {
        if (context.Headers.TryGetValue(key, out var value) && int.TryParse(value, CultureInfo.InvariantCulture, out var result))
        {
            return result;
        }

        return 0;
    }

    public static IEnumerable<string> AsStrings(this Context context, string key)
    {
        if (context.Headers.TryGetValue(key, out var value))
        {
            return value.Split(Separators, StringSplitOptions.RemoveEmptyEntries).Select(x => x.Trim()).Distinct();
        }

        return Enumerable.Empty<string>();
    }
}
