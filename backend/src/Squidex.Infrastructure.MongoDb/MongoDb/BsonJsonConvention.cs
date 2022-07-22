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
        public static JsonSerializerOptions Options { get; set; } = new JsonSerializerOptions(JsonSerializerDefaults.Web);

        public static void Register(JsonSerializerOptions? options = null)
        {
            try
            {
                if (options != null)
                {
                    Options = options;
                }

                var pack = new ConventionPack();

                pack.AddMemberMapConvention("JsonBson", memberMap =>
                {
                    var attributes = memberMap.MemberInfo.GetCustomAttributes();

                    if (attributes.OfType<BsonJsonAttribute>().Any())
                    {
                        var bsonSerializerType = typeof(BsonJsonSerializer<>).MakeGenericType(memberMap.MemberType);
                        var bsonSerializer = Activator.CreateInstance(bsonSerializerType);

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
