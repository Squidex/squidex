// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using Newtonsoft.Json;

namespace Squidex.Infrastructure.Json.Newtonsoft
{
    public sealed class SurrogateConverter<T, TSurrogate> : JsonClassConverter<T> where T : class where TSurrogate : ISurrogate<T>, new()
    {
        protected override T? ReadValue(JsonReader reader, Type objectType, JsonSerializer serializer)
        {
            var surrogate = serializer.Deserialize<TSurrogate>(reader);

            return surrogate?.ToSource();
        }

        protected override void WriteValue(JsonWriter writer, T value, JsonSerializer serializer)
        {
            var surrogate = new TSurrogate();

            surrogate.FromSource(value);

            serializer.Serialize(writer, surrogate);
        }
    }
}
