// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Linq;
using System.Reflection;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Conventions;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

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
                else if (memberMap.MemberType == typeof(JToken))
                {
                    memberMap.SetSerializer(JTokenSerializer<JToken>.Instance);
                }
                else if (memberMap.MemberType == typeof(JObject))
                {
                    memberMap.SetSerializer(JTokenSerializer<JObject>.Instance);
                }
                else if (memberMap.MemberType == typeof(JValue))
                {
                    memberMap.SetSerializer(JTokenSerializer<JValue>.Instance);
                }
            });

            ConventionRegistry.Register("json", pack, t => true);
        }
    }
}
