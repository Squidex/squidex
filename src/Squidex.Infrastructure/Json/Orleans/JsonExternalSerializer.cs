// ==========================================================================
//  JsonExternalSerializer.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Orleans.Runtime;
using Orleans.Serialization;

namespace Squidex.Infrastructure.Json.Orleans
{
    public class JsonExternalSerializer : IExternalSerializer
    {
        private readonly JsonSerializer serializer;
        private readonly HashSet<Type> types;

        public JsonExternalSerializer(JsonSerializer serializer, params Type[] types)
        {
            Guard.NotNull(serializer, nameof(serializer));

            this.serializer = serializer;

            this.types = new HashSet<Type>(types);
        }

        public void Initialize(Logger logger)
        {
        }

        public bool IsSupportedType(Type itemType)
        {
            return types.Contains(itemType);
        }

        public object DeepCopy(object source, ICopyContext context)
        {
            if (source == null)
            {
                return null;
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
