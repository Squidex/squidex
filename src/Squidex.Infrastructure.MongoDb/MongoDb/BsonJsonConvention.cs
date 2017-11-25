// ==========================================================================
//  BsonJsonConvention.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System;
using System.Linq;
using System.Reflection;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Conventions;
using Newtonsoft.Json;

namespace Squidex.Infrastructure.MongoDb
{
    public static class BsonJsonConvention
    {
        public static void Register(JsonSerializer serializer)
        {
            var pack = new ConventionPack();

            pack.AddMemberMapConvention("JsonBson", memberMap =>
            {
                var attributes = memberMap.MemberInfo.GetCustomAttributes();

                if (attributes.OfType<BsonJsonAttribute>().Any())
                {
                    var bsonSerializerType = typeof(BsonJsonSerializer<>).MakeGenericType(memberMap.MemberType);
                    var bsonSerializer = Activator.CreateInstance(bsonSerializerType, serializer);

                    memberMap.SetSerializer((IBsonSerializer)bsonSerializer);
                }
            });

            ConventionRegistry.Register("json", pack, t => true);
        }
    }
}
