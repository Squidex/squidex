// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.ComponentModel;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;

namespace Squidex.Infrastructure.MongoDb
{
    public sealed class TypeConverterStringSerializer<T> : SerializerBase<T>
    {
        private readonly TypeConverter typeConverter;

        public static void Register()
        {
            try
            {
                BsonSerializer.RegisterSerializer(new TypeConverterStringSerializer<T>());
            }
            catch (BsonSerializationException)
            {
                return;
            }
        }

        public TypeConverterStringSerializer()
        {
            typeConverter = TypeDescriptor.GetConverter(typeof(T));
        }

        public override T Deserialize(BsonDeserializationContext context, BsonDeserializationArgs args)
        {
            var value = context.Reader.ReadString();

            return (T)typeConverter.ConvertFromInvariantString(value);
        }

        public override void Serialize(BsonSerializationContext context, BsonSerializationArgs args, T value)
        {
            context.Writer.WriteString(value!.ToString());
        }
    }
}
