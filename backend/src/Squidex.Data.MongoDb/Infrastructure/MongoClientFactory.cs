// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using MongoDB.Driver;
using Squidex.Infrastructure.Json.Objects;

namespace Squidex.Infrastructure;

public static class MongoClientFactory
{
    public static MongoClient Create(string? connectionString, Action<MongoClientSettings>? configure = null)
    {
        BsonDefaultConventions.Register();
        BsonDomainIdSerializer.Register();
        BsonEscapedDictionarySerializer<JsonValue, JsonObject>.Register();
        BsonInstantSerializer.Register();
        BsonJsonValueSerializer.Register();
        BsonStringSerializer<RefToken>.Register();

        var clientSettings = MongoClientSettings.FromConnectionString(connectionString);

        // If we really need custom config.
        configure?.Invoke(clientSettings);

        return new MongoClient(clientSettings);
    }
}
