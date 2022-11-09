// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using MongoDB.Bson.Serialization.Attributes;
using NodaTime;
using Squidex.Infrastructure;

namespace Squidex.Domain.Apps.Entities.MongoDb.Schemas;

public sealed class MongoSchemasHashEntity
{
    [BsonId]
    [BsonElement("_id")]
    public DomainId AppId { get; set; }

    [BsonRequired]
    [BsonElement("s")]
    public Dictionary<string, long> SchemaVersions { get; set; }

    [BsonRequired]
    [BsonElement("t")]
    public Instant Updated { get; set; }
}
