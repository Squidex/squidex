// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Threading;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;
using Squidex.Domain.Apps.Core.Contents;

namespace Squidex.Domain.Apps.Entities.MongoDb.Contents
{
    public sealed class StatusSerializer : SerializerBase<Status2>
    {
        private static volatile int isRegistered;

        public static void Register()
        {
            if (Interlocked.Increment(ref isRegistered) == 1)
            {
                BsonSerializer.RegisterSerializer(new StatusSerializer());
            }
        }

        public override Status2 Deserialize(BsonDeserializationContext context, BsonDeserializationArgs args)
        {
            var value = context.Reader.ReadString();

            return new Status2(value);
        }

        public override void Serialize(BsonSerializationContext context, BsonSerializationArgs args, Status2 value)
        {
            context.Writer.WriteString(value.Name);
        }
    }
}
