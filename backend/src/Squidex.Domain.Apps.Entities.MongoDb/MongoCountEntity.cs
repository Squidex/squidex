// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using MongoDB.Bson.Serialization.Attributes;
using NodaTime;

namespace Squidex.Domain.Apps.Entities.MongoDb;

internal sealed class MongoCountEntity
{
    [BsonId]
    [BsonElement("_id")]
    public string Key { get; set; }

    [BsonRequired]
    [BsonElement(nameof(Count))]
    public long Count { get; set; }

    [BsonRequired]
    [BsonElement(nameof(Created))]
    public Instant Created { get; set; }
}
