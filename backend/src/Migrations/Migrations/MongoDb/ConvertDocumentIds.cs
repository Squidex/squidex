// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using MongoDB.Bson;
using MongoDB.Driver;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Migrations;

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

        public async Task UpdateAsync()
        {
            switch (scope)
            {
                case Scope.Assets:
                    await RebuildAsync(database, ConvertParentId, "States_Assets");
                    await RebuildAsync(database, ConvertParentId, "States_AssetFolders");
                    break;
                case Scope.Contents:
                    await RebuildAsync(databaseContent, null, "State_Contents_All");
                    await RebuildAsync(databaseContent, null, "State_Contents_Published");
                    break;
            }
        }

        private static async Task RebuildAsync(IMongoDatabase database, Action<BsonDocument>? extraAction, string collectionNameOld)
        {
            const int SizeOfBatch = 1000;
            const int SizeOfQueue = 10;

            string collectionNameNew;

            collectionNameNew = $"{collectionNameOld}2";
            collectionNameNew = collectionNameNew.Replace("State_", "States_");

            var collectionOld = database.GetCollection<BsonDocument>(collectionNameOld);
            var collectionNew = database.GetCollection<BsonDocument>(collectionNameNew);

            await collectionNew.DeleteManyAsync(new BsonDocument());

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
                var updates = new List<WriteModel<BsonDocument>>();

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

                    updates.Add(new ReplaceOneModel<BsonDocument>(filter, document)
                    {
                        IsUpsert = true
                    });
                }

                await collectionNew.BulkWriteAsync(updates, writeOptions);
            }, new ExecutionDataflowBlockOptions
            {
                MaxDegreeOfParallelism = Environment.ProcessorCount * 2,
                MaxMessagesPerTask = 1,
                BoundedCapacity = SizeOfQueue
            });

            batchBlock.LinkTo(actionBlock, new DataflowLinkOptions
            {
                PropagateCompletion = true
            });

            await collectionOld.Find(new BsonDocument()).ForEachAsync(batchBlock.SendAsync);

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
