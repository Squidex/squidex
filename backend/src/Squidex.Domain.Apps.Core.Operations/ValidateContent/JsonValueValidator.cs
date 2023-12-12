// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using NodaTime.Text;
using Squidex.Domain.Apps.Core.Contents;
using Squidex.Domain.Apps.Core.Schemas;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Json;
using Squidex.Infrastructure.Json.Objects;

#pragma warning disable SA1313 // Parameter names should begin with lower-case letter

namespace Squidex.Domain.Apps.Core.ValidateContent;

public sealed class JsonValueValidator : IFieldPropertiesVisitor<bool, JsonValueValidator.Args>
{
    private static readonly JsonValueValidator Instance = new JsonValueValidator();

    public record struct Args(JsonValue Value, IJsonSerializer Serializer);

    private JsonValueValidator()
    {
    }

    public static bool IsValid(IField field, JsonValue value, IJsonSerializer serializer)
    {
        Guard.NotNull(field);
        Guard.NotNull(value);

        var args = new Args(value, serializer);

        return field.RawProperties.Accept(Instance, args);
    }

    public bool Visit(ArrayFieldProperties properties, Args args)
    {
        return IsValidObjectList(args.Value);
    }

    public bool Visit(AssetsFieldProperties properties, Args args)
    {
        return IsValidStringList(args.Value);
    }

    public bool Visit(BooleanFieldProperties properties, Args args)
    {
        return args.Value.Value is bool;
    }

    public bool Visit(ComponentFieldProperties properties, Args args)
    {
        return IsValidComponent(args.Value);
    }

    public bool Visit(ComponentsFieldProperties properties, Args args)
    {
        return IsValidComponentList(args.Value);
    }

    public bool Visit(DateTimeFieldProperties properties, Args args)
    {
        if (args.Value.Value is string s)
        {
            var parseResult = InstantPattern.ExtendedIso.Parse(s);

            return parseResult.Success;
        }

        return false;
    }

    public bool Visit(GeolocationFieldProperties properties, Args args)
    {
        var result = GeoJsonValue.TryParse(args.Value, args.Serializer, out _);

        return result == GeoJsonParseResult.Success;
    }

    public bool Visit(JsonFieldProperties properties, Args args)
    {
        return true;
    }

    public bool Visit(NumberFieldProperties properties, Args args)
    {
        return args.Value.Value is double;
    }

    public bool Visit(ReferencesFieldProperties properties, Args args)
    {
        return IsValidStringList(args.Value);
    }

    public bool Visit(RichTextFieldProperties properties, Args args)
    {
        return args.Value.Type == JsonValueType.Null || RichTextNode.TryCreate(args.Value, out _);
    }

    public bool Visit(StringFieldProperties properties, Args args)
    {
        return args.Value.Value is string;
    }

    public bool Visit(TagsFieldProperties properties, Args args)
    {
        return IsValidStringList(args.Value);
    }

    public bool Visit(UIFieldProperties properties, Args args)
    {
        return true;
    }

    private static bool IsValidStringList(JsonValue value)
    {
        if (value.Value is not JsonArray a)
        {
            return false;
        }

        if (a.Count == 0)
        {
            return true;
        }

        foreach (var item in a)
        {
            if (item.Value is not string)
            {
                return false;
            }
        }

        return true;
    }

    private static bool IsValidObjectList(JsonValue value)
    {
        if (value.Value is not JsonArray a)
        {
            return false;
        }

        if (a.Count == 0)
        {
            return true;
        }

        foreach (var item in a)
        {
            if (item.Value is not JsonObject)
            {
                return false;
            }
        }

        return true;
    }

    private static bool IsValidComponentList(JsonValue value)
    {
        if (value.Value is not JsonArray a)
        {
            return false;
        }

        foreach (var item in a)
        {
            if (!IsValidComponent(item))
            {
                return false;
            }
        }

        return true;
    }

    private static bool IsValidComponent(JsonValue value)
    {
        return Component.IsValid(value, out _);
    }
}
