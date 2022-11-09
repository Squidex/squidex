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
using Squidex.Infrastructure.Collections;
using Squidex.Infrastructure.Json;
using Squidex.Infrastructure.Json.Objects;
using Squidex.Infrastructure.Translations;

#pragma warning disable SA1313 // Parameter names should begin with lower-case letter

namespace Squidex.Domain.Apps.Core.ValidateContent;

public sealed class JsonValueConverter : IFieldVisitor<(object? Result, JsonError? Error), JsonValueConverter.Args>
{
    private static readonly JsonValueConverter Instance = new JsonValueConverter();

    public record struct Args(JsonValue Value, IJsonSerializer Serializer, ResolvedComponents Components);

    private JsonValueConverter()
    {
    }

    public static (object? Result, JsonError? Error) ConvertValue(IField field, JsonValue value, IJsonSerializer serializer,
        ResolvedComponents components)
    {
        Guard.NotNull(field);
        Guard.NotNull(value);

        var args = new Args(value, serializer, components);

        return field.Accept(Instance, args);
    }

    public (object? Result, JsonError? Error) Visit(IField<JsonFieldProperties> field, Args args)
    {
        return (args.Value, null);
    }

    public (object? Result, JsonError? Error) Visit(IArrayField field, Args args)
    {
        return ConvertToObjectList(args.Value);
    }

    public (object? Result, JsonError? Error) Visit(IField<AssetsFieldProperties> field, Args args)
    {
        return ConvertToIdList(args.Value);
    }

    public (object? Result, JsonError? Error) Visit(IField<ComponentFieldProperties> field, Args args)
    {
        return ConvertToComponent(args.Value, args.Components, field.Properties.SchemaIds);
    }

    public (object? Result, JsonError? Error) Visit(IField<ComponentsFieldProperties> field, Args args)
    {
        return ConvertToComponentList(args.Value, args.Components, field.Properties.SchemaIds);
    }

    public (object? Result, JsonError? Error) Visit(IField<ReferencesFieldProperties> field, Args args)
    {
        return ConvertToIdList(args.Value);
    }

    public (object? Result, JsonError? Error) Visit(IField<TagsFieldProperties> field, Args args)
    {
        return ConvertToStringList(args.Value);
    }

    public (object? Result, JsonError? Error) Visit(IField<BooleanFieldProperties> field, Args args)
    {
        if (args.Value.Value is bool b)
        {
            return (b, null);
        }

        return (null, new JsonError(T.Get("contents.invalidBoolean")));
    }

    public (object? Result, JsonError? Error) Visit(IField<NumberFieldProperties> field, Args args)
    {
        if (args.Value.Value is double d)
        {
            return (d, null);
        }

        return (null, new JsonError(T.Get("contents.invalidNumber")));
    }

    public (object? Result, JsonError? Error) Visit(IField<StringFieldProperties> field, Args args)
    {
        if (args.Value.Value is string s)
        {
            return (s, null);
        }

        return (null, new JsonError(T.Get("contents.invalidString")));
    }

    public (object? Result, JsonError? Error) Visit(IField<UIFieldProperties> field, Args args)
    {
        return (args.Value, null);
    }

    public (object? Result, JsonError? Error) Visit(IField<DateTimeFieldProperties> field, Args args)
    {
        if (args.Value.Value is string s)
        {
            var parseResult = InstantPattern.ExtendedIso.Parse(s);

            if (!parseResult.Success)
            {
                return (null, new JsonError(parseResult.Exception.Message));
            }

            return (parseResult.Value, null);
        }

        return (null, new JsonError(T.Get("contents.invalidString")));
    }

    public (object? Result, JsonError? Error) Visit(IField<GeolocationFieldProperties> field, Args args)
    {
        var result = GeoJsonValue.TryParse(args.Value, args.Serializer, out var value);

        switch (result)
        {
            case GeoJsonParseResult.InvalidLatitude:
                return (null, new JsonError(T.Get("contents.invalidGeolocationLatitude")));
            case GeoJsonParseResult.InvalidLongitude:
                return (null, new JsonError(T.Get("contents.invalidGeolocationLongitude")));
            case GeoJsonParseResult.InvalidValue:
                return (null, new JsonError(T.Get("contents.invalidGeolocation")));
            default:
                return (value, null);
        }
    }

    private static (object? Result, JsonError? Error) ConvertToIdList(JsonValue value)
    {
        if (value.Value is JsonArray a)
        {
            var result = new List<DomainId>(a.Count);

            foreach (var item in a)
            {
                if (item.Value is string s)
                {
                    if (!string.IsNullOrWhiteSpace(s))
                    {
                        result.Add(DomainId.Create(s));
                        continue;
                    }
                }

                return (null, new JsonError(T.Get("contents.invalidArrayOfStrings")));
            }

            return (result, null);
        }

        return (null, new JsonError(T.Get("contents.invalidArrayOfStrings")));
    }

    private static (object? Result, JsonError? Error) ConvertToStringList(JsonValue value)
    {
        if (value.Value is JsonArray a)
        {
            var result = new List<string?>(a.Count);

            foreach (var item in a)
            {
                if (item.Value is string s)
                {
                    if (!string.IsNullOrWhiteSpace(s))
                    {
                        result.Add(s);
                        continue;
                    }
                }

                return (null, new JsonError(T.Get("contents.invalidArrayOfStrings")));
            }

            return (result, null);
        }

        return (null, new JsonError(T.Get("contents.invalidArrayOfStrings")));
    }

    private static (object? Result, JsonError? Error) ConvertToObjectList(JsonValue value)
    {
        if (value.Value is JsonArray a)
        {
            var result = new List<JsonObject>(a.Count);

            foreach (var item in a)
            {
                if (item.Value is JsonObject o)
                {
                    result.Add(o);
                    continue;
                }

                return (null, new JsonError(T.Get("contents.invalidArrayOfObjects")));
            }

            return (result, null);
        }

        return (null, new JsonError(T.Get("contents.invalidArrayOfObjects")));
    }

    private static (object? Result, JsonError? Error) ConvertToComponentList(JsonValue value,
        ResolvedComponents components, ReadonlyList<DomainId>? allowedIds)
    {
        if (value.Value is JsonArray a)
        {
            var result = new List<Component>(a.Count);

            foreach (var item in a)
            {
                var (component, error) = ConvertToComponent(item, components, allowedIds);

                if (error != null)
                {
                    return (null, error);
                }

                if (component != null)
                {
                    result.Add(component);
                }
            }

            return (result, null);
        }

        return (null, new JsonError(T.Get("contents.invalidArrayOfObjects")));
    }

    private static (Component? Result, JsonError? Error) ConvertToComponent(JsonValue value,
        ResolvedComponents components, ReadonlyList<DomainId>? allowedIds)
    {
        if (value.Value is not JsonObject o)
        {
            return (null, new JsonError(T.Get("contents.invalidComponentNoObject")));
        }

        var id = DomainId.Empty;

        if (o.TryGetValue("schemaName", out var found) && found.Value is string schemaName)
        {
            id = components.FirstOrDefault(x => x.Value.Name == schemaName).Key;

            o.Remove("schemaName");
            o[Component.Discriminator] = id;
        }
        else if (o.TryGetValue(Component.Discriminator, out found) && found.Value is string discriminator)
        {
            id = DomainId.Create(discriminator);
        }
        else if (allowedIds?.Count == 1)
        {
            id = allowedIds[0];

            o[Component.Discriminator] = id;
        }

        if (id == default)
        {
            return (null, new JsonError(T.Get("contents.invalidComponentNoType")));
        }

        if (allowedIds?.Contains(id) == false || !components.TryGetValue(id, out var schema))
        {
            return (null, new JsonError(T.Get("contents.invalidComponentUnknownSchema")));
        }

        var data = new JsonObject(o);

        data.Remove(Component.Discriminator);

        return (new Component(id.ToString(), data, schema), null);
    }
}
