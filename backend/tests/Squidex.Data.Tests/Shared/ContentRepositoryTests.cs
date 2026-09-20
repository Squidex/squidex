// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using LoremNET;
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
        var appName = appId.Name;

        app = CreateApp(appId.Id, appName);

        var schemaId = SchemaIds[Random.Shared.Next(SchemaIds.Length)];
        var schemaName = schemaId.Name;

        schema = CreateSchema(app, schemaId.Id, schemaName);

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

    private async Task<IContentRepository> CreateAndPrepareSutAsync(params WriteContent[] contents)
    {
        var sut = await CreateSutAsync();
        if (sut is not ISnapshotStore<WriteContent> store)
        {
            return sut;
        }

        // Write the contents of the test itself, because the shared contents are only written once.
        await WriteAsync(store, contents);

        if (await sut.StreamAll(AppIds[0].Id, [schema.Id], SearchScope.All).AnyAsync())
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

    private static async Task WriteAsync(ISnapshotStore<WriteContent> store, WriteContent[] contents)
    {
        if (contents.Length == 0)
        {
            return;
        }

        await store.WriteManyAsync(contents.Select(x => new SnapshotWriteJob<WriteContent>(x.UniqueId, x, 0)).ToList(), default);
    }

    [Fact]
    public async Task Should_find_by_id()
    {
        var sut = await CreateAndPrepareSutAsync();

        var contentId = await sut.StreamAll(app.Id, [schema.Id], default).Select(x => x.Id).FirstOrDefaultAsync();
        var content = await sut.FindContentAsync(app, schema, contentId, null, SearchScope.All);

        // ID is not predicable, therefore the weak assertion.
        Assert.NotNull(content);
    }

    [Fact]
    public async Task Should_find_by_id_with_limited_fields()
    {
        var sut = await CreateAndPrepareSutAsync();

        var contentId = await sut.StreamAll(app.Id, [schema.Id], default).Select(x => x.Id).FirstOrDefaultAsync();
        var content = await sut.FindContentAsync(app, schema, contentId, HashSet.Of("field1"), SearchScope.All);

        // Only check that the we only go one field.
        Assert.NotNull(content);
        Assert.Single(content.Data);
        Assert.Contains("field1", content.Data);
    }

    [Fact]
    public async Task Should_stream_all_with_schema()
    {
        var sut = await CreateAndPrepareSutAsync();

        var count = await sut.StreamAll(AppIds[0].Id, [schema.Id], SearchScope.All).CountAsync();

        // IDs is not predicable, therefore the weak assertion.
        Assert.Equal(NumValues, count);
    }

    [Fact]
    public async Task Should_stream_all_without_schema()
    {
        var sut = await CreateAndPrepareSutAsync();

        var count = await sut.StreamAll(AppIds[0].Id, null, SearchScope.All).CountAsync();

        // IDs is not predicable, therefore the weak assertion.
        Assert.Equal(NumValues * SchemaIds.Length, count);
    }

    [Fact]
    public async Task Should_stream_all_with_empty_schemas()
    {
        var sut = await CreateAndPrepareSutAsync();

        var count = await sut.StreamAll(AppIds[0].Id, [], SearchScope.All).CountAsync();

        // IDs is not predicable, therefore the weak assertion.
        Assert.Equal(0, count);
    }

    [Fact]
    public async Task Should_stream_write_contents_with_schema()
    {
        var sut = await CreateAndPrepareSutAsync();

        var count = await sut.StreamWriteContents(AppIds[0].Id, [schema.Id], null).CountAsync();

        // IDs is not predicable, therefore the weak assertion.
        Assert.Equal(NumValues, count);
    }

    [Fact]
    public async Task Should_stream_write_contents_without_schema()
    {
        var sut = await CreateAndPrepareSutAsync();

        var count = await sut.StreamWriteContents(AppIds[0].Id, null, null).CountAsync();

        // IDs is not predicable, therefore the weak assertion.
        Assert.Equal(NumValues * SchemaIds.Length, count);
    }

    [Fact]
    public async Task Should_stream_write_contents_with_empty_schemas()
    {
        var sut = await CreateAndPrepareSutAsync();

        var count = await sut.StreamWriteContents(AppIds[0].Id, [], null).CountAsync();

        Assert.Equal(0, count);
    }

    [Fact]
    public async Task Should_stream_write_contents_with_all_versions()
    {
        static ContentData DataOf(string value)
        {
            return new ContentData()
                .AddField("field1",
                    new ContentFieldData()
                        .AddInvariant(JsonValue.Create(value)));
        }

        // Use another app to not change the results of the other tests.
        var appId = NamedId.Of(DomainId.NewGuid(), "my-app-versions");

        var content = CreateWriteContent() with
        {
            Id = DomainId.NewGuid(),
            AppId = appId,
            CurrentVersion = new ContentVersion(Status.Published, DataOf("published")),
            NewVersion = new ContentVersion(Status.Draft, DataOf("draft")),
            SchemaId = schema.NamedId(),
        };

        var sut = await CreateAndPrepareSutAsync(content);

        var actual = await sut.StreamWriteContents(appId.Id, [schema.Id], [content.Id]).ToListAsync();

        var found = Assert.Single(actual);

        Assert.Equal(Status.Published, found.CurrentVersion.Status);
        Assert.Equal(Status.Draft, found.NewVersion?.Status);

        found.CurrentVersion.Data.Should().BeEquivalentTo(DataOf("published"));
        found.NewVersion!.Data.Should().BeEquivalentTo(DataOf("draft"));
    }

    [Fact]
    public async Task Should_stream_write_contents_with_ids()
    {
        var sut = await CreateAndPrepareSutAsync();

        var contentIds = await sut.StreamAll(app.Id, [schema.Id], SearchScope.All).Select(x => x.Id).Take(2).ToHashSetAsync();

        var contents = await sut.StreamWriteContents(app.Id, [schema.Id], contentIds).ToListAsync();

        Assert.Equal(contentIds, contents.Select(x => x.Id).ToHashSet());
    }

    [Fact]
    public async Task Should_stream_write_contents_with_empty_ids()
    {
        var sut = await CreateAndPrepareSutAsync();

        var count = await sut.StreamWriteContents(app.Id, [schema.Id], []).CountAsync();

        Assert.Equal(0, count);
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
    public async Task Should_query_by_ids()
    {
        var sut = await CreateAndPrepareSutAsync();

        var contentIds = await sut.StreamAll(app.Id, [schema.Id], default).Select(x => x.Id).ToHashSetAsync();
        var contents = await sut.QueryAsync(app, [schema], Q.Empty.WithIds(contentIds), SearchScope.All);

        // The IDs are valid.
        Assert.Equal(contents.Count, contentIds.Count);
    }

    [Fact]
    public async Task Should_query_by_ids_and_schema()
    {
        var sut = await CreateAndPrepareSutAsync();

        var contentIds = await sut.StreamAll(app.Id, [schema.Id], default).Select(x => x.Id).ToHashSetAsync();
        var contents = await sut.QueryAsync(app, schema, Q.Empty.WithIds(contentIds), SearchScope.All);

        // The IDs are valid.
        Assert.Equal(contents.Count, contentIds.Count);
    }

    [Fact]
    public async Task Should_query_ids_by_filter()
    {
        var sut = await CreateAndPrepareSutAsync();

        var filter = ClrFilter.Eq("data.field1.iv", 12);

        var contents = await sut.QueryIdsAsync(app, schema, filter, SearchScope.All);

        // We have a concrete query, so we expect an actual.
        Assert.Single(contents);
    }

    [Fact]
    public async Task Should_query_with_limit_and_total()
    {
        var contents = await QueryAsync(new ClrQuery(), 20, 0, withTotal: true);

        // We have a concrete query, so we expect an actual.
        Assert.Equal(20, contents.Count);
        Assert.Equal(50, contents.Total);
    }

    [Fact]
    public async Task Should_query_by_filter()
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
    public async Task Should_query_scheduled()
    {
        var sut = await CreateAndPrepareSutAsync();

        var contents = await sut.StreamScheduledWithoutDataAsync(now.Plus(Duration.FromDays(30)), default).ToListAsync();

        // The IDs are random here, as it does not really matter.
        Assert.NotEmpty(contents);
    }

    [Fact]
    public async Task Should_query_with_default_query()
    {
        var query = new ClrQuery();

        var contents = await QueryAsync(query);

        // We have a concrete query, so we expect an actual result.
        Assert.Equal(NumValues, contents.Count);
    }

    [Fact]
    public async Task Should_query_with_fields()
    {
        var query = new ClrQuery();

        var contents = await QueryAsync(query, fields: HashSet.Of("field1"));

        // We have a concrete query, so we expect an actual result.
        Assert.All(contents, content =>
        {
            Assert.Single(content.Data);
            Assert.Contains("field1", content.Data);
        });
    }

    [Fact]
    public async Task Should_query_with_large_skip()
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
        Assert.Empty(contents);
    }

    [Fact]
    public async Task Should_query_with_query_fulltext()
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
    public async Task Should_query_with_query_filter()
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
    public async Task Should_query_with_reference()
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
    public async Task Should_query_with_referencing()
    {
        var content = await QueryAsync(new ClrQuery(), 1, NumValues / 2);
        var contents = await QueryAsync(new ClrQuery(), 1000, 0, referencing: content[0].Id);

        // We do not insert test entities with references, so we cannot verify the actual result.
        Assert.Single(contents);
    }

    [Fact]
    public async Task Should_query_with_random_count()
    {
        var query = new ClrQuery
        {
            Random = 40,
        };

        var contents = await QueryAsync(query);

        // We do not insert test entities with references, so we cannot verify the actual.
        Assert.Equal(40, contents.Count);
    }

    [Fact]
    public async Task Should_stream_ids()
    {
        var sut = await CreateAndPrepareSutAsync();

        var count = await sut.StreamIds(AppIds[0].Id, [schema.Id], SearchScope.All).CountAsync();

        Assert.Equal(NumValues, count);
    }

    [Fact]
    public async Task Should_stream_ids_including_deleted_content()
    {
        var (otherApp, otherSchema) = CreateOtherApp();

        var active = CreateContent(otherApp, Status.Published);
        var deleted = CreateContent(otherApp, Status.Published) with { IsDeleted = true };

        var sut = await CreateAndPrepareSutAsync(active, deleted);

        var streamedIds = await sut.StreamIds(otherApp.Id, [otherSchema.Id], SearchScope.All).ToHashSetAsync();

        // The IDs are used to clean up events and states, which also exist for deleted contents.
        Assert.Equal(HashSet.Of(active.Id, deleted.Id), streamedIds);
    }

    [Fact]
    public async Task Should_stream_ids_with_empty_schemas()
    {
        var sut = await CreateAndPrepareSutAsync();

        var count = await sut.StreamIds(AppIds[0].Id, [], SearchScope.All).CountAsync();

        Assert.Equal(0, count);
    }

    [Fact]
    public async Task Should_only_return_published_content_in_published_scope()
    {
        var (otherApp, otherSchema) = CreateOtherApp();

        var published = CreateContent(otherApp, Status.Published);
        var draft = CreateContent(otherApp, Status.Draft);

        var sut = await CreateAndPrepareSutAsync(published, draft);

        var ids = HashSet.Of(published.Id, draft.Id);

        var streamedAll = await sut.StreamAll(otherApp.Id, [otherSchema.Id], SearchScope.All).Select(x => x.Id).ToHashSetAsync();
        var streamedPublished = await sut.StreamAll(otherApp.Id, [otherSchema.Id], SearchScope.Published).Select(x => x.Id).ToHashSetAsync();
        var streamedIds = await sut.StreamIds(otherApp.Id, [otherSchema.Id], SearchScope.Published).ToHashSetAsync();
        var queriedIds = await sut.QueryIdsAsync(otherApp, ids, SearchScope.Published);

        Assert.Equal(ids, streamedAll);
        Assert.Equal(HashSet.Of(published.Id), streamedPublished);
        Assert.Equal(HashSet.Of(published.Id), streamedIds);
        Assert.Equal(HashSet.Of(published.Id), queriedIds.Select(x => x.Id).ToHashSet());

        Assert.NotNull(await sut.FindContentAsync(otherApp, otherSchema, draft.Id, null, SearchScope.All));
        Assert.Null(await sut.FindContentAsync(otherApp, otherSchema, draft.Id, null, SearchScope.Published));
    }

    [Fact]
    public async Task Should_remove_content_from_published_scope_if_unpublished()
    {
        var (otherApp, otherSchema) = CreateOtherApp();

        var sut = await CreateAndPrepareSutAsync();
        if (sut is not ISnapshotStore<WriteContent> store)
        {
            return;
        }

        var published = CreateContent(otherApp, Status.Published);

        await store.WriteAsync(new SnapshotWriteJob<WriteContent>(published.UniqueId, published, 0));

        var unpublished = published with
        {
            CurrentVersion = new ContentVersion(Status.Draft, published.CurrentVersion.Data),
            Version = 1,
        };

        await store.WriteAsync(new SnapshotWriteJob<WriteContent>(unpublished.UniqueId, unpublished, 1));

        Assert.NotNull(await sut.FindContentAsync(otherApp, otherSchema, published.Id, null, SearchScope.All));
        Assert.Null(await sut.FindContentAsync(otherApp, otherSchema, published.Id, null, SearchScope.Published));
    }

    [Fact]
    public async Task Should_not_return_deleted_content()
    {
        var (otherApp, otherSchema) = CreateOtherApp();

        var active = CreateContent(otherApp, Status.Published);
        var deleted = CreateContent(otherApp, Status.Published) with { IsDeleted = true };

        var sut = await CreateAndPrepareSutAsync(active, deleted);

        var ids = HashSet.Of(active.Id, deleted.Id);

        var streamed = await sut.StreamAll(otherApp.Id, null, SearchScope.All).Select(x => x.Id).ToHashSetAsync();
        var streamedWrites = await sut.StreamWriteContents(otherApp.Id, null, null).Select(x => x.Id).ToHashSetAsync();
        var queriedIds = await sut.QueryIdsAsync(otherApp, ids, SearchScope.All);
        var queried = await sut.QueryAsync(otherApp, otherSchema, Q.Empty.WithIds(ids), SearchScope.All);

        Assert.Equal(HashSet.Of(active.Id), streamed);
        Assert.Equal(HashSet.Of(active.Id), streamedWrites);
        Assert.Equal(HashSet.Of(active.Id), queriedIds.Select(x => x.Id).ToHashSet());
        Assert.Equal(HashSet.Of(active.Id), queried.Select(x => x.Id).ToHashSet());
    }

    [Fact]
    public async Task Should_find_referrers()
    {
        var (otherApp, _) = CreateOtherApp();

        var referenced = CreateContent(otherApp, Status.Published);
        var referencing = CreateContent(otherApp, Status.Published, referenced.Id);

        var sut = await CreateAndPrepareSutAsync(referenced, referencing);

        var referrers = await sut.StreamReferencing(otherApp.Id, referenced.Id, 100, SearchScope.All).ToListAsync();

        Assert.True(await sut.HasReferrersAsync(otherApp, referenced.Id, SearchScope.All));
        Assert.False(await sut.HasReferrersAsync(otherApp, referencing.Id, SearchScope.All));
        Assert.Equal(referencing.Id, referrers.Single().Id);
    }

    [Fact]
    public async Task Should_not_find_deleted_referrers()
    {
        var (otherApp, _) = CreateOtherApp();

        var referenced = CreateContent(otherApp, Status.Published);
        var referencing = CreateContent(otherApp, Status.Published, referenced.Id) with { IsDeleted = true };

        var sut = await CreateAndPrepareSutAsync(referenced, referencing);

        var referrers = await sut.StreamReferencing(otherApp.Id, referenced.Id, 100, SearchScope.All).ToListAsync();

        // A deleted content must not block the deletion of the referenced content.
        Assert.False(await sut.HasReferrersAsync(otherApp, referenced.Id, SearchScope.All));
        Assert.Empty(referrers);
    }

    [Fact]
    public async Task Should_not_find_content_as_referrer_of_itself()
    {
        var (otherApp, _) = CreateOtherApp();

        var id = DomainId.NewGuid();

        var selfReferencing = CreateContent(otherApp, Status.Published, id) with { Id = id };

        var sut = await CreateAndPrepareSutAsync(selfReferencing);

        var referrers = await sut.StreamReferencing(otherApp.Id, id, 100, SearchScope.All).ToListAsync();

        Assert.False(await sut.HasReferrersAsync(otherApp, id, SearchScope.All));
        Assert.Empty(referrers);
    }

    [Fact]
    public async Task Should_only_stream_scheduled_content_that_is_due()
    {
        var (otherApp, _) = CreateOtherApp();

        var due = CreateContent(otherApp, Status.Draft) with { ScheduleJob = CreateScheduleJob(now.Minus(Duration.FromDays(1))) };
        var future = CreateContent(otherApp, Status.Draft) with { ScheduleJob = CreateScheduleJob(now.Plus(Duration.FromDays(1))) };
        var deleted = CreateContent(otherApp, Status.Draft) with { ScheduleJob = CreateScheduleJob(now.Minus(Duration.FromDays(1))), IsDeleted = true };

        var sut = await CreateAndPrepareSutAsync(due, future, deleted);

        // The scheduler scans all apps, therefore we only look at the contents of this test.
        var scheduled =
            await sut.StreamScheduledWithoutDataAsync(now, SearchScope.All)
                .Where(x => x.AppId.Id == otherApp.Id)
                .Select(x => x.Id)
                .ToHashSetAsync();

        Assert.Equal(HashSet.Of(due.Id), scheduled);
    }

    [Fact]
    public async Task Should_reset_scheduled()
    {
        var (otherApp, _) = CreateOtherApp();

        var due = CreateContent(otherApp, Status.Draft) with { ScheduleJob = CreateScheduleJob(now.Minus(Duration.FromDays(1))) };

        var sut = await CreateAndPrepareSutAsync(due);

        await sut.ResetScheduledAsync(otherApp.Id, due.Id, SearchScope.All);

        var scheduled =
            await sut.StreamScheduledWithoutDataAsync(now, SearchScope.All)
                .Where(x => x.Id == due.Id)
                .ToListAsync();

        Assert.Empty(scheduled);
    }

    private (App, Schema) CreateOtherApp()
    {
        // Use another app to not change the results of the other tests.
        var otherApp = CreateApp(DomainId.NewGuid(), "my-app-other");
        var otherSchema = CreateSchema(otherApp, schema.Id, schema.Name);

        return (otherApp, otherSchema);
    }

    private WriteContent CreateContent(App forApp, Status status, params DomainId[] references)
    {
        return CreateWriteContent() with
        {
            Id = DomainId.NewGuid(),
            AppId = forApp.NamedId(),
            CurrentVersion = new ContentVersion(
                status,
                new ContentData()
                    .AddField("field1",
                        new ContentFieldData()
                            .AddInvariant(JsonValue.Create(1)))
                    .AddField("references",
                        new ContentFieldData()
                            .AddInvariant(JsonValue.Array(references)))),
            SchemaId = schema.NamedId(),
            ScheduleJob = null,
        };
    }

    private ScheduleJob CreateScheduleJob(Instant dueTime)
    {
        return new ScheduleJob(DomainId.NewGuid(), Status.Published, User, dueTime);
    }

    private async Task<IResultList<Content>> QueryAsync(
        ClrQuery clrQuery,
        int top = 1000,
        int skip = 0,
        DomainId reference = default,
        DomainId referencing = default,
        HashSet<string>? fields = null,
        bool withTotal = false)
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
                .WithFields(fields)
                .WithoutTotal(!withTotal)
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
