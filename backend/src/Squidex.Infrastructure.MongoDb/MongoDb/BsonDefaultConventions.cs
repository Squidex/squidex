// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Conventions;
using MongoDB.Bson.Serialization.Serializers;

namespace Squidex.Infrastructure.MongoDb;

public static class BsonDefaultConventions
{
    private static bool isRegistered;

    public static void Register()
    {
        try
        {
            if (isRegistered)
            {
                return;
            }

            ConventionRegistry.Register("IgnoreExtraElements", new ConventionPack
            {
                new IgnoreExtraElementsConvention(true)
            }, t => true);

            // Allow all types, independent from the actual assembly.
            BsonSerializer.TryRegisterSerializer(new ObjectSerializer(type => true));

            isRegistered = true;
        }
        catch (BsonSerializationException)
        {
            return;
        }
    }
}
