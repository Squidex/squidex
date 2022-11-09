// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Runtime.CompilerServices;
using Squidex.Infrastructure.Translations;
using Squidex.Text;

namespace Squidex.Infrastructure.Validation;

public static class Not
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static string Defined()
    {
        return T.Get("validation.requiredValue");
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static string Defined(string propertyName)
    {
        var property = T.Get($"common.{propertyName.ToCamelCase()}", propertyName);

        return T.Get("validation.required", new { property });
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static string BothDefined(string propertyName1, string propertyName2)
    {
        var property1 = T.Get($"common.{propertyName1.ToCamelCase()}", propertyName1);
        var property2 = T.Get($"common.{propertyName2.ToCamelCase()}", propertyName2);

        return T.Get("validation.requiredBoth", new { property1, property2 });
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static string ValidSlug(string propertyName)
    {
        var property = T.Get($"common.{propertyName.ToCamelCase()}", propertyName);

        return T.Get("validation.slug", new { property });
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static string ValidJavascriptName(string propertyName)
    {
        var property = T.Get($"common.{propertyName.ToCamelCase()}", propertyName);

        return T.Get("validation.javascriptProperty", new { property });
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static string GreaterThan(string propertyName, string otherName)
    {
        var property = T.Get($"common.{propertyName.ToCamelCase()}", propertyName);

        var other = T.Get($"common.{otherName.ToCamelCase()}", otherName);

        return T.Get("validation.greaterThan", new { property, other });
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static string GreaterEqualsThan(string propertyName, string otherName)
    {
        var property = T.Get($"common.{propertyName.ToCamelCase()}", propertyName);

        var other = T.Get($"common.{otherName.ToCamelCase()}", otherName);

        return T.Get("validation.greaterEqualsThan", new { property, other });
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static string LessThan(string propertyName, string otherName)
    {
        var property = T.Get($"common.{propertyName.ToCamelCase()}", propertyName);

        var other = T.Get($"common.{otherName.ToCamelCase()}", otherName);

        return T.Get("validation.lessThan", new { property, other });
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static string LessEqualsThan(string propertyName, string otherName)
    {
        var property = T.Get($"common.{propertyName.ToCamelCase()}", propertyName);

        var other = T.Get($"common.{otherName.ToCamelCase()}", otherName);

        return T.Get("validation.lessEqualsThan", new { property, other });
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static string Between<TField>(string propertyName, TField min, TField max)
    {
        var property = T.Get($"common.{propertyName.ToCamelCase()}", propertyName);

        return T.Get("validation.between", new { property, min, max });
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static string Valid(string propertyName)
    {
        var property = T.Get($"common.{propertyName.ToCamelCase()}", propertyName);

        return T.Get("validation.valid", new { property });
    }
}
