// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Text.Json;
using System.Text.Json.Serialization;

namespace Squidex.Infrastructure.Json.System;

public sealed class SurrogateJsonConverter<T, TSurrogate> : JsonConverter<T> where T : class where TSurrogate : ISurrogate<T>, new()
{
    public override T? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var surrogate = JsonSerializer.Deserialize<TSurrogate>(ref reader, options);

        return surrogate?.ToSource();
    }

    public override void Write(Utf8JsonWriter writer, T value, JsonSerializerOptions options)
    {
        var surrogate = new TSurrogate();

        surrogate.FromSource(value);

        JsonSerializer.Serialize(writer, surrogate, options);
    }
}
