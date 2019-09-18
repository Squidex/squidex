// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Confluent.Kafka;
using Newtonsoft.Json;
using Squidex.ICIS.Kafka.Entities;
using System;
using System.IO;

namespace Squidex.ICIS.Kafka
{
    public sealed class KafkaJsonSerializer<T> : ISerializer<T>, IDeserializer<T> where T : class
    {
        private static readonly JsonSerializer jsonSerializer = JsonSerializer.CreateDefault();
        private readonly Type type;

        public KafkaJsonSerializer(Type type)
        {
            this.type = type;
        }
            
        public T Deserialize(ReadOnlySpan<byte> data, bool isNull, SerializationContext context)
        {
            using (var stream = new MemoryStream(data.ToArray()))
            {
                using (var reader = new StreamReader(stream))
                {
                    using (var jsonReader = new JsonTextReader(reader))
                    {
                        return jsonSerializer.Deserialize(jsonReader, type) as T;
                    }
                }
            }
        }

        public byte[] Serialize(T data, SerializationContext context)
        {
            using (var stream = new MemoryStream())
            {
                using (var writer = new StreamWriter(stream))
                {
                    using (var jsonWriter = new JsonTextWriter(writer))
                    {
                        jsonSerializer.Serialize(jsonWriter, data, type);
                    }
                }

                return stream.ToArray();
            }
        }
    }
}
