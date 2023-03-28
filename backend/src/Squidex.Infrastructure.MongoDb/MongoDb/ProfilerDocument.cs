// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Squidex.Infrastructure.MongoDb;

[BsonIgnoreExtraElements]
public sealed class ProfilerDocument
{
    public const string CollectionName = "system.profile";

    [BsonElement("op")]
    public string Operation { get; set; }

    [BsonElement("ns")]
    public string Namespace { get; set; }

    [BsonElement("nreturned")]
    public int NumDocuments { get; set; }

    [BsonElement("keysExamined")]
    public int KeysExamined { get; set; }

    [BsonElement("docsExamined")]
    public int DocsExamined { get; set; }

    [BsonElement("planSummary")]
    public string PlanSummary { get; set; }
}
