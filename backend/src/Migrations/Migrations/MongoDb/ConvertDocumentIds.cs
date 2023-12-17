// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using MongoDB.Bson;
using MongoDB.Driver;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Migrations;
using Squidex.Infrastructure.MongoDb;
using Squidex.Infrastructure.Tasks;

namespace Migrations.Migrations.MongoDb;

public sealed class ConvertDocumentIds : MongoBase<BsonDocument>, IMigration
{
    private readonly IMongoDatabase databaseDefault;
    private readonly IMongoDatabase databaseContent;
    private Scope scope;

    private enum Scope
    {
        None,
        Assets,
        Contents
    }

    public ConvertDocumentIds(IMongoDatabase databaseDefault, IMongoDatabase databaseContent)
    {
        this.databaseDefault = databaseDefault;
        this.databaseContent = databaseContent;
    }

    public override string ToString()
    {
        return $"{base.ToString()}({scope})";
    }

    public ConvertDocumentIds ForContents()
    {
        scope = Scope.Contents;

        return this;
    }

    public ConvertDocumentIds ForAssets()
    {
        scope = Scope.Assets;

        return this;
    }

    public async Task UpdateAsync(
        CancellationToken ct)
    {
        switch (scope)
        {
            case Scope.Assets:
                await RebuildAsync(databaseDefault, ConvertParentId, "States_Assets", ct);
                await RebuildAsync(databaseDefault, ConvertParentId, "States_AssetFolders", ct);
                break;
            case Scope.Contents:
                await RebuildAsync(databaseContent, null, "State_Contents_All", ct);
                await RebuildAsync(databaseContent, null, "State_Contents_Published", ct);
                break;
        }
    }

    private static async Task RebuildAsync(IMongoDatabase database, Action<BsonDocument>? extraAction, string collectionNameV1,
        CancellationToken ct)
    {
        string collectionNameV2;

        collectionNameV2 = $"{collectionNameV1}2";
        collectionNameV2 = collectionNameV2.Replace("State_", "States_", StringComparison.Ordinal);

        // Do not resolve in constructor, because most of the time it is not executed anyway.
        var collectionV1 = database.GetCollection<BsonDocument>(collectionNameV1);
        var collectionV2 = database.GetCollection<BsonDocument>(collectionNameV2);

        if (!await collectionV1.AnyAsync(ct: ct))
        {
            return;
        }

        await collectionV2.DeleteManyAsync(FindAll, ct);

        // Run batch first, because it is cheaper as it has less items.
        var batches = collectionV1.Find(FindAll).ToAsyncEnumerable(ct).Batch(500, ct).Buffered(2, ct);

        await Parallel.ForEachAsync(batches, ct, async (batch, ct) =>
        {
            var writes = new List<WriteModel<BsonDocument>>();

            foreach (var document in batch)
            {
                var appId = document["_ai"].AsString;

                var documentIdOld = document["_id"].AsString;

                if (documentIdOld.Contains("--", StringComparison.OrdinalIgnoreCase))
                {
                    var index = documentIdOld.LastIndexOf("--", StringComparison.OrdinalIgnoreCase);

                    documentIdOld = documentIdOld[(index + 2)..];
                }

                var documentIdNew = DomainId.Combine(DomainId.Create(appId), DomainId.Create(documentIdOld)).ToString();

                document["id"] = documentIdOld;
                document["_id"] = documentIdNew;

                extraAction?.Invoke(document);

                var filter = Filter.Eq("_id", documentIdNew);

                writes.Add(new ReplaceOneModel<BsonDocument>(filter, document)
                {
                    IsUpsert = true
                });
            }

            if (writes.Count > 0)
            {
                await collectionV2.BulkWriteAsync(writes, BulkUnordered, ct);
            }
        });
    }

    private static void ConvertParentId(BsonDocument document)
    {
        if (document.Contains("pi"))
        {
            document["pi"] = document["pi"].AsGuid.ToString();
        }
    }
}
