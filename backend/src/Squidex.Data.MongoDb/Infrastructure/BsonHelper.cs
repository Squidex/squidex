﻿// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

namespace Squidex.Infrastructure;

public static class BsonHelper
{
    private const string Empty = "§empty";
    private const string TypeBson = "§type";
    private const string TypeJson = "$type";
    private const string DotBson = "_§§_";
    private const string DotJson = ".";

    public static string BsonToJsonName(this string value)
    {
        if (value == Empty)
        {
            return string.Empty;
        }

        if (value == TypeBson)
        {
            return TypeJson;
        }

        var result = value.ReplaceFirst('§', '$').Replace(DotBson, DotJson, StringComparison.Ordinal);

        return result;
    }

    public static string JsonToBsonName(this string value)
    {
        if (value.Length == 0)
        {
            return Empty;
        }

        if (value == TypeJson)
        {
            return TypeBson;
        }

        var result = value.ReplaceFirst('$', '§').Replace(DotJson, DotBson, StringComparison.Ordinal);

        return result;
    }

    private static string ReplaceFirst(this string value, char toReplace, char replacement)
    {
        if (value.Length == 0 || value[0] != toReplace)
        {
            return value;
        }

        if (value.Length == 1)
        {
            return toReplace.ToString();
        }

        return replacement + value[1..];
    }
}
