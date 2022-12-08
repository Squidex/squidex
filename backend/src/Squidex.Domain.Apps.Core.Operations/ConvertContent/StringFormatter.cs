// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Domain.Apps.Core.Schemas;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Json.Objects;

#pragma warning disable SA1313 // Parameter names should begin with lower-case letter

namespace Squidex.Domain.Apps.Core.ConvertContent;

public sealed class StringFormatter : IFieldPropertiesVisitor<string, StringFormatter.Args>
{
    private static readonly StringFormatter Instance = new StringFormatter();

    public record struct Args(JsonValue Value);

    private StringFormatter()
    {
    }

    public static string Format(IField field, JsonValue value)
    {
        Guard.NotNull(field);

        if (value == default)
        {
            return string.Empty;
        }

        var args = new Args(value);

        return field.RawProperties.Accept(Instance, args);
    }

    public string Visit(ArrayFieldProperties properties, Args args)
    {
        return FormatArray(args.Value, "Item", "Items");
    }

    public string Visit(AssetsFieldProperties properties, Args args)
    {
        return FormatArray(args.Value, "Asset", "Assets");
    }

    public string Visit(BooleanFieldProperties properties, Args args)
    {
        if (Equals(args.Value.Value, true))
        {
            return "Yes";
        }
        else
        {
            return "No";
        }
    }

    public string Visit(ComponentFieldProperties properties, Args args)
    {
        return "{ Component }";
    }

    public string Visit(ComponentsFieldProperties properties, Args args)
    {
        return FormatArray(args.Value, "Component", "Components");
    }

    public string Visit(DateTimeFieldProperties properties, Args args)
    {
        return args.Value.ToString();
    }

    public string Visit(GeolocationFieldProperties properties, Args args)
    {
        if (args.Value.Value is JsonObject o &&
            o.TryGetValue("latitude", out var found) && found.Value is double lat &&
            o.TryGetValue("longitude", out found) && found.Value is double lon)
        {
            return $"{lat}, {lon}";
        }
        else
        {
            return string.Empty;
        }
    }

    public string Visit(JsonFieldProperties properties, Args args)
    {
        return "<Json />";
    }

    public string Visit(NumberFieldProperties properties, Args args)
    {
        return args.Value.ToString();
    }

    public string Visit(ReferencesFieldProperties properties, Args args)
    {
        return FormatArray(args.Value, "Reference", "References");
    }

    public string Visit(StringFieldProperties properties, Args args)
    {
        if (properties.Editor == StringFieldEditor.StockPhoto)
        {
            return "[Photo]";
        }
        else
        {
            return args.Value.ToString();
        }
    }

    public string Visit(TagsFieldProperties properties, Args args)
    {
        if (args.Value.Value is JsonArray a)
        {
            return string.Join(", ", a);
        }
        else
        {
            return string.Empty;
        }
    }

    public string Visit(UIFieldProperties properties, Args args)
    {
        return string.Empty;
    }

    private static string FormatArray(JsonValue value, string singularName, string pluralName)
    {
        if (value.Value is JsonArray a)
        {
            if (a.Count > 1)
            {
                return $"{a.Count} {pluralName}";
            }
            else if (a.Count == 1)
            {
                return $"1 {singularName}";
            }
        }

        return $"0 {pluralName}";
    }
}
