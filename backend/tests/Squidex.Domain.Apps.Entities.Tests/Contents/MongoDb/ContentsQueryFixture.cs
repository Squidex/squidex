// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Globalization;
using LoremNET;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using Squidex.Domain.Apps.Core.Apps;
using Squidex.Domain.Apps.Core.Contents;
using Squidex.Domain.Apps.Core.Schemas;
using Squidex.Domain.Apps.Core.TestHelpers;
using Squidex.Domain.Apps.Entities.MongoDb.Contents;
using Squidex.Domain.Apps.Entities.TestHelpers;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Json.Objects;
using Squidex.Infrastructure.MongoDb;
using Squidex.Infrastructure.States;

#pragma warning disable MA0048 // File name must match type name

namespace Squidex.Domain.Apps.Entities.Contents.MongoDb;

public sealed class ContentsQueryFixture_Default : ContentsQueryFixture
{
    public ContentsQueryFixture_Default()
        : base(false)
    {
    }
}

public sealed class ContentsQueryFixture_Dedicated : ContentsQueryFixture
{
    public ContentsQueryFixture_Dedicated()
        : base(true)
    {
    }
}

public abstract class ContentsQueryFixture : GivenContext, IAsyncLifetime
{
    private readonly int numValues = 10000;

    public IMongoDatabase Database { get; }

    public MongoContentRepository ContentRepository { get; }

    public NamedId<DomainId>[] AppIds { get; } =
    [
        NamedId.Of(DomainId.Create("3b5ba909-e5a5-4858-9d0d-df4ff922d452"), "my-app1"),
        NamedId.Of(DomainId.Create("4b3672c1-97c6-4e0b-a067-71e9e9a29db9"), "my-app1")
    ];

    public NamedId<DomainId>[] SchemaIds { get; } =
    [
        NamedId.Of(DomainId.Create("3b5ba909-e5a5-4858-9d0d-df4ff922d452"), "my-schema1"),
        NamedId.Of(DomainId.Create("4b3672c1-97c6-4e0b-a067-71e9e9a29db9"), "my-schema2"),
        NamedId.Of(DomainId.Create("76357c9b-0514-4377-9fcc-a632e7ef960d"), "my-schema3"),
        NamedId.Of(DomainId.Create("164c451e-e5a8-41f8-8aaf-e4b56603d7e7"), "my-schema4"),
        NamedId.Of(DomainId.Create("741e902c-fdfa-41ad-8e5a-b7cb9d6e3d94"), "my-schema5")
    ];

    protected ContentsQueryFixture(bool selfHosting)
    {
        BsonJsonConvention.Register(TestUtils.DefaultOptions());

        var mongoClient = MongoClientFactory.Create(TestConfig.Configuration["mongoDb:configuration"]);
        var mongoDatabase = mongoClient.GetDatabase(TestConfig.Configuration["mongodb:database"]);

        Database = mongoDatabase;

        var services =
            new ServiceCollection()
                .AddSingleton(Options.Create(new ContentOptions { OptimizeForSelfHosting = selfHosting }))
                .AddSingleton(CreateAppProvider())
                .AddSingleton(mongoClient)
                .AddSingleton(mongoDatabase)
                .AddSingleton<MongoContentRepository>()
                .AddLogging()
                .BuildServiceProvider();

        ContentRepository = services.GetRequiredService<MongoContentRepository>();
    }

    public async Task InitializeAsync()
    {
        await ContentRepository.InitializeAsync(default);

        await CreateDataAsync(default);
    }

    public Task DisposeAsync()
    {
        return Task.CompletedTask;
    }

    private async Task CreateDataAsync(
        CancellationToken ct)
    {
        if (await ContentRepository.StreamAll(AppIds[0].Id, null, SearchScope.All, ct).AnyAsync(ct))
        {
            return;
        }

        var batch = new List<SnapshotWriteJob<WriteContent>>();

        async Task ExecuteBatchAsync(WriteContent? state)
        {
            if (state != null)
            {
                batch.Add(new SnapshotWriteJob<WriteContent>(state.UniqueId, state, 0));
            }

            if ((state == null || batch.Count >= 1000) && batch.Count > 0)
            {
                var store = (ISnapshotStore<WriteContent>)ContentRepository;

                await store.WriteManyAsync(batch, ct);

                batch.Clear();
            }
        }

        foreach (var appId in AppIds)
        {
            foreach (var schemaId in SchemaIds)
            {
                for (var i = 0; i < numValues; i++)
                {
                    var content = CreateWriteContent() with
                    {
                        AppId = appId,
                        CurrentVersion = new ContentVersion(
                            Status.Published,
                            new ContentData()
                                .AddField("field1",
                                    new ContentFieldData()
                                        .AddInvariant(JsonValue.Create(i)))
                                .AddField("field2",
                                    new ContentFieldData()
                                        .AddInvariant(JsonValue.Create(Lorem.Paragraph(200, 20))))),
                        SchemaId = schemaId
                    };

                    await ExecuteBatchAsync(content);
                }
            }
        }

        await ExecuteBatchAsync(null);
    }

    private IAppProvider CreateAppProvider()
    {
        var appProvider = A.Fake<IAppProvider>();

        A.CallTo(() => appProvider.GetAppWithSchemaAsync(A<DomainId>._, A<DomainId>._, false, A<CancellationToken>._))
            .ReturnsLazily(x =>
            {
                var appId = x.GetArgument<DomainId>(0)!;

                return Task.FromResult<(App?, Schema?)>((
                    CreateApp(appId),
                    CreateSchema(appId, x.GetArgument<DomainId>(1)!)));
            });

        return appProvider;
    }

    public DomainId RandomAppId()
    {
        return AppIds[Random.Shared.Next(AppIds.Length)].Id;
    }

    public DomainId RandomSchemaId()
    {
        return SchemaIds[Random.Shared.Next(SchemaIds.Length)].Id;
    }

    public App RandomApp()
    {
        return CreateApp(RandomAppId());
    }

    public Schema RandomSchema()
    {
        return CreateSchema(RandomAppId(), RandomSchemaId());
    }

    public string RandomValue()
    {
        return Random.Shared.Next(numValues).ToString(CultureInfo.InvariantCulture);
    }

    private App CreateApp(DomainId appId)
    {
        return App with { Id = appId };
    }

    private Schema CreateSchema(DomainId appId, DomainId schemaId)
    {
        return Schema with { Id = schemaId, AppId = NamedId.Of(appId, "my-app") };
    }
}
