// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Threading;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;

namespace Squidex.Infrastructure.MongoDb
{
    public class RefTokenSerializer : ClassSerializerBase<RefToken>
    {
        private static int isRegistered;

        public static void Register()
        {
            if (Interlocked.Increment(ref isRegistered) == 1)
            {
                try
                {
                    BsonSerializer.RegisterSerializer(new RefTokenSerializer());
                }
                catch (BsonSerializationException)
                {
                    return;
                }
            }
        }

        protected override RefToken DeserializeValue(BsonDeserializationContext context, BsonDeserializationArgs args)
        {
            var value = context.Reader.ReadString();

            return RefToken.Parse(value);
        }

        protected override void SerializeValue(BsonSerializationContext context, BsonSerializationArgs args, RefToken value)
        {
            context.Writer.WriteString(value.ToString());
        }
    }
}
