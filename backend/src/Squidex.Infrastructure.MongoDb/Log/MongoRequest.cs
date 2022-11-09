// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using NodaTime;

namespace Squidex.Infrastructure.Log;

public sealed class MongoRequest
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

    public static MongoRequest FromRequest(Request request)
    {
        return new MongoRequest { Key = request.Key, Timestamp = request.Timestamp, Properties = request.Properties };
    }

    public Request ToRequest()
    {
        return new Request { Key = Key, Timestamp = Timestamp, Properties = Properties };
    }
}
