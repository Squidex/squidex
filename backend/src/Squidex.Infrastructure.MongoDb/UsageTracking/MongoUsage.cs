// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Squidex.Infrastructure.UsageTracking;

public sealed class MongoUsage
{
    [BsonId]
    [BsonElement("_id")]
    public string Id { get; set; }

    [BsonRequired]
    [BsonElement(nameof(Date))]
    [BsonDateTimeOptions(DateOnly = true)]
    public DateTime Date { get; set; }

    [BsonRequired]
    [BsonElement(nameof(Key))]
    public string Key { get; set; }

    [BsonIgnoreIfNull]
    [BsonElement(nameof(Category))]
    public string Category { get; set; }

    [BsonRequired]
    [BsonElement(nameof(Counters))]
    public Counters Counters { get; set; } = new Counters();
}
