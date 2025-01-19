// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Runtime.CompilerServices;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Driver;
using Squidex.Domain.Apps.Core.Contents;
using Squidex.Infrastructure;

namespace Squidex.Domain.Apps.Entities.Contents.Operations;

public sealed class QueryAsStream : OperationBase
{
    public sealed class IdOnly
    {
        [BsonElement("id")]
        public DomainId Id { get; set; }
    }

    public async IAsyncEnumerable<Content> StreamAll(DomainId appId, HashSet<DomainId>? schemaIds,
        [EnumeratorCancellation] CancellationToken ct)
    {
        var filter = CreateFilter(appId, schemaIds);

        using (var cursor = await Collection.Find(filter).SelectFields(null).ToCursorAsync(ct))
        {
            while (await cursor.MoveNextAsync(ct))
            {
                foreach (var entity in cursor.Current)
                {
                    yield return entity;
                }
            }
        }
    }

    public async IAsyncEnumerable<DomainId> StreamAllIds(DomainId appId, DomainId schemaId,
        [EnumeratorCancellation] CancellationToken ct)
    {
        var filter = CreateFilter(appId, [schemaId]);

        // Only query the ID from the database to improve performance.
        var projection = Builders<MongoContentEntity>.Projection.Include(x => x.Id);

        using (var cursor = await Collection.Find(filter).Project<IdOnly>(projection).ToCursorAsync(ct))
        {
            while (await cursor.MoveNextAsync(ct))
            {
                foreach (var entity in cursor.Current)
                {
                    yield return entity.Id;
                }
            }
        }
    }

    private static FilterDefinition<MongoContentEntity> CreateFilter(DomainId appId, HashSet<DomainId>? schemaIds)
    {
        var filters = new List<FilterDefinition<MongoContentEntity>>
        {
            Filter.Gt(x => x.LastModified, default),
            Filter.Gt(x => x.Id, default),
            Filter.Eq(x => x.IndexedAppId, appId)
        };

        if (schemaIds != null)
        {
            filters.Add(Filter.In(x => x.IndexedSchemaId, schemaIds));
        }
        else
        {
            // If we also add this filter, it is more likely that the index will be used.
            filters.Add(Filter.Exists(x => x.IndexedSchemaId));
        }

        filters.Add(Filter.Ne(x => x.IsDeleted, true));

        return Filter.And(filters);
    }
}
