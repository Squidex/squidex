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
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Orleans.Runtime;
using Orleans.Serialization;

namespace Squidex.Infrastructure.Json.Orleans
{
    public class JsonExternalSerializer : IExternalSerializer
    {
        private readonly JsonSerializer serializer;

        public JsonExternalSerializer(JsonSerializer serializer)
        {
            Guard.NotNull(serializer, nameof(serializer));

            this.serializer = serializer;
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
            var jsonValue = source as IJsonValue;

            if (jsonValue == null)
            {
                return null;
            }
            else if (jsonValue.IsImmutable)
            {
                return jsonValue;
            }
            else if (jsonValue.Value == null)
            {
                return jsonValue;
            }
            else
            {
                return JObject.FromObject(source, serializer).ToObject(source.GetType(), serializer);
            }
        }

        public object Deserialize(Type expectedType, IDeserializationContext context)
        {
            var outLength = context.StreamReader.ReadInt();
            var outBytes = context.StreamReader.ReadBytes(outLength);

            var stream = new MemoryStream(outBytes);

            using (var reader = new JsonTextReader(new StreamReader(stream)))
            {
                return serializer.Deserialize(reader, expectedType);
            }
        }

        public void Serialize(object item, ISerializationContext context, Type expectedType)
        {
            var stream = new MemoryStream();

            using (var writer = new JsonTextWriter(new StreamWriter(stream)))
            {
                serializer.Serialize(writer, item);

                writer.Flush();
            }

            var outBytes = stream.ToArray();

            context.StreamWriter.Write(outBytes.Length);
            context.StreamWriter.Write(outBytes);
        }
    }
}
