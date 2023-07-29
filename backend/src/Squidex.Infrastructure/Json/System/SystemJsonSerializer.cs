// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Text.Json;
using SystemJsonException = System.Text.Json.JsonException;

namespace Squidex.Infrastructure.Json.System;

public sealed class SystemJsonSerializer : IJsonSerializer
{
    private readonly JsonSerializerOptions optionsNormal;
    private readonly JsonSerializerOptions optionsIndented;

    public SystemJsonSerializer(JsonSerializerOptions options)
    {
        optionsNormal = new JsonSerializerOptions(options)
        {
            WriteIndented = false
        };

        optionsIndented = new JsonSerializerOptions(options)
        {
            WriteIndented = true
        };
    }

    public T Deserialize<T>(string value, Type? actualType = null)
    {
        try
        {
            return (T)JsonSerializer.Deserialize(value, actualType ?? typeof(T), optionsNormal)!;
        }
        catch (SystemJsonException ex)
        {
            ThrowHelper.JsonException(ex.Message, ex);
            return default!;
        }
    }

    public T Deserialize<T>(Stream stream, Type? actualType = null)
    {
        try
        {
            return (T)JsonSerializer.Deserialize(stream, actualType ?? typeof(T), optionsNormal)!;
        }
        catch (SystemJsonException ex)
        {
            ThrowHelper.JsonException(ex.Message, ex);
            return default!;
        }
    }

    public string Serialize<T>(T value, bool indented = false)
    {
        return Serialize(value, typeof(T), indented);
    }

    public string Serialize(object? value, Type type, bool indented = false)
    {
        try
        {
            var options = indented ? optionsIndented : optionsNormal;

            return JsonSerializer.Serialize(value, type, options);
        }
        catch (SystemJsonException ex)
        {
            ThrowHelper.JsonException(ex.Message, ex);
            return default!;
        }
    }

    public byte[] SerializeToBytes<T>(T value, bool indented = false)
    {
        return SerializeToBytes(value, typeof(T), indented);
    }

    public byte[] SerializeToBytes(object? value, Type type, bool indented = false)
    {
        try
        {
            var options = indented ? optionsIndented : optionsNormal;

            return JsonSerializer.SerializeToUtf8Bytes(value, type, options);
        }
        catch (SystemJsonException ex)
        {
            ThrowHelper.JsonException(ex.Message, ex);
            return default!;
        }
    }

    public void Serialize<T>(T value, Stream stream, bool indented = false)
    {
        Serialize(value, typeof(T), stream, indented);
    }

    public void Serialize(object? value, Type type, Stream stream, bool indented = false)
    {
        try
        {
            var options = indented ? optionsIndented : optionsNormal;

            JsonSerializer.Serialize(stream, value, optionsNormal);
        }
        catch (SystemJsonException ex)
        {
            ThrowHelper.JsonException(ex.Message, ex);
        }
    }

    public Task SerializeAsync<T>(T value, Stream stream, bool indented = false,
        CancellationToken ct = default)
    {
        return SerializeAsync(value, typeof(T), stream, indented, ct);
    }

    public async Task SerializeAsync(object? value, Type type, Stream stream, bool indented = false,
        CancellationToken ct = default)
    {
        try
        {
            var options = indented ? optionsIndented : optionsNormal;

            await JsonSerializer.SerializeAsync(stream, value, optionsNormal, ct);
        }
        catch (SystemJsonException ex)
        {
            ThrowHelper.JsonException(ex.Message, ex);
        }
    }
}
