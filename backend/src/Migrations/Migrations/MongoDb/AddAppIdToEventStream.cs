// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using MongoDB.Bson;
using MongoDB.Driver;
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

        public async Task UpdateAsync()
        {
            var collection = database.GetCollection<BsonDocument>("Events");

            const int SizeOfBatch = 200;
            const int SizeOfQueue = 20;

            var batchBlock = new BatchBlock<BsonDocument>(SizeOfBatch, new GroupingDataflowBlockOptions
            {
                BoundedCapacity = SizeOfQueue * SizeOfBatch
            });

            var actionBlock = new ActionBlock<BsonDocument[]>(async batch =>
            {
                var updates = new List<WriteModel<BsonDocument>>();

                foreach (var commit in batch)
                {
                    var eventStream = commit["EventStream"].AsString;

                    string? appId = null;

                    foreach (var @event in commit["Events"].AsBsonArray)
                    {
                        var metadata = @event["Metadata"].AsBsonDocument;

                        if (metadata.TryGetValue("AppId", out var value))
                        {
                            appId = value.AsString;
                        }
                    }

                    if (appId != null)
                    {
                        var parts = eventStream.Split("-");

                        var domainType = parts[0];
                        var domainId = string.Join("-", parts.Skip(1));

                        var newStreamName = $"{domainType}-{appId}--{domainId}";

                        var update = Builders<BsonDocument>.Update.Set("EventStream", newStreamName);

                        var i = 0;

                        foreach (var @event in commit["Events"].AsBsonArray)
                        {
                            update = update.Set($"Events.{i}.Metadata.AggregateId", $"{appId}--{domainId}");
                            update = update.Unset($"Events.{i}.Metadata.AppId");

                            i++;
                        }

                        var filter = Builders<BsonDocument>.Filter.Eq("_id", commit["_id"].AsString);

                        updates.Add(new UpdateOneModel<BsonDocument>(filter, update));
                    }
                }

                if (updates.Count > 0)
                {
                    await collection.BulkWriteAsync(updates);
                }
            }, new ExecutionDataflowBlockOptions
            {
                MaxDegreeOfParallelism = 4,
                MaxMessagesPerTask = 1,
                BoundedCapacity = SizeOfQueue
            });

            batchBlock.LinkTo(actionBlock, new DataflowLinkOptions
            {
                PropagateCompletion = true
            });

            await collection.Find(new BsonDocument()).ForEachAsync(async commit =>
            {
                var eventStream = commit["EventStream"].AsString;

                if (!eventStream.Contains("--") && !eventStream.StartsWith("app-", StringComparison.OrdinalIgnoreCase))
                {
                    await batchBlock.SendAsync(commit);
                }
            });

            batchBlock.Complete();

            await actionBlock.Completion;
        }
    }
}
