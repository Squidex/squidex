// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using NodaTime;
using Squidex.Domain.Apps.Core.Apps;
using Squidex.Domain.Apps.Core.Schemas;
using Squidex.Domain.Apps.Entities;
using Squidex.Domain.Apps.Entities.Schemas;
using Squidex.Domain.Apps.Entities.Schemas.Repositories;
using Squidex.Infrastructure;
using Squidex.Infrastructure.States;

namespace Squidex.Shared;

public abstract class SchemaRepositoryTests
{
    private readonly DomainId knownId = DomainId.Create("3e764e15-3cf5-427f-bb6f-f0fa29a40a2d");

    protected abstract Task<ISchemaRepository> CreateSutAsync();

    protected virtual async Task PrepareAsync(ISchemaRepository sut, Schema[] schemas)
    {
        if (sut is not ISnapshotStore<Schema> store)
        {
            return;
        }

        var writes = schemas.Select(x => new SnapshotWriteJob<Schema>(x.Id, x, 0));

        await store.WriteManyAsync(writes);
    }

    private async Task<ISchemaRepository> CreateAndPrepareSutAsync()
    {
        var sut = await CreateSutAsync();

        if (await sut.FindAsync(knownId, knownId) != null)
        {
            return sut;
        }

        var created = SystemClock.Instance.GetCurrentInstant();
        var createdBy = RefToken.Client("client1");

        var schema1 = new Schema
        {
            AppId = NamedId.Of(knownId, "my-app"),
            Id = DomainId.NewGuid(),
            Name = "schema1",
            Created = created,
            CreatedBy = createdBy,
        };

        var schema2 = new Schema
        {
            AppId = NamedId.Of(knownId, "my-app"),
            Id = knownId,
            Name = "schema2",
            Created = created,
            CreatedBy = createdBy,
        };

        var otherApp = new Schema
        {
            AppId = NamedId.Of(DomainId.NewGuid(), "my-app"),
            Id = DomainId.NewGuid(),
            Name = "schema3",
            Created = created,
            CreatedBy = createdBy,
        };

        await PrepareAsync(sut, [
            schema1,
            schema2,
            otherApp,
        ]);

        return sut;
    }

    [Fact]
    public async Task Should_find_by_id()
    {
        var sut = await CreateAndPrepareSutAsync();

        var found = await sut.FindAsync(knownId, knownId);

        Assert.Equal(knownId, found!.Id);
    }

    [Fact]
    public async Task Should_find_by_name()
    {
        var sut = await CreateAndPrepareSutAsync();

        var found = await sut.FindAsync(knownId, "schema2");

        Assert.Equal(knownId, found!.Id);
    }

    [Fact]
    public async Task Should_query_by_app()
    {
        var sut = await CreateAndPrepareSutAsync();

        var found = await sut.QueryAllAsync(knownId);

        Assert.Equal(2, found.Count);
    }

    [Fact]
    public async Task Should_calculate_schema_hash()
    {
        var sut = await CreateAndPrepareSutAsync();
        if (sut is not ISchemasHash hash)
        {
            return;
        }

        var found = await hash.GetCurrentHashAsync(new App { Id = knownId });

        Assert.Equal(2, found.Count);
    }

    [Fact]
    public async Task Should_delete_by_app()
    {
        var sut = await CreateSutAsync();
        if (sut is not IDeleter deleter)
        {
            return;
        }

        var appId = NamedId.Of(DomainId.NewGuid(), "my-app");

        var schema1 = new Schema
        {
            AppId = appId,
            Id = DomainId.NewGuid(),
            Name = "my-schema",
        };

        var schema2 = new Schema
        {
            AppId = appId,
            Id = DomainId.NewGuid(),
            Name = "my-schema",
        };

        await PrepareAsync(sut, [
            schema1,
            schema2,
        ]);

        var found1 = await sut.QueryAllAsync(appId.Id);
        Assert.Equal(2, found1.Count);

        await deleter.DeleteAppAsync(new App { Id = appId.Id }, default);

        var found2 = await sut.QueryAllAsync(appId.Id);
        Assert.Empty(found2);
    }

    [Fact]
    public async Task Should_delete_by_schema()
    {
        var sut = await CreateSutAsync();
        if (sut is not IDeleter deleter)
        {
            return;
        }

        var appId = NamedId.Of(DomainId.NewGuid(), "my-app");

        var schema1 = new Schema
        {
            AppId = appId,
            Id = DomainId.NewGuid(),
            Name = "my-schema",
        };

        var schema2 = new Schema
        {
            AppId = appId,
            Id = DomainId.NewGuid(),
            Name = "my-schema",
        };

        await PrepareAsync(sut, [
            schema1,
            schema2,
        ]);

        var found1 = await sut.QueryAllAsync(appId.Id);
        Assert.Equal(2, found1.Count);

        await deleter.DeleteSchemaAsync(new App { Id = appId.Id }, schema2, default);

        var found2 = await sut.QueryAllAsync(appId.Id);
        Assert.Equal(schema1.Id, found2.Single().Id);
    }
}
