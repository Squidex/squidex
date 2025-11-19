// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Infrastructure.Json.Objects;

namespace Squidex.Domain.Apps.Core.Contents;

public static class RichTextExtensions
{
    public static int GetIntAttr(this JsonObject? attrs, string name, int defaultValue = 0)
    {
        if (attrs?.TryGetValue(name, out var value) == true && value.Value is double attr)
        {
            return (int)attr;
        }

        return defaultValue;
    }

    public static string GetStringAttr(this JsonObject? attrs, string name, string defaultValue = "")
    {
        if (attrs?.TryGetValue(name, out var value) == true && value.Value is string attr)
        {
            return attr;
        }

        return defaultValue;
    }

    public static bool TryGetArrayOfObject(this JsonValue value, out JsonArray array)
    {
        array = default!;

        if (value.Value is not JsonArray temp)
        {
            return false;
        }

        foreach (var item in temp)
        {
            if (item.Value is not JsonObject)
            {
                return false;
            }
        }

        array = temp;
        return true;
    }
}
