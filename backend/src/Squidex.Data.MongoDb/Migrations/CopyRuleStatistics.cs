﻿// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Driver;
using Squidex.Domain.Apps.Entities.Rules;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Migrations;

namespace Squidex.Migrations;

public sealed class CopyRuleStatistics(IMongoDatabase database, IRuleUsageTracker ruleUsageTracker) : IMigration
{
    [BsonIgnoreExtraElements]
    public class Document
    {
        public DomainId AppId { get; private set; }

        public DomainId RuleId { get; private set; }

        public int NumFailed { get; private set; }

        public int NumSucceeded { get; private set; }
    }

    public async Task UpdateAsync(
        CancellationToken ct)
    {
        var collectionName = "RuleStatistics";

        // Do not create the collection if not needed.
        if (!await database.CollectionExistsAsync(collectionName, ct))
        {
            return;
        }

        var collection = database.GetCollection<Document>(collectionName);

        await foreach (var document in collection.Find(new BsonDocument()).ToAsyncEnumerable(ct))
        {
            await ruleUsageTracker.TrackAsync(
                document.AppId,
                document.RuleId,
                default,
                0,
                document.NumSucceeded,
                document.NumFailed,
                ct);
        }
    }
}
