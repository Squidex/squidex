// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Globalization;

namespace Squidex.Infrastructure.Translations;

public static class T
{
    private static ILocalizer? localizer;

    public static void Setup(ILocalizer newLocalizer)
    {
        localizer = newLocalizer;
    }

    public static string Get(string key, object? args = null)
    {
        return Get(key, key, args);
    }

    public static string Get(string key, string fallback, object? args = null)
    {
        Guard.NotNullOrEmpty(key);

        if (localizer == null)
        {
            return key;
        }

        var (result, _) = localizer.Get(CultureInfo.CurrentUICulture, key, fallback, args);

        return result;
    }
}
