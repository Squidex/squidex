// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Globalization;
using NodaTime;
using Squidex.Domain.Apps.Core.Schemas;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Json.Objects;

#pragma warning disable SA1313 // Parameter names should begin with lower-case letter

namespace Squidex.Domain.Apps.Core.DefaultValues;

public sealed class DefaultValueFactory : IFieldPropertiesVisitor<JsonValue, DefaultValueFactory.Args>
{
    private static readonly DefaultValueFactory Instance = new DefaultValueFactory();

    public record struct Args(Instant Now, string Partition);

    private DefaultValueFactory()
    {
    }

    public static JsonValue CreateDefaultValue(IField field, Instant now, string partition)
    {
        Guard.NotNull(field);
        Guard.NotNull(partition);

        var x = field.RawProperties.Accept(Instance, new Args(now, partition));

        return x;
    }

    public JsonValue Visit(ArrayFieldProperties properties, Args args)
    {
        return new JsonArray();
    }

    public JsonValue Visit(AssetsFieldProperties properties, Args args)
    {
        var value = GetDefaultValue(properties.DefaultValue, properties.DefaultValues, args.Partition);

        return Array(value);
    }

    public JsonValue Visit(BooleanFieldProperties properties, Args args)
    {
        var value = GetDefaultValue(properties.DefaultValue, properties.DefaultValues, args.Partition);

        return value ?? JsonValue.Null;
    }

    public JsonValue Visit(ComponentFieldProperties properties, Args args)
    {
        return JsonValue.Null;
    }

    public JsonValue Visit(ComponentsFieldProperties properties, Args args)
    {
        return new JsonArray();
    }

    public JsonValue Visit(GeolocationFieldProperties properties, Args args)
    {
        return JsonValue.Null;
    }

    public JsonValue Visit(JsonFieldProperties properties, Args args)
    {
        return JsonValue.Null;
    }

    public JsonValue Visit(NumberFieldProperties properties, Args args)
    {
        var value = GetDefaultValue(properties.DefaultValue, properties.DefaultValues, args.Partition);

        return value ?? JsonValue.Null;
    }

    public JsonValue Visit(ReferencesFieldProperties properties, Args args)
    {
        var value = GetDefaultValue(properties.DefaultValue, properties.DefaultValues, args.Partition);

        return Array(value);
    }

    public JsonValue Visit(StringFieldProperties properties, Args args)
    {
        var value = GetDefaultValue(properties.DefaultValue, properties.DefaultValues, args.Partition);

        return value;
    }

    public JsonValue Visit(TagsFieldProperties properties, Args args)
    {
        var value = GetDefaultValue(properties.DefaultValue, properties.DefaultValues, args.Partition);

        return Array(value);
    }

    public JsonValue Visit(UIFieldProperties properties, Args args)
    {
        return JsonValue.Null;
    }

    public JsonValue Visit(DateTimeFieldProperties properties, Args args)
    {
        if (properties.CalculatedDefaultValue == DateTimeCalculatedDefaultValue.Now)
        {
            return JsonValue.Create(args.Now);
        }

        if (properties.CalculatedDefaultValue == DateTimeCalculatedDefaultValue.Today)
        {
            return JsonValue.Create($"{args.Now.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture)}T00:00:00Z");
        }

        var value = GetDefaultValue(properties.DefaultValue, properties.DefaultValues, args.Partition);

        return value ?? JsonValue.Null;
    }

    private static T GetDefaultValue<T>(T value, LocalizedValue<T>? values, string partition)
    {
        if (values != null && values.TryGetValue(partition, out var @default))
        {
            return @default;
        }

        return value;
    }

    private static JsonValue Array(IEnumerable<string>? values)
    {
        if (values != null)
        {
            return JsonValue.Array(values);
        }
        else
        {
            return new JsonArray();
        }
    }
}
