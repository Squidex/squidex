// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.IO;
using Newtonsoft.Json;
using NewtonsoftException = Newtonsoft.Json.JsonException;

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

        public string Serialize<T>(T value, bool intented = false)
        {
            var formatting = intented ? Formatting.Indented : Formatting.None;

            return JsonConvert.SerializeObject(value, formatting, settings);
        }

        public void Serialize<T>(T value, Stream stream, bool leaveOpen = false)
        {
            try
            {
                using (var writer = new StreamWriter(stream, leaveOpen: leaveOpen))
                {
                    serializer.Serialize(writer, value);

                    writer.Flush();
                }
            }
            catch (NewtonsoftException ex)
            {
                throw new JsonException(ex.Message, ex);
            }
        }

        public T Deserialize<T>(string value, Type? actualType = null)
        {
            try
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
            catch (NewtonsoftException ex)
            {
                throw new JsonException(ex.Message, ex);
            }
        }

        public T Deserialize<T>(Stream stream, Type? actualType = null, bool leaveOpen = false)
        {
            try
            {
                using (var textReader = new StreamReader(stream, leaveOpen: leaveOpen))
                {
                    actualType ??= typeof(T);

                    using (var reader = GetReader(textReader))
                    {
                        return (T)serializer.Deserialize(reader, actualType)!;
                    }
                }
            }
            catch (NewtonsoftException ex)
            {
                throw new JsonException(ex.Message, ex);
            }
        }

        private static JsonTextReader GetReader(TextReader textReader)
        {
            return new JsonTextReader(textReader);
        }
    }
}
