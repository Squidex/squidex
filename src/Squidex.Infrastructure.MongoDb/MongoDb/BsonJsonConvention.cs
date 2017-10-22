// ==========================================================================
//  BsonJsonConvention.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System.Linq;
using System.Reflection;
using MongoDB.Bson.Serialization.Conventions;
using Newtonsoft.Json;

namespace Squidex.Infrastructure.MongoDb
{
    public static class BsonJsonConvention
    {
        public static void Register(JsonSerializer serializer)
        {
            var pack = new ConventionPack();

            var bsonSerializer = new JsonBsonSerializer(serializer);

            pack.AddMemberMapConvention("JsonBson", memberMap =>
            {
                if (memberMap.MemberType.GetCustomAttributes().OfType<BsonJsonAttribute>().Any())
                {
                    memberMap.SetSerializer(bsonSerializer);
                }
            });

            ConventionRegistry.Register("json", pack, t => true);
        }
    }
}
