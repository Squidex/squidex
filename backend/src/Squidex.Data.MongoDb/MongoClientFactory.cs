// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Text.Json;
using MongoDB.Bson;
using MongoDB.Driver;
using Squidex.Domain.Apps.Core.Contents;
using Squidex.Domain.Apps.Entities;
using Squidex.Domain.Apps.Entities.Assets;
using Squidex.Domain.Apps.Entities.Contents;
using Squidex.Domain.Apps.Entities.Contents.Text;
using Squidex.Domain.Apps.Entities.History;
using Squidex.Domain.Apps.Entities.MongoDb;
using Squidex.Domain.Users;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Json.Objects;

namespace Squidex;

public static class MongoClientFactory
{
    public static void SetupSerializer(JsonSerializerOptions jsonSerializerOptions, BsonType representation)
    {
        // Register the serializers first.
        BsonDomainIdSerializer.Register();
        BsonEscapedDictionarySerializer<JsonValue, JsonObject>.Register();
        BsonEscapedDictionarySerializer<JsonValue, ContentFieldData>.Register();
        BsonEscapedDictionarySerializer<ContentFieldData, ContentData>.Register();
        BsonInstantSerializer.Register();
        BsonJsonValueSerializer.Register();
        BsonStringSerializer<RefToken>.Register();
        BsonStringSerializer<Status>.Register();
        BsonUniqueContentIdSerializer.Register();
        BsonJsonConvention.Register(jsonSerializerOptions, representation);

        // Keep the order, because we have an inheritance between entities.
        MongoEntityClassMap.RegisterClassMap();
        MongoAppEntityClassMap.RegisterClassMap();
        MongoAssetItemClassMap.RegisterClassMap();
        MongoAssetEntity.RegisterClassMap();
        MongoAssetFolderEntity.RegisterClassMap();
        MongoContentEntity.RegisterClassMap();
        MongoHistoryClassMap.RegisterClassMap();
        MongoIdentityClassMap.RegisterClassMap();
        MongoTextStateClassMap.RegisterClassMap();

        BsonDefaultConventions.Register();
    }

    public static MongoClient Create(string? connectionString, Action<MongoClientSettings>? configure = null)
    {
        var clientSettings = MongoClientSettings.FromConnectionString(connectionString);

        // If we really need custom config.
        configure?.Invoke(clientSettings);

        return new MongoClient(clientSettings);
    }
}
