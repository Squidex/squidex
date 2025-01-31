// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using LoremNET;
using Microsoft.Extensions.Azure;
using NodaTime;
using Squidex.Domain.Apps.Core;
using Squidex.Domain.Apps.Core.Apps;
using Squidex.Domain.Apps.Core.Contents;
using Squidex.Domain.Apps.Core.Schemas;
using Squidex.Domain.Apps.Entities;
using Squidex.Domain.Apps.Entities.Contents;
using Squidex.Domain.Apps.Entities.Contents.Repositories;
using Squidex.Domain.Apps.Entities.TestHelpers;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Json.Objects;
using Squidex.Infrastructure.Queries;
using Squidex.Infrastructure.States;

#pragma warning disable xUnit1044 // Avoid using TheoryData type arguments that are not serializable
#pragma warning disable MA0040 // Forward the CancellationToken parameter to methods that take one

namespace Squidex.Shared;

public abstract class ContentRepositoryTests : GivenContext
{
    private const int NumValues = 50;
    private static readonly NamedId<DomainId>[] AppIds =
    [
        NamedId.Of(DomainId.Create("3b5ba909-e5a5-4858-9d0d-df4ff922d452"), "my-app1"),
        NamedId.Of(DomainId.Create("3b5ba909-e5a5-4858-9d0d-df4ff922d453"), "my-app2"),
    ];

    private static readonly NamedId<DomainId>[] SchemaIds =
    [
        NamedId.Of(DomainId.Create("3b5ba909-e5a5-4858-9d0d-df4ff922d454"), "my-schema1"),
        NamedId.Of(DomainId.Create("3b5ba909-e5a5-4858-9d0d-df4ff922d455"), "my-schema2"),
        NamedId.Of(DomainId.Create("3b5ba909-e5a5-4858-9d0d-df4ff922d456"), "my-schema3"),
        NamedId.Of(DomainId.Create("3b5ba909-e5a5-4858-9d0d-df4ff922d457"), "my-schema4"),
    ];

    private readonly Instant now = SystemClock.Instance.GetCurrentInstant();
    private readonly App app;
    private readonly Schema schema;

    protected ContentRepositoryTests()
    {
        var appId = AppIds[Random.Shared.Next(AppIds.Length)];
        app = CreateApp(appId.Id, appId.Name);

        var schemaId = SchemaIds[Random.Shared.Next(SchemaIds.Length)];
        schema = CreateSchema(app, schemaId.Id, schemaId.Name);

        A.CallTo(() => AppProvider.GetSchemaAsync(A<DomainId>._, A<DomainId>._, A<bool>._, A<CancellationToken>._))
            .ReturnsLazily(x =>
            {
                var appId = x.GetArgument<DomainId>(0);
                var appFound = CreateApp(appId, "my-app");

                var schemaId = x.GetArgument<DomainId>(1);
                var schemaFound = CreateSchema(appFound, schemaId, "my-schema");

                return Task.FromResult<Schema?>(schemaFound);
            });

        A.CallTo(() => AppProvider.GetAppWithSchemaAsync(A<DomainId>._, A<DomainId>._, A<bool>._, A<CancellationToken>._))
            .ReturnsLazily(x =>
            {
                var appId = x.GetArgument<DomainId>(0);
                var appFound = CreateApp(appId, "my-app");

                var schemaId = x.GetArgument<DomainId>(1);
                var schemaFound = CreateSchema(appFound, schemaId, "my-schema");

                return Task.FromResult<(App?, Schema?)>((appFound, schemaFound));
            });
    }

    protected abstract Task<IContentRepository> CreateSutAsync();

    private async Task<IContentRepository> CreateAndPrepareSutAsync()
    {
        var sut = await CreateSutAsync();
        if (sut is not ISnapshotStore<WriteContent> store)
        {
            return sut;
        }

        if (await sut.StreamAll(AppIds[0].Id, [], SearchScope.All).AnyAsync())
        {
            return sut;
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
                await store.WriteManyAsync(batch, default);
                batch.Clear();
            }
        }

        foreach (var forAppId in AppIds)
        {
            foreach (var forSchemaId in SchemaIds)
            {
                var previousIds = new List<DomainId>();

                for (var i = 0; i < NumValues; i++)
                {
                    var contentId = DomainId.NewGuid();

                    if (i == 0)
                    {
                        previousIds = [contentId];
                    }

                    var content = CreateWriteContent() with
                    {
                        Id = contentId,
                        CurrentVersion = new ContentVersion(
                            Status.Published,
                            new ContentData()
                                .AddField("field1",
                                    new ContentFieldData()
                                        .AddInvariant(JsonValue.Create(i)))
                                .AddField("field2",
                                    new ContentFieldData()
                                        .AddInvariant(JsonValue.Create(Lorem.Paragraph(200, 20))))
                                .AddField("references",
                                    new ContentFieldData()
                                        .AddInvariant(
                                            JsonValue.Array(previousIds.ToArray())))),
                        SchemaId = forSchemaId,
                        ScheduleJob =
                            i > NumValues / 2
                            ? new ScheduleJob(DomainId.NewGuid(), Status.Archived, User, now.Plus(Duration.FromDays(i)))
                            : null,
                        AppId = forAppId,
                    };

                    await ExecuteBatchAsync(content);
                }
            }
        }

        await ExecuteBatchAsync(null);

        return sut;
    }

    [Fact]
    public async Task Should_verify_ids()
    {
        var sut = await CreateAndPrepareSutAsync();

        var contentIds = await sut.StreamAll(app.Id, [schema.Id], default).Select(x => x.Id).ToHashSetAsync();
        var contents = await sut.QueryIdsAsync(app, contentIds, SearchScope.Published);

        // The IDs are valid.
        Assert.Equal(contents.Count, contentIds.Count);
    }

    [Fact]
    public async Task Should_query_contents_by_ids()
    {
        var sut = await CreateAndPrepareSutAsync();

        var contentIds = await sut.StreamAll(app.Id, [schema.Id], default).Select(x => x.Id).ToHashSetAsync();
        var contents = await sut.QueryAsync(app, [schema], Q.Empty.WithIds(contentIds), SearchScope.All);

        // The IDs are valid.
        Assert.Equal(contents.Count, contentIds.Count);
    }

    [Fact]
    public async Task Should_query_contents_by_ids_and_schema()
    {
        var sut = await CreateAndPrepareSutAsync();

        var contentIds = await sut.StreamAll(app.Id, [schema.Id], default).Select(x => x.Id).ToHashSetAsync();
        var contents = await sut.QueryAsync(app, schema, Q.Empty.WithIds(contentIds), SearchScope.All);

        // The IDs are valid.
        Assert.Equal(contents.Count, contentIds.Count);
    }

    [Fact]
    public async Task Should_query_contents_ids_by_filter()
    {
        var sut = await CreateAndPrepareSutAsync();

        var filter = ClrFilter.Eq("data.field1.iv", 12);

        var contents = await sut.QueryIdsAsync(app, schema, filter, SearchScope.All);

        // We have a concrete query, so we expect an actual.
        Assert.Single(contents);
    }

    [Fact]
    public async Task Should_query_contents_by_filter()
    {
        var query = new ClrQuery
        {
            Filter = ClrFilter.Eq("data.field1.iv", 12),
        };

        var contents = await QueryAsync(query, 1000, 0);

        // We have a concrete query, so we expect an actual.
        Assert.Single(contents);
    }

    [Fact]
    public async Task Should_query_contents_scheduled()
    {
        var sut = await CreateAndPrepareSutAsync();

        var contents = await sut.StreamScheduledWithoutDataAsync(now.Plus(Duration.FromDays(30)), default).ToListAsync();

        // The IDs are random here, as it does not really matter.
        Assert.NotEmpty(contents);
    }

    [Fact]
    public async Task Should_query_contents_with_default_query()
    {
        var query = new ClrQuery();

        var contents = await QueryAsync(query);

        // We have a concrete query, so we expect an actual result.
        Assert.Equal(NumValues, contents.Count);
    }

    [Fact]
    public async Task Should_query_contents_with_large_skip()
    {
        var query = new ClrQuery
        {
            Sort =
            [
                new SortNode("data.field1.iv", SortOrder.Ascending),
            ],
        };

        var contents = await QueryAsync(query, 1000, 9000);

        // We have a concrete query, so we expect an actual result.
        Assert.NotEmpty(contents);
    }

    [Fact]
    public async Task Should_query_contents_with_query_fulltext()
    {
        var query = new ClrQuery
        {
            FullText = "hello",
        };

        var contents = await QueryAsync(query);

        // The full text is resolved by another system, so we cannot verify the actual result.
        Assert.NotNull(contents);
    }

    [Fact]
    public async Task Should_query_contents_with_query_filter()
    {
        var query = new ClrQuery
        {
            Filter = ClrFilter.Eq("data.field1.iv", NumValues / 4),
        };

        var contents = await QueryAsync(query, 1000, 0);

        // We have a concrete query, so we expect an actual result.
        Assert.NotEmpty(contents);
    }

    [Fact]
    public async Task Should_query_contents_with_reference()
    {
        var baseQuery = new ClrQuery
        {
            Filter = ClrFilter.Eq("data.field1.iv", 0),
        };

        var content = await QueryAsync(baseQuery, 1);
        var contents = await QueryAsync(new ClrQuery(), reference: content[0].Id);

        // We do not insert test entities with references, so we cannot verify the actual result.
        Assert.Equal(NumValues, contents.Count);
    }

    [Fact]
    public async Task Should_query_contents_with_referencing()
    {
        var content = await QueryAsync(new ClrQuery(), 1, NumValues / 2);
        var contents = await QueryAsync(new ClrQuery(), 1000, 0, referencing: content[0].Id);

        // We do not insert test entities with references, so we cannot verify the actual result.
        Assert.Single(contents);
    }

    [Fact]
    public async Task Should_query_contents_with_random_count()
    {
        var query = new ClrQuery
        {
            Random = 40,
        };

        var contents = await QueryAsync(query);

        // We do not insert test entities with references, so we cannot verify the actual.
        Assert.Equal(40, contents.Count);
    }

    private async Task<IResultList<Content>> QueryAsync(
        ClrQuery clrQuery,
        int top = 1000,
        int skip = 0,
        DomainId reference = default,
        DomainId referencing = default)
    {
        clrQuery.Take = top;
        clrQuery.Skip = skip;
        clrQuery.Sort ??= [];

        if (clrQuery.Sort.Count == 0)
        {
            clrQuery.Sort.Add(new SortNode("lastModified", SortOrder.Descending));
        }

        if (!clrQuery.Sort.Exists(x => x.Path.Equals("id")))
        {
            clrQuery.Sort.Add(new SortNode("id", SortOrder.Ascending));
        }

        var q =
            Q.Empty
                .WithoutTotal()
                .WithQuery(clrQuery)
                .WithReference(reference)
                .WithReferencing(referencing);

        var sut = await CreateAndPrepareSutAsync();

        return await sut.QueryAsync(app, schema, q, SearchScope.All);
    }

    private App CreateApp(DomainId id, string name)
    {
        var newApp = App with { Id = id, Name = name };

        return newApp;
    }

    private Schema CreateSchema(App app, DomainId id, string name)
    {
        var newSchema = Schema with { AppId = app.NamedId(), Id = id, Name = name };

        return newSchema.AddReferences(0, "references", Partitioning.Invariant);
    }
}
