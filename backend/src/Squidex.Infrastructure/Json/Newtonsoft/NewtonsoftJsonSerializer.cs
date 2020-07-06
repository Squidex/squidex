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

        public string Serialize<T>(T value, bool intented)
        {
            return JsonConvert.SerializeObject(value, intented ? Formatting.Indented : Formatting.None, settings);
        }

        public void Serialize<T>(T value, Stream stream)
        {
            using (var writer = new StreamWriter(stream))
            {
                serializer.Serialize(writer, value);

                writer.Flush();
            }
        }

        public T Deserialize<T>(string value, Type? actualType = null)
        {
            using (var textReader = new StringReader(value))
            {
                actualType ??= typeof(T);

                using (var reader = GetReader(textReader))
                {
                    return (T)serializer.Deserialize(reader, actualType)!;
                }
            }
        }

        public T Deserialize<T>(Stream stream, Type? actualType = null)
        {
            using (var textReader = new StreamReader(stream))
            {
                actualType ??= typeof(T);

                using (var reader = GetReader(textReader))
                {
                    return (T)serializer.Deserialize(reader, actualType)!;
                }
            }
        }

        private static JsonTextReader GetReader(TextReader textReader)
        {
            return new JsonTextReader(textReader);
        }
    }
}
