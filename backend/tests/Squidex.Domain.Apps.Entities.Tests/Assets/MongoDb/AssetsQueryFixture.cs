// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Globalization;
using Microsoft.Extensions.DependencyInjection;
using MongoDB.Driver;
using Squidex.Domain.Apps.Core.Assets;
using Squidex.Domain.Apps.Core.TestHelpers;
using Squidex.Domain.Apps.Entities.MongoDb.Assets;
using Squidex.Domain.Apps.Entities.TestHelpers;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Json.Objects;
using Squidex.Infrastructure.MongoDb;
using Squidex.Infrastructure.States;

namespace Squidex.Domain.Apps.Entities.Assets.MongoDb;

public sealed class AssetsQueryFixture : GivenContext, IAsyncLifetime
{
    private readonly int numValues = 250;

    private readonly NamedId<DomainId>[] appIds =
    [
        NamedId.Of(DomainId.Create("3b5ba909-e5a5-4858-9d0d-df4ff922d452"), "my-app1"),
        NamedId.Of(DomainId.Create("4b3672c1-97c6-4e0b-a067-71e9e9a29db9"), "my-app1")
    ];

    public IMongoDatabase Database { get; }

    public MongoAssetRepository AssetRepository { get; }

    public AssetsQueryFixture()
    {
        BsonJsonConvention.Register(TestUtils.DefaultOptions());

        var mongoClient = MongoClientFactory.Create(TestConfig.Configuration["mongoDb:configuration"]);
        var mongoDatabase = mongoClient.GetDatabase(TestConfig.Configuration["mongodb:database"]);

        Database = mongoDatabase;

        var services =
            new ServiceCollection()
                .AddSingleton<MongoAssetRepository>()
                .AddSingleton(mongoClient)
                .AddSingleton(mongoDatabase)
                .AddLogging()
                .BuildServiceProvider();

        AssetRepository = services.GetRequiredService<MongoAssetRepository>();
    }

    public async Task InitializeAsync()
    {
        await AssetRepository.InitializeAsync(default);

        await CreateDataAsync(default);
    }

    public Task DisposeAsync()
    {
        return Task.CompletedTask;
    }

    private async Task CreateDataAsync(
        CancellationToken ct)
    {
        if (await AssetRepository.StreamAll(appIds[0].Id, ct).AnyAsync(ct))
        {
            return;
        }

        var batch = new List<SnapshotWriteJob<Asset>>();

        async Task ExecuteBatchAsync(Asset? entity)
        {
            if (entity != null)
            {
                batch.Add(new SnapshotWriteJob<Asset>(entity.UniqueId, entity, 0));
            }

            if ((entity == null || batch.Count >= 1000) && batch.Count > 0)
            {
                var store = (ISnapshotStore<Asset>)AssetRepository;

                await store.WriteManyAsync(batch, ct);
                batch.Clear();
            }
        }

        foreach (var appId in appIds)
        {
            for (var i = 0; i < numValues; i++)
            {
                var fileName = i.ToString(CultureInfo.InvariantCulture);

                for (var j = 0; j < numValues; j++)
                {
                    var tag = j.ToString(CultureInfo.InvariantCulture);

                    var asset = CreateAsset() with
                    {
                        FileHash = fileName,
                        FileName = fileName,
                        Metadata = new AssetMetadata
                        {
                            ["value"] = JsonValue.Create(tag)
                        },
                        Tags =
                        [
                            tag
                        ],
                        Slug = fileName
                    };

                    await ExecuteBatchAsync(asset);
                }
            }
        }

        await ExecuteBatchAsync(null);
    }

    public DomainId RandomAppId()
    {
        return appIds[Random.Shared.Next(appIds.Length)].Id;
    }

    public string RandomValue()
    {
        return Random.Shared.Next(numValues).ToString(CultureInfo.InvariantCulture);
    }
}
