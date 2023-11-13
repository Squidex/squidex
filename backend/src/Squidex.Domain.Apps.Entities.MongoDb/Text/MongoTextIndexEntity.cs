// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using NetTopologySuite.Geometries;
using Squidex.Infrastructure;
using Squidex.Infrastructure.MongoDb;

namespace Squidex.Domain.Apps.Entities.MongoDb.Text;

public sealed class MongoTextIndexEntity<T>
{
    [BsonId]
    [BsonElement("_id")]
    public ObjectId Id { get; set; }

    [BsonRequired]
    [BsonElement("d")]
    public string DocId { get; set; }

    [BsonRequired]
    [BsonElement("c")]
    [BsonRepresentation(BsonType.Binary)]
    public DomainId ContentId { get; set; }

    [BsonRequired]
    [BsonElement("a")]
    [BsonRepresentation(BsonType.Binary)]
    public DomainId AppId { get; set; }

    [BsonRequired]
    [BsonElement("s")]
    [BsonRepresentation(BsonType.Binary)]
    public DomainId SchemaId { get; set; }

    [BsonRequired]
    [BsonElement("e")]
    public bool ServeAlways { get; set; }

    [BsonRequired]
    [BsonElement("p")]
    public bool ServePublished { get; set; }

    [BsonIgnoreIfNull]
    [BsonElement("t")]
    public T Texts { get; set; }

    [BsonIgnoreIfNull]
    [BsonElement("g")]
    public string GeoField { get; set; }

    [BsonIgnoreIfNull]
    [BsonElement("o")]
    [BsonJson]
    [BsonRepresentation(BsonType.Document)]
    public Geometry GeoObject { get; set; }
}
