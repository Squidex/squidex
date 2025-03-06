// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Domain.Apps.Core;
using Squidex.Domain.Apps.Core.Apps;
using Squidex.Domain.Apps.Core.Contents;
using Squidex.Domain.Apps.Core.Schemas;
using Squidex.Domain.Apps.Entities;
using Squidex.Domain.Apps.Entities.TestHelpers;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Json.Objects;
using Squidex.Infrastructure.Reflection;
using Squidex.Infrastructure.States;

namespace Squidex.Shared;

public abstract class ContentSnapshotStoreTests : SnapshotStoreTests<WriteContent>
{
    public GivenContext Context { get; } = new GivenContext();

    protected ContentSnapshotStoreTests()
    {
        Context.Schema = Context.Schema.AddReferences(0, "myReferences", Partitioning.Invariant);
    }

    protected override WriteContent CreateEntity(DomainId id, int version)
    {
        return Cleanup(Context.CreateWriteContent() with
        {
            Id = id,
            CurrentVersion = new ContentVersion(Status.Published,
                new ContentData()
                    .AddField("myString",
                        new ContentFieldData()
                            .AddInvariant("Hello Squidex"))
                    .AddField("myReferences",
                        new ContentFieldData()
                            .AddInvariant(
                                JsonValue.Array(
                                    DomainId.NewGuid(),
                                    DomainId.NewGuid(),
                                    DomainId.NewGuid())))),
            Version = version,
        });
    }

    [Fact]
    public async Task Should_remove_by_app()
    {
        var sut = await CreateSutAsync();

        if (sut is not IDeleter deleter)
        {
            return;
        }

        var app1 = Context.App with { Id = DomainId.NewGuid() };
        var app2 = Context.App with { Id = DomainId.NewGuid() };

        A.CallTo(() => Context.AppProvider.GetAppWithSchemaAsync(app1.Id, Context.SchemaId.Id, true, default))
            .Returns((app1, Context.Schema));

        A.CallTo(() => Context.AppProvider.GetAppWithSchemaAsync(app2.Id, Context.SchemaId.Id, true, default))
            .Returns((app2, Context.Schema));

        var content1 = CreateEntity(DomainId.NewGuid(), 1) with { AppId = app1.NamedId() };
        var content2 = CreateEntity(DomainId.NewGuid(), 1) with { AppId = app2.NamedId() };

        await sut.WriteManyAsync([
            new SnapshotWriteJob<WriteContent>(content1.Id, content1, 0),
            new SnapshotWriteJob<WriteContent>(content2.Id, content2, 0),
        ], default);

        await deleter.DeleteAppAsync(app1, default);

        var all = await sut.ReadAllAsync(default).ToListAsync(default);

        var byApp1 = all.Where(x => x.Value.AppId.Id == app1.Id);
        var byApp2 = all.Where(x => x.Value.AppId.Id == app2.Id);

        Assert.Empty(byApp1);
        Assert.NotEmpty(byApp2);
    }

    [Fact]
    public async Task Should_remove_by_schema()
    {
        var sut = await CreateSutAsync();

        if (sut is not IDeleter deleter)
        {
            return;
        }

        var schema1 = Context.Schema with { Id = DomainId.NewGuid() };
        var schema2 = Context.Schema with { Id = DomainId.NewGuid() };

        A.CallTo(() => Context.AppProvider.GetAppWithSchemaAsync(Context.App.Id, schema1.Id, true, default))
            .Returns((Context.App, schema1));

        A.CallTo(() => Context.AppProvider.GetAppWithSchemaAsync(Context.App.Id, schema2.Id, true, default))
            .Returns((Context.App, schema2));

        var content1 = CreateEntity(DomainId.NewGuid(), 1) with { SchemaId = schema1.NamedId() };
        var content2 = CreateEntity(DomainId.NewGuid(), 1) with { SchemaId = schema2.NamedId() };

        await sut.WriteManyAsync([
            new SnapshotWriteJob<WriteContent>(content1.Id, content1, 0),
            new SnapshotWriteJob<WriteContent>(content2.Id, content2, 0),
        ], default);

        await deleter.DeleteSchemaAsync(Context.App, schema1, default);

        var all = await sut.ReadAllAsync(default).ToListAsync(default);

        var bySchema1 = all.Where(x => x.Value.SchemaId.Id == schema1.Id);
        var bySchema2 = all.Where(x => x.Value.SchemaId.Id == schema2.Id);

        Assert.Empty(bySchema1);
        Assert.NotEmpty(bySchema2);
    }

    protected override WriteContent Cleanup(WriteContent expected)
    {
        return SimpleMapper.Map(expected, new WriteContent());
    }
}
