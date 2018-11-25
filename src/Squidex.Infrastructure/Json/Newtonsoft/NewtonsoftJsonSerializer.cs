// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.IO;
using Newtonsoft.Json;

namespace Squidex.Infrastructure.Json.Newtonsoft
{
    public sealed class NewtonsoftJsonSerializer : IJsonSerializer
    {
        private readonly JsonSerializerSettings settings;
        private readonly JsonSerializer serializer;

        public NewtonsoftJsonSerializer(JsonSerializerSettings settings)
        {
            Guard.NotNull(settings, nameof(settings));

            this.settings = settings;

            serializer = JsonSerializer.Create(settings);
        }

        public T Deserialize<T>(string value, Type actualType = null)
        {
            actualType = actualType ?? typeof(T);

            return (T)JsonConvert.DeserializeObject(value, actualType, settings);
        }

        public T Deserialize<T>(Stream stream, Type actualType = null)
        {
            using (var streamReader = new StreamReader(stream))
            {
                actualType = actualType ?? typeof(T);

                return (T)serializer.Deserialize(streamReader, actualType);
            }
        }

        public string Serialize<T>(T value)
        {
            return JsonConvert.SerializeObject(value, settings);
        }

        public void Serialize<T>(T value, Stream stream)
        {
            using (var writer = new StreamWriter(stream))
            {
                serializer.Serialize(writer, value);

                writer.Flush();
            }
        }
    }
}
