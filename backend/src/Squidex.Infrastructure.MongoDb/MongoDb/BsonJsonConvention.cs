// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Reflection;
using System.Text.Json;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Conventions;

namespace Squidex.Infrastructure.MongoDb
{
    public static class BsonJsonConvention
    {
        public static void Register(JsonSerializerOptions options)
        {
            try
            {
                var pack = new ConventionPack();

                pack.AddMemberMapConvention("JsonBson", memberMap =>
                {
                    var attributes = memberMap.MemberInfo.GetCustomAttributes();

                    if (attributes.OfType<BsonJsonAttribute>().Any())
                    {
                        var bsonSerializerType = typeof(BsonJsonSerializer<>).MakeGenericType(memberMap.MemberType);
                        var bsonSerializer = Activator.CreateInstance(bsonSerializerType, options);

                        memberMap.SetSerializer((IBsonSerializer)bsonSerializer!);
                    }
                });

                ConventionRegistry.Register("json", pack, t => true);
            }
            catch (BsonSerializationException)
            {
                return;
            }
        }
    }
}
