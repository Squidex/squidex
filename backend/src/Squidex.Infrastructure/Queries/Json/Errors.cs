// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Globalization;
using Squidex.Infrastructure.Properties;

namespace Squidex.Infrastructure.Queries.Json;

internal static class Errors
{
    public static string InvalidJsonStructure()
    {
        return Resources.QueryInvalidJsonStructure;
    }

    public static string InvalidQuery(object message)
    {
        return string.Format(CultureInfo.InvariantCulture, Resources.QueryInvalid, message);
    }

    public static string InvalidQueryJson(object message)
    {
        return string.Format(CultureInfo.InvariantCulture, Resources.QueryInvalidJson, message);
    }

    public static string InvalidPath(PropertyPath path)
    {
        return string.Format(CultureInfo.InvariantCulture, Resources.QueryInvalidPath, path);
    }

    public static string InvalidOperator(object @operator, object type, PropertyPath path)
    {
        return string.Format(CultureInfo.InvariantCulture, Resources.QueryInvalidOperator, @operator, type, path);
    }

    public static string InvalidRegex(object value, PropertyPath path)
    {
        return string.Format(CultureInfo.InvariantCulture, Resources.QueryInvalidRegex, value, path);
    }

    public static string InvalidArray(object @operator, PropertyPath path)
    {
        return string.Format(CultureInfo.InvariantCulture, Resources.QueryInvalidArray, @operator, path);
    }

    public static string WrongExpectedType(object expected, object type, PropertyPath path)
    {
        return string.Format(CultureInfo.InvariantCulture, Resources.QueryWrongExpectedType, expected, type, path);
    }

    public static string WrongFormat(object expected, PropertyPath path)
    {
        return string.Format(CultureInfo.InvariantCulture, Resources.QueryWrongFormat, expected, path);
    }

    public static string WrongType(object type, PropertyPath path)
    {
        return string.Format(CultureInfo.InvariantCulture, Resources.QueryWrongType, type, path);
    }

    public static string WrongPrimitive(object type, PropertyPath path)
    {
        return string.Format(CultureInfo.InvariantCulture, Resources.QueryWrongPrimitive, type, path);
    }
}
