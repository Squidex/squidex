// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Squidex.Infrastructure.Migrations;

public sealed class MongoMigrationEntity
{
    [BsonId]
    [BsonElement("_id")]
    public string Id { get; set; }

    [BsonRequired]
    [BsonElement(nameof(IsLocked))]
    public bool IsLocked { get; set; }

    [BsonElement]
    [BsonRequired]
    public int Version { get; set; }
}
