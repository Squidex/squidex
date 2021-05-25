// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using MongoDB.Bson;
using MongoDB.Driver;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Migrations;

namespace Migrations.Migrations.MongoDb
{
    public sealed class AddAppIdToEventStream : IMigration
    {
        private readonly IMongoDatabase database;

        public AddAppIdToEventStream(IMongoDatabase database)
        {
            this.database = database;
        }

        public async Task UpdateAsync(CancellationToken ct)
        {
            const int SizeOfBatch = 1000;
            const int SizeOfQueue = 20;

            var collectionOld = database.GetCollection<BsonDocument>("Events");
            var collectionNew = database.GetCollection<BsonDocument>("Events2");

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
                        var eventStream = document["EventStream"].AsString;

                        if (TryGetAppId(document, out var appId))
                        {
                            if (!eventStream.StartsWith("app-", StringComparison.OrdinalIgnoreCase))
                            {
                                var indexOfType = eventStream.IndexOf('-');
                                var indexOfId = indexOfType + 1;

                                var indexOfOldId = eventStream.LastIndexOf("--", StringComparison.OrdinalIgnoreCase);

                                if (indexOfOldId > 0)
                                {
                                    indexOfId = indexOfOldId + 2;
                                }

                                var domainType = eventStream.Substring(0, indexOfType);
                                var domainId = eventStream[indexOfId..];

                                var newDomainId = DomainId.Combine(DomainId.Create(appId), DomainId.Create(domainId)).ToString();
                                var newStreamName = $"{domainType}-{newDomainId}";

                                document["EventStream"] = newStreamName;

                                foreach (var @event in document["Events"].AsBsonArray)
                                {
                                    var metadata = @event["Metadata"].AsBsonDocument;

                                    metadata["AggregateId"] = newDomainId;
                                }
                            }

                            foreach (var @event in document["Events"].AsBsonArray)
                            {
                                var metadata = @event["Metadata"].AsBsonDocument;

                                metadata.Remove("AppId");
                            }
                        }

                        var filter = Builders<BsonDocument>.Filter.Eq("_id", document["_id"].AsString);

                        writes.Add(new ReplaceOneModel<BsonDocument>(filter, document)
                        {
                            IsUpsert = true
                        });
                    }

                    if (writes.Count > 0)
                    {
                        await collectionNew.BulkWriteAsync(writes, writeOptions);
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
                MaxMessagesPerTask = 10,
                BoundedCapacity = SizeOfQueue
            });

            batchBlock.LinkTo(actionBlock, new DataflowLinkOptions
            {
                PropagateCompletion = true
            });

            await collectionOld.Find(new BsonDocument()).ForEachAsync(batchBlock.SendAsync, ct);

            batchBlock.Complete();

            await actionBlock.Completion;
        }

        private static bool TryGetAppId(BsonDocument document, [MaybeNullWhen(false)] out string appId)
        {
            const int guidLength = 36;

            foreach (var @event in document["Events"].AsBsonArray)
            {
                var metadata = @event["Metadata"].AsBsonDocument;

                if (metadata.TryGetValue("AppId", out var value))
                {
                    appId = value.AsString;
                    return true;
                }

                if (metadata.TryGetValue("AggregateId", out var aggregateId))
                {
                    var parts = aggregateId.AsString.Split("--");

                    if (parts.Length == 2)
                    {
                        appId = parts[0];
                        return true;
                    }
                }

                var payload = @event["Payload"].AsString;

                var indexOfAppId = payload.IndexOf("appId\":\"", StringComparison.OrdinalIgnoreCase);

                if (indexOfAppId > 0)
                {
                    appId = payload.Substring(indexOfAppId, guidLength);
                    return true;
                }
            }

            appId = null;

            return false;
        }
    }
}
