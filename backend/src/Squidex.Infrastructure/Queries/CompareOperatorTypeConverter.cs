// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.ComponentModel;
using System.Globalization;

namespace Squidex.Infrastructure.Queries;

public sealed class CompareOperatorTypeConverter : TypeConverter
{
    public override bool CanConvertFrom(ITypeDescriptorContext? context, Type sourceType)
    {
        return sourceType == typeof(string);
    }

    public override bool CanConvertTo(ITypeDescriptorContext? context, Type? destinationType)
    {
        return destinationType == typeof(string);
    }

    public override object ConvertFrom(ITypeDescriptorContext? context, CultureInfo? culture, object value)
    {
        var op = (string)value;

        switch (op.ToLowerInvariant())
        {
            case "eq":
                return CompareOperator.Equals;
            case "ne":
                return CompareOperator.NotEquals;
            case "lt":
                return CompareOperator.LessThan;
            case "le":
                return CompareOperator.LessThanOrEqual;
            case "gt":
                return CompareOperator.GreaterThan;
            case "ge":
                return CompareOperator.GreaterThanOrEqual;
            case "empty":
                return CompareOperator.Empty;
            case "exists":
                return CompareOperator.Exists;
            case "matchs":
                return CompareOperator.Matchs;
            case "contains":
                return CompareOperator.Contains;
            case "endswith":
                return CompareOperator.EndsWith;
            case "startswith":
                return CompareOperator.StartsWith;
            case "in":
                return CompareOperator.In;
        }

        throw new InvalidCastException($"Unexpected compare operator, got {op}.");
    }

    public override object ConvertTo(ITypeDescriptorContext? context, CultureInfo? culture, object? value, Type? destinationType)
    {
        var op = (CompareOperator)value!;

        switch (op)
        {
            case CompareOperator.Equals:
                return "eq";
            case CompareOperator.NotEquals:
                return "ne";
            case CompareOperator.LessThan:
                return "lt";
            case CompareOperator.LessThanOrEqual:
                return "le";
            case CompareOperator.GreaterThan:
                return "gt";
            case CompareOperator.GreaterThanOrEqual:
                return "gt";
            case CompareOperator.Empty:
                return "empty";
            case CompareOperator.Exists:
                return "exists";
            case CompareOperator.Matchs:
                return "matchs";
            case CompareOperator.Contains:
                return "contains";
            case CompareOperator.EndsWith:
                return "endsWith";
            case CompareOperator.StartsWith:
                return "startsWith";
            case CompareOperator.In:
                return "in";
        }

        throw new InvalidCastException($"Unexpected compare operator, got {op}.");
    }
}
