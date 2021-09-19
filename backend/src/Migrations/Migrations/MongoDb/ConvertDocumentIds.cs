// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using MongoDB.Bson;
using MongoDB.Driver;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Migrations;
using Squidex.Infrastructure.MongoDb;
using Squidex.Infrastructure.Tasks;

namespace Migrations.Migrations.MongoDb
{
    public sealed class ConvertDocumentIds : IMigration
    {
        private readonly IMongoDatabase database;
        private readonly IMongoDatabase databaseContent;
        private Scope scope;

        private enum Scope
        {
            None,
            Assets,
            Contents
        }

        public ConvertDocumentIds(IMongoDatabase database, IMongoDatabase databaseContent)
        {
            this.database = database;
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
                    await RebuildAsync(database, ConvertParentId, "States_Assets", ct);
                    await RebuildAsync(database, ConvertParentId, "States_AssetFolders", ct);
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
            const int SizeOfBatch = 1000;
            const int SizeOfQueue = 10;

            string collectionNameV2;

            collectionNameV2 = $"{collectionNameV1}2";
            collectionNameV2 = collectionNameV2.Replace("State_", "States_", StringComparison.Ordinal);

            var collectionV1 = database.GetCollection<BsonDocument>(collectionNameV1);
            var collectionV2 = database.GetCollection<BsonDocument>(collectionNameV2);

            if (!await collectionV1.AnyAsync(ct: ct))
            {
                return;
            }

            await collectionV2.DeleteManyAsync(new BsonDocument(), ct);

            var batchBlock = new BatchBlock<BsonDocument>(SizeOfBatch, new GroupingDataflowBlockOptions
            {
                BoundedCapacity = SizeOfQueue * SizeOfBatch
            });

            var writeOptions = new BulkWriteOptions
            {
                IsOrdered = false
            };

            var actionBlock = new ActionBlock<BsonDocument[]>(async batch =>
            {
                try
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

                        var filter = Builders<BsonDocument>.Filter.Eq("_id", documentIdNew);

                        writes.Add(new ReplaceOneModel<BsonDocument>(filter, document)
                        {
                            IsUpsert = true
                        });
                    }

                    if (writes.Count > 0)
                    {
                        await collectionV2.BulkWriteAsync(writes, writeOptions, ct);
                    }
                }
                catch (OperationCanceledException ex)
                {
                    // Dataflow swallows operation cancelled exception.
                    throw new AggregateException(ex);
                }
            }, new ExecutionDataflowBlockOptions
            {
                MaxDegreeOfParallelism = Environment.ProcessorCount * 2,
                MaxMessagesPerTask = 1,
                BoundedCapacity = SizeOfQueue
            });

            batchBlock.BidirectionalLinkTo(actionBlock);

            await foreach (var document in collectionV1.Find(new BsonDocument()).ToAsyncEnumerable(ct: ct))
            {
                if (!await batchBlock.SendAsync(document, ct))
                {
                    break;
                }
            }

            batchBlock.Complete();

            await actionBlock.Completion;
        }

        private static void ConvertParentId(BsonDocument document)
        {
            if (document.Contains("pi"))
            {
                document["pi"] = document["pi"].AsGuid.ToString();
            }
        }
    }
}
