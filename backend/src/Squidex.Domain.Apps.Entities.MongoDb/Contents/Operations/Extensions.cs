// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Driver;
using Squidex.Domain.Apps.Core.Contents;
using Squidex.Infrastructure;

namespace Squidex.Domain.Apps.Entities.MongoDb.Contents.Operations
{
    public static class Extensions
    {
        public sealed class StatusModel
        {
            [BsonId]
            [BsonElement("_id")]
            public DomainId DocumentId { get; set; }

            [BsonRequired]
            [BsonElement("id")]
            public DomainId Id { get; set; }

            [BsonRequired]
            [BsonElement("_si")]
            public DomainId IndexedSchemaId { get; set; }

            [BsonRequired]
            [BsonElement("ss")]
            public Status Status { get; set; }
        }

        public static Task<List<StatusModel>> FindStatusAsync(this IMongoCollection<MongoContentEntity> collection, FilterDefinition<MongoContentEntity> filter,
            CancellationToken ct)
        {
            var projections = Builders<MongoContentEntity>.Projection;

            return collection.Find(filter)
                .Project<StatusModel>(projections
                    .Include(x => x.Id)
                    .Include(x => x.IndexedSchemaId)
                    .Include(x => x.Status))
                .ToListAsync(ct);
        }
    }
}
