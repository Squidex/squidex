// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Globalization;
using MongoDB.Bson;
using MongoDB.Driver;
using NodaTime;
using Squidex.Domain.Apps.Core.Assets;
using Squidex.Domain.Apps.Core.TestHelpers;
using Squidex.Domain.Apps.Entities.Assets.DomainObject;
using Squidex.Domain.Apps.Entities.MongoDb.Assets;
using Squidex.Domain.Apps.Entities.TestHelpers;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Json.Objects;
using Squidex.Infrastructure.MongoDb;
using Squidex.Infrastructure.States;

namespace Squidex.Domain.Apps.Entities.Assets.MongoDb;

public sealed class AssetsQueryFixture : IAsyncLifetime
{
    private readonly int numValues = 250;
    private readonly IMongoClient mongoClient;
    private readonly IMongoDatabase mongoDatabase;

    public MongoAssetRepository AssetRepository { get; }

    public NamedId<DomainId>[] AppIds { get; } =
    {
        NamedId.Of(DomainId.Create("3b5ba909-e5a5-4858-9d0d-df4ff922d452"), "my-app1"),
        NamedId.Of(DomainId.Create("4b3672c1-97c6-4e0b-a067-71e9e9a29db9"), "my-app1")
    };

    public AssetsQueryFixture()
    {
        BsonJsonConvention.Register(TestUtils.DefaultOptions());

        mongoClient = new MongoClient(TestConfig.Configuration["mongodb:configuration"]);
        mongoDatabase = mongoClient.GetDatabase(TestConfig.Configuration["mongodb:database"]);

        AssetRepository = new MongoAssetRepository(mongoDatabase);
    }

    public Task DisposeAsync()
    {
        return Task.CompletedTask;
    }

    public async Task InitializeAsync()
    {
        await AssetRepository.InitializeAsync(default);

        await CreateDataAsync(default);
        await ClearProfileAsync(default);
    }

    private async Task CreateDataAsync(
        CancellationToken ct)
    {
        if (await AssetRepository.StreamAll(AppIds[0].Id, ct).AnyAsync(ct))
        {
            return;
        }

        var batch = new List<SnapshotWriteJob<AssetDomainObject.State>>();

        async Task ExecuteBatchAsync(AssetDomainObject.State? entity)
        {
            if (entity != null)
            {
                batch.Add(new SnapshotWriteJob<AssetDomainObject.State>(entity.UniqueId, entity, 0));
            }

            if ((entity == null || batch.Count >= 1000) && batch.Count > 0)
            {
                var store = (ISnapshotStore<AssetDomainObject.State>)AssetRepository;

                await store.WriteManyAsync(batch, ct);

                batch.Clear();
            }
        }

        var now = SystemClock.Instance.GetCurrentInstant();

        var user = RefToken.User("1");

        foreach (var appId in AppIds)
        {
            for (var i = 0; i < numValues; i++)
            {
                var fileName = i.ToString(CultureInfo.InvariantCulture);

                for (var j = 0; j < numValues; j++)
                {
                    var tag = j.ToString(CultureInfo.InvariantCulture);

                    var asset = new AssetDomainObject.State
                    {
                        Id = DomainId.NewGuid(),
                        AppId = appId,
                        Created = now,
                        CreatedBy = user,
                        FileHash = fileName,
                        FileName = fileName,
                        FileSize = 1024,
                        LastModified = now,
                        LastModifiedBy = user,
                        IsDeleted = false,
                        IsProtected = false,
                        Metadata = new AssetMetadata
                        {
                            ["value"] = JsonValue.Create(tag)
                        },
                        Tags = new HashSet<string>
                        {
                            tag
                        },
                        Slug = fileName
                    };

                    await ExecuteBatchAsync(asset);
                }
            }
        }

        await ExecuteBatchAsync(null);
    }

    private async Task ClearProfileAsync(
        CancellationToken ct)
    {
        await mongoDatabase.RunCommandAsync<BsonDocument>("{ profile : 0 }", cancellationToken: ct);
        await mongoDatabase.DropCollectionAsync("system.profile", ct);
        await mongoDatabase.RunCommandAsync<BsonDocument>("{ profile : 2 }", cancellationToken: ct);
    }

    public DomainId RandomAppId()
    {
        return AppIds[Random.Shared.Next(AppIds.Length)].Id;
    }

    public string RandomValue()
    {
        return Random.Shared.Next(numValues).ToString(CultureInfo.InvariantCulture);
    }
}
