// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using MongoDB.Bson.Serialization.Attributes;

namespace Squidex.Infrastructure.Caching;

public sealed class MongoCacheEntry
{
    [BsonId]
    [BsonElement("_id")]
    public string Key { get; set; }

    [BsonRequired]
    [BsonElement(nameof(Expires))]
    public DateTime Expires { get; set; }

    [BsonRequired]
    [BsonElement(nameof(Value))]
    public byte[] Value { get; set; }
}
