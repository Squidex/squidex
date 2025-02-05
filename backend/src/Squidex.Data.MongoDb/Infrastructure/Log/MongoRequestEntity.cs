// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using NodaTime;
using Squidex.Infrastructure.Reflection;

namespace Squidex.Infrastructure.Log;

public sealed class MongoRequestEntity
{
    [BsonId]
    [BsonElement("_id")]
    public ObjectId Id { get; set; }

    [BsonRequired]
    [BsonElement(nameof(Key))]
    public string Key { get; set; }

    [BsonRequired]
    [BsonElement(nameof(Timestamp))]
    public Instant Timestamp { get; set; }

    [BsonRequired]
    [BsonElement(nameof(Properties))]
    public Dictionary<string, string> Properties { get; set; }

    public static MongoRequestEntity FromRequest(Request request)
    {
        return SimpleMapper.Map(request, new MongoRequestEntity());
    }

    public Request ToRequest()
    {
        return SimpleMapper.Map(this, new Request());
    }
}
