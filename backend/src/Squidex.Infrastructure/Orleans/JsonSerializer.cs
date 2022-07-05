// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Orleans.Serialization;
using Squidex.Infrastructure.Json;

namespace Squidex.Infrastructure.Orleans
{
    public sealed class JsonSerializer : IExternalSerializer
    {
        private readonly IJsonSerializer jsonSerializer;

        public JsonSerializer(IJsonSerializer jsonSerializer)
        {
            this.jsonSerializer = jsonSerializer;
        }

        public bool IsSupportedType(Type itemType)
        {
            return itemType.Namespace?.StartsWith("Squidex", StringComparison.OrdinalIgnoreCase) == true;
        }

        public object DeepCopy(object source, ICopyContext context)
        {
            return source;
        }

        public object Deserialize(Type expectedType, IDeserializationContext context)
        {
            var stream = new StreamReaderWrapper(context.StreamReader);

            return jsonSerializer.Deserialize<object>(stream, expectedType);
        }

        public void Serialize(object item, ISerializationContext context, Type expectedType)
        {
            var stream = new StreamWriterWrapper(context.StreamWriter);

            jsonSerializer.Serialize(item, stream);
        }
    }
}
