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

        private sealed class CustomReader : JsonTextReader
        {
            private readonly Func<string, string> stringConverter;

            public override object Value
            {
                get
                {
                    var value = base.Value;

                    if (value is string s)
                    {
                        return stringConverter(s);
                    }

                    return value;
                }
            }

            public CustomReader(TextReader reader, Func<string, string> stringConverter)
                : base(reader)
            {
                this.stringConverter = stringConverter;
            }
        }

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

        public T Deserialize<T>(string value, Type actualType = null, Func<string, string> stringConverter = null)
        {
            using (var textReader = new StringReader(value))
            {
                actualType = actualType ?? typeof(T);

                using (var reader = GetReader(stringConverter, textReader))
                {
                    return (T)serializer.Deserialize(reader, actualType);
                }
            }
        }

        public T Deserialize<T>(Stream stream, Type actualType = null, Func<string, string> stringConverter = null)
        {
            using (var textReader = new StreamReader(stream))
            {
                actualType = actualType ?? typeof(T);

                using (var reader = GetReader(stringConverter, textReader))
                {
                    return (T)serializer.Deserialize(reader, actualType);
                }
            }
        }

        private static JsonTextReader GetReader(Func<string, string> stringConverter, TextReader textReader)
        {
            return stringConverter != null ? new CustomReader(textReader, stringConverter) : new JsonTextReader(textReader);
        }
    }
}
