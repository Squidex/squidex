// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Driver;
using Squidex.Domain.Apps.Core.Contents;
using Squidex.Infrastructure;
using Squidex.Infrastructure.MongoDb;
using Squidex.Infrastructure.MongoDb.Queries;
using Squidex.Infrastructure.Queries;

namespace Squidex.Domain.Apps.Entities.MongoDb.Contents.Operations;

public static class Extensions
{
    public sealed class StatusOnly
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

    public sealed class IdOnly
    {
        [BsonId]
        [BsonElement("_id")]
        public DomainId Id { get; set; }

        [BsonElement(nameof(Joined))]
        public MongoContentEntity[] Joined { get; set; }
    }

    public static bool IsSatisfiedByIndex(this ClrQuery query)
    {
        return
            query.Sort is { Count: 2 } &&
            query.Sort[0].Path.ToString() == "mt" &&
            query.Sort[0].Order == SortOrder.Descending &&
            query.Sort[1].Path.ToString() == "id" &&
            query.Sort[1].Order == SortOrder.Ascending;
    }

    public static async Task<List<MongoContentEntity>> QueryContentsAsync(this IMongoCollection<MongoContentEntity> collection, FilterDefinition<MongoContentEntity> filter, ClrQuery query,
        CancellationToken ct)
    {
        if (query.Skip > 0 && !query.IsSatisfiedByIndex())
        {
            // If we have to skip over items, we could reach the limit of the sort buffer, therefore get the ids and all filter fields only
            // in a first iteration and get the actual content in the a second query.
            var projection = Builders<MongoContentEntity>.Projection.Include("_id");

            foreach (var field in query.GetAllFields())
            {
                projection = projection.Include(field);
            }

            if (query.Random > 0)
            {
                var ids =
                    await collection.Aggregate()
                        .Match(filter)
                        .Project<IdOnly>(projection)
                        .QuerySort(query)
                        .QuerySkip(query)
                        .QueryLimit(query)
                        .ToListAsync(ct);

                var randomIds = ids.Select(x => x.Id).TakeRandom(query.Random);

                var documents =
                    await collection.Find(Builders<MongoContentEntity>.Filter.In(x => x.Id, randomIds))
                        .ToListAsync(ct);

                return documents.Shuffle().ToList();
            }

            var joined =
                await collection.Aggregate()
                    .Match(filter)
                    .Project<IdOnly>(projection)
                    .QuerySort(query)
                    .QuerySkip(query)
                    .QueryLimit(query)
                    .Lookup<IdOnly, MongoContentEntity, IdOnly>(collection, x => x.Id, x => x.DocumentId, x => x.Joined)
                    .ToListAsync(ct);

            return joined.Select(x => x.Joined[0]).ToList();
        }

        var result =
            collection.Find(filter)
                .QuerySort(query)
                .QuerySkip(query)
                .QueryLimit(query)
                .ToListRandomAsync(collection, query.Random, ct);

        return await result;
    }

    public static Task<List<StatusOnly>> FindStatusAsync(this IMongoCollection<MongoContentEntity> collection,
        FilterDefinition<MongoContentEntity> filter,
        CancellationToken ct)
    {
        var projections = Builders<MongoContentEntity>.Projection;

        return collection.Find(filter)
            .Project<StatusOnly>(projections
                .Include(x => x.Id)
                .Include(x => x.IndexedSchemaId)
                .Include(x => x.Status))
            .ToListAsync(ct);
    }
}
