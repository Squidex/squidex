// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using MongoDB.Driver;

namespace Squidex.Domain.Apps.Entities.MongoDb.Text;

public sealed class MongoTextIndex : MongoTextIndexBase<List<MongoTextIndexEntityText>>
{
    public MongoTextIndex(IMongoDatabase database)
        : base(database)
    {
    }

    protected override async Task SetupCollectionAsync(IMongoCollection<MongoTextIndexEntity<List<MongoTextIndexEntityText>>> collection,
        CancellationToken ct)
    {
        await base.SetupCollectionAsync(collection, ct);

        await collection.Indexes.CreateOneAsync(
            new CreateIndexModel<MongoTextIndexEntity<List<MongoTextIndexEntityText>>>(
                Index
                    .Text("t.t")
                    .Ascending(x => x.AppId)
                    .Ascending(x => x.ServeAll)
                    .Ascending(x => x.ServePublished)
                    .Ascending(x => x.SchemaId)),
            cancellationToken: ct);
    }

    protected override List<MongoTextIndexEntityText> BuildTexts(Dictionary<string, string> source)
    {
        return source.Select(x => new MongoTextIndexEntityText { Text = x.Value }).ToList();
    }
}
