// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;
using Squidex.Domain.Apps.Core.Contents;

namespace Squidex.Domain.Apps.Entities.MongoDb.Contents
{
    public sealed class StatusSerializer : SerializerBase<Status>
    {
        public static void Register()
        {
            try
            {
                try
                {
                    BsonSerializer.RegisterSerializer(new StatusSerializer());
                }
                catch (BsonSerializationException)
                {
                    return;
                }
            }
            catch (BsonSerializationException)
            {
                return;
            }
        }

        public override Status Deserialize(BsonDeserializationContext context, BsonDeserializationArgs args)
        {
            var value = context.Reader.ReadString();

            return new Status(value);
        }

        public override void Serialize(BsonSerializationContext context, BsonSerializationArgs args, Status value)
        {
            context.Writer.WriteString(value.Name);
        }
    }
}
