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

    public T Deserialize<T>(Stream stream, Type? actualType = null, bool leaveOpen = false)
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
        finally
        {
            if (!leaveOpen)
            {
                stream.Dispose();
            }
        }
    }

    public string Serialize<T>(T value, bool indented = false)
    {
        return Serialize(value, typeof(T), indented);
    }

    public string Serialize(object? value, Type type, bool intented = false)
    {
        try
        {
            var options = intented ? optionsIndented : optionsNormal;

            return JsonSerializer.Serialize(value, type, options);
        }
        catch (SystemJsonException ex)
        {
            ThrowHelper.JsonException(ex.Message, ex);
            return default!;
        }
    }

    public void Serialize<T>(T value, Stream stream, bool leaveOpen = false)
    {
        Serialize(value, typeof(T), stream, leaveOpen);
    }

    public void Serialize(object? value, Type type, Stream stream, bool leaveOpen = false)
    {
        try
        {
            JsonSerializer.Serialize(stream, value, optionsNormal);
        }
        catch (SystemJsonException ex)
        {
            ThrowHelper.JsonException(ex.Message, ex);
        }
        finally
        {
            if (!leaveOpen)
            {
                stream.Dispose();
            }
        }
    }
}
