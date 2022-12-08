// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Squidex.Infrastructure.Json.System;

public sealed class StringConverter<T> : JsonConverter<T> where T : notnull
{
    private readonly Func<string, T> convertFromString;
    private readonly Func<T, string?> convertToString;

    public StringConverter(Func<string, T> convertFromString, Func<T, string?>? convertToString = null)
    {
        this.convertFromString = convertFromString;
        this.convertToString = convertToString ?? (x => x.ToString());
    }

    public StringConverter()
    {
        var typeConverter = TypeDescriptor.GetConverter(typeof(T));

        convertFromString = x => (T)typeConverter.ConvertFromInvariantString(x)!;
        convertToString = x => typeConverter.ConvertToInvariantString(x)!;
    }

    public override T? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        switch (reader.TokenType)
        {
            case JsonTokenType.String:
                var text = reader.GetString();
                try
                {
                    return convertFromString(text!);
                }
                catch (Exception ex)
                {
                    ThrowHelper.JsonException("Error while converting from string.", ex);
                    return default;
                }

            case JsonTokenType.StartObject:
                var optionsWithoutSelf = OptionClones.GetOptionsWithoutConverter(options, this);

                return JsonSerializer.Deserialize<T>(ref reader, optionsWithoutSelf);

            default:
                ThrowHelper.JsonException($"Expected string or object, got {reader.TokenType}.");
                return default;
        }
    }

    public override T ReadAsPropertyName(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        return convertFromString(reader.GetString()!);
    }

    public override void Write(Utf8JsonWriter writer, T value, JsonSerializerOptions options)
    {
        writer.WriteStringValue(convertToString(value));
    }

    public override void WriteAsPropertyName(Utf8JsonWriter writer, T value, JsonSerializerOptions options)
    {
        writer.WritePropertyName(convertToString(value)!);
    }
}

#pragma warning disable MA0048 // File name must match type name
internal static class OptionClones
#pragma warning restore MA0048 // File name must match type name
{
    private static readonly ConditionalWeakTable<JsonSerializerOptions, JsonSerializerOptions> Clones = new ConditionalWeakTable<JsonSerializerOptions, JsonSerializerOptions>();

    public static JsonSerializerOptions GetOptionsWithoutConverter(JsonSerializerOptions source, JsonConverter converter)
    {
        if (!source.Converters.Contains(converter))
        {
            return source;
        }

        if (!Clones.TryGetValue(source, out var clone))
        {
            clone = new JsonSerializerOptions(source);

            // Remove the current converter, otherwise we would create a stackoverflow exception.
            clone.Converters.Remove(converter);

            Clones.TryAdd(source, clone);
        }

        return clone;
    }
}
