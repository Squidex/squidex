// ==========================================================================
//  JsonExternalSerializer.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System;
using System.IO;
using System.Linq;
using System.Text;
using Newtonsoft.Json;
using Orleans.Runtime;
using Orleans.Serialization;

namespace Squidex.Infrastructure.Json.Orleans
{
    public class JsonExternalSerializer : IExternalSerializer
    {
        private readonly JsonSerializer jsonSerializer;

        public JsonExternalSerializer(JsonSerializer jsonSerializer)
        {
            Guard.NotNull(jsonSerializer, nameof(jsonSerializer));

            this.jsonSerializer = jsonSerializer;
        }

        public void Initialize(Logger logger)
        {
        }

        public bool IsSupportedType(Type itemType)
        {
            return itemType.GetInterfaces().Contains(typeof(IJsonValue));
        }

        public object DeepCopy(object source, ICopyContext context)
        {
            return source;
        }

        public object Deserialize(Type expectedType, IDeserializationContext context)
        {
            var outLength = context.StreamReader.ReadInt();
            var outBytes = context.StreamReader.ReadBytes(outLength);

            var json = Encoding.Default.GetString(outBytes);

            var stream = new MemoryStream(outBytes);

            using (var reader = new JsonTextReader(new StreamReader(stream)))
            {
                return jsonSerializer.Deserialize(reader, expectedType);
            }
        }

        public void Serialize(object item, ISerializationContext context, Type expectedType)
        {
            var stream = new MemoryStream();

            using (var writer = new JsonTextWriter(new StreamWriter(stream)))
            {
                jsonSerializer.Serialize(writer, item);

                writer.Flush();
            }

            var outBytes = stream.ToArray();

            var json = Encoding.Default.GetString(outBytes, 0, outBytes.Length);

            context.StreamWriter.Write(outBytes.Length);
            context.StreamWriter.Write(outBytes);
        }
    }
}
