// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Globalization;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Driver;
using Squidex.Infrastructure.States;

namespace Squidex.Infrastructure.MongoDb;

public static class MongoExtensions
{
    private static readonly ReplaceOptions UpsertReplace = new ReplaceOptions { IsUpsert = true };

    public static async Task<bool> CollectionExistsAsync(this IMongoDatabase database, string collectionName,
        CancellationToken ct = default)
    {
        var options = new ListCollectionNamesOptions
        {
            Filter = new BsonDocument("name", collectionName)
        };

        var collections = await database.ListCollectionNamesAsync(options, ct);

        return await collections.AnyAsync(ct);
    }

    public static Task<bool> AnyAsync<T>(this IMongoCollection<T> collection,
        CancellationToken ct = default)
    {
        var find = collection.Find(new BsonDocument()).Limit(1);

        return find.AnyAsync(ct);
    }

    public static async Task<bool> InsertOneIfNotExistsAsync<T>(this IMongoCollection<T> collection, T document,
        CancellationToken ct = default)
    {
        try
        {
            await collection.InsertOneAsync(document, null, ct);
        }
        catch (MongoWriteException ex) when (ex.WriteError?.Category == ServerErrorCategory.DuplicateKey)
        {
            return false;
        }

        return true;
    }

    public static async IAsyncEnumerable<T> ToAsyncEnumerable<T>(this IFindFluent<T, T> find,
        [EnumeratorCancellation] CancellationToken ct = default)
    {
        using var cursor = await find.ToCursorAsync(ct);

        while (await cursor.MoveNextAsync(ct))
        {
            foreach (var item in cursor.Current)
            {
                ct.ThrowIfCancellationRequested();

                yield return item;
            }
        }
    }

    public static IFindFluent<T, BsonDocument> Only<T>(this IFindFluent<T, T> find,
        Expression<Func<T, object?>> include)
    {
        return find.Project<BsonDocument>(Builders<T>.Projection.Include(include));
    }

    public static IFindFluent<T, BsonDocument> Only<T>(this IFindFluent<T, T> find,
        Expression<Func<T, object?>> include1,
        Expression<Func<T, object?>> include2)
    {
        return find.Project<BsonDocument>(Builders<T>.Projection.Include(include1).Include(include2));
    }

    public static IFindFluent<T, BsonDocument> Only<T>(this IFindFluent<T, T> find,
        Expression<Func<T, object?>> include1,
        Expression<Func<T, object?>> include2,
        Expression<Func<T, object?>> include3)
    {
        return find.Project<BsonDocument>(Builders<T>.Projection.Include(include1).Include(include2).Include(include3));
    }

    public static IFindFluent<T, T> Not<T>(this IFindFluent<T, T> find,
        Expression<Func<T, object?>> exclude)
    {
        return find.Project<T>(Builders<T>.Projection.Exclude(exclude));
    }

    public static IFindFluent<T, T> Not<T>(this IFindFluent<T, T> find,
        Expression<Func<T, object?>> exclude1,
        Expression<Func<T, object?>> exclude2)
    {
        return find.Project<T>(Builders<T>.Projection.Exclude(exclude1).Exclude(exclude2));
    }

    public static long ToLong(this BsonValue value)
    {
        switch (value.BsonType)
        {
            case BsonType.Int32:
                return value.AsInt32;
            case BsonType.Int64:
                return value.AsInt64;
            case BsonType.Double:
                return (long)value.AsDouble;
            default:
                throw new InvalidCastException($"Cannot cast from {value.BsonType} to long.");
        }
    }

    public static async Task<bool> UpsertVersionedAsync<T>(this IMongoCollection<T> collection, IClientSessionHandle session, SnapshotWriteJob<T> job, string versionField,
        CancellationToken ct = default)
        where T : IVersionedEntity<DomainId>
    {
        var filters = Builders<T>.Filter;

        var (key, snapshot, _, oldVersion) = job;
        try
        {
            var filter = filters.Eq("_id", key);

            if (oldVersion > EtagVersion.Any)
            {
                filter = filters.And(filters.Eq(versionField, oldVersion));
            }

            var result = await collection.ReplaceOneAsync(session, filter, job.Value, UpsertReplace, ct);

            return result.IsAcknowledged && result.ModifiedCount == 1;
        }
        catch (MongoWriteException ex) when (ex.WriteError?.Category == ServerErrorCategory.DuplicateKey)
        {
            var existingVersion =
                await collection.Find(session, filters.Eq("_id", key)).Project<BsonDocument>(Builders<T>.Projection.Include("_id").Include(versionField))
                    .FirstOrDefaultAsync(ct);

            if (existingVersion != null)
            {
                var field = Field.Of<T>(x => nameof(x.Version));

                throw new InconsistentStateException(existingVersion[field].AsInt64, oldVersion);
            }
            else
            {
                throw new InconsistentStateException(EtagVersion.Any, oldVersion);
            }
        }
    }

    public static async Task<bool> UpsertVersionedAsync<T>(this IMongoCollection<T> collection, SnapshotWriteJob<T> job, string versionField,
        CancellationToken ct = default)
        where T : IVersionedEntity<DomainId>
    {
        var filters = Builders<T>.Filter;

        var (key, snapshot, _, oldVersion) = job;
        try
        {
            Expression<Func<T, bool>> filter2 =
                oldVersion > EtagVersion.Any ?
                x => x.DocumentId.Equals(key) && x.Version == oldVersion :
                x => x.DocumentId.Equals(key);

            var filter = filters.Eq(x => x.DocumentId, key);

            if (oldVersion > EtagVersion.Any)
            {
                filter = filters.And(filter, filters.Eq(versionField, oldVersion));
            }

            var rendered =
                filter.Render(
                    BsonSerializer.SerializerRegistry.GetSerializer<T>(),
                    BsonSerializer.SerializerRegistry)
                .ToString();

            var result = await collection.ReplaceOneAsync(filter, job.Value, UpsertReplace, ct);

            return result.IsAcknowledged && result.ModifiedCount == 1;
        }
        catch (MongoWriteException ex) when (ex.WriteError?.Category == ServerErrorCategory.DuplicateKey)
        {
            var existingVersion =
                await collection.Find(filters.Eq("_id", key)).Project<BsonDocument>(Builders<T>.Projection.Include("_id").Include(versionField))
                    .FirstOrDefaultAsync(ct);

            if (existingVersion != null)
            {
                var field = Field.Of<T>(x => nameof(x.Version));

                throw new InconsistentStateException(existingVersion[field].AsInt64, oldVersion);
            }
            else
            {
                throw new InconsistentStateException(EtagVersion.Any, oldVersion);
            }
        }
    }

    public static async Task<int> GetMajorVersionAsync(this IMongoDatabase database,
        CancellationToken ct = default)
    {
        var command =
            new BsonDocumentCommand<BsonDocument>(new BsonDocument
            {
                { "buildInfo", 1 }
            });

        var document = await database.RunCommandAsync(command, cancellationToken: ct);

        var versionString = document["version"].AsString;
        var versionMajor = versionString.Split('.')[0];

        int.TryParse(versionMajor, NumberStyles.Integer, CultureInfo.InvariantCulture, out int result);

        return result;
    }

    public static async Task<List<T>> ToListRandomAsync<T>(this IFindFluent<T, T> find, IMongoCollection<T> collection, long take,
        CancellationToken ct = default)
    {
        if (take <= 0)
        {
            return await find.ToListAsync(ct);
        }

        var idDocuments = await find.Project<BsonDocument>(Builders<T>.Projection.Include("_id")).ToListAsync(ct);
        var idValues = idDocuments.Select(x => x["_id"]);

        var randomIds = idValues.TakeRandom(take);

        var documents = await collection.Find(Builders<T>.Filter.In("_id", randomIds)).ToListAsync(ct);

        return documents.Shuffle().ToList();
    }

    public static async Task<List<T>> ToListRandomAsync<T>(this IAggregateFluent<T> find, IMongoCollection<T> collection, long take,
        CancellationToken ct = default)
    {
        if (take <= 0)
        {
            return await find.ToListAsync(ct);
        }

        var idDocuments = await find.Project<BsonDocument>(Builders<T>.Projection.Include("_id")).ToListAsync(ct);
        var idsOrdered = idDocuments.Select(x => x["_id"]);
        var idsRandom = idsOrdered.TakeRandom(take);

        var documents = await collection.Find(Builders<T>.Filter.In("_id", idsRandom)).ToListAsync(ct);

        return documents.Shuffle().ToList();
    }
}
