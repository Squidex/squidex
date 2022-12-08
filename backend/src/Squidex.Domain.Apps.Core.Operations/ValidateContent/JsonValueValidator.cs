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

public sealed class JsonValueValidator : IFieldVisitor<bool, JsonValueValidator.Args>
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

        return field.Accept(Instance, args);
    }

    public bool Visit(IArrayField field, Args args)
    {
        return IsValidObjectList(args.Value);
    }

    public bool Visit(IField<AssetsFieldProperties> field, Args args)
    {
        return IsValidStringList(args.Value);
    }

    public bool Visit(IField<BooleanFieldProperties> field, Args args)
    {
        return args.Value.Value is bool;
    }

    public bool Visit(IField<ComponentFieldProperties> field, Args args)
    {
        return IsValidComponent(args.Value);
    }

    public bool Visit(IField<ComponentsFieldProperties> field, Args args)
    {
        return IsValidComponentList(args.Value);
    }

    public bool Visit(IField<DateTimeFieldProperties> field, Args args)
    {
        if (args.Value.Value is string s)
        {
            var parseResult = InstantPattern.ExtendedIso.Parse(s);

            return parseResult.Success;
        }

        return false;
    }

    public bool Visit(IField<GeolocationFieldProperties> field, Args args)
    {
        var result = GeoJsonValue.TryParse(args.Value, args.Serializer, out _);

        return result == GeoJsonParseResult.Success;
    }

    public bool Visit(IField<JsonFieldProperties> field, Args args)
    {
        return true;
    }

    public bool Visit(IField<NumberFieldProperties> field, Args args)
    {
        return args.Value.Value is double;
    }

    public bool Visit(IField<ReferencesFieldProperties> field, Args args)
    {
        return IsValidStringList(args.Value);
    }

    public bool Visit(IField<StringFieldProperties> field, Args args)
    {
        return args.Value.Value is string;
    }

    public bool Visit(IField<TagsFieldProperties> field, Args args)
    {
        return IsValidStringList(args.Value);
    }

    public bool Visit(IField<UIFieldProperties> field, Args args)
    {
        return true;
    }

    private static bool IsValidStringList(JsonValue value)
    {
        if (value.Value is not JsonArray a)
        {
            return false;
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
