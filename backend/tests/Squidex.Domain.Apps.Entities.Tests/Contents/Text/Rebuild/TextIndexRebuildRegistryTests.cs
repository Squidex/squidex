// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Domain.Apps.Entities.TestHelpers;
using Squidex.Infrastructure;

namespace Squidex.Domain.Apps.Entities.Contents.Text.Rebuild;

public class TextIndexRebuildRegistryTests : GivenContext
{
    private readonly TextIndexRebuildRegistry sut =
        new TextIndexRebuildRegistry(
            new InMemoryPersistenceFactory<TextIndexRebuildRequests>(),
            new InMemoryPersistenceFactory<TextIndexSkipList>());

    [Fact]
    public async Task Should_start_request()
    {
        var request = await sut.StartAsync(AppId.Id, CancellationToken);

        var actual = await GetRequestAsync();

        Assert.Equal(request.Id, actual?.Id);
        Assert.Equal(TextIndexRebuildStatus.Running, actual?.Status);
        Assert.Equal(0, actual?.ProcessedSequence);
    }

    [Fact]
    public async Task Should_replace_existing_request_with_new_id()
    {
        var request1 = await sut.StartAsync(AppId.Id, CancellationToken);
        var request2 = await sut.StartAsync(AppId.Id, CancellationToken);

        var requests = sut.GetRequests();
        await requests.LoadAsync(CancellationToken);

        Assert.NotEqual(request1.Id, request2.Id);
        Assert.Equal(request2.Id, Assert.Single(requests.Value.Requests).Id);
    }

    [Fact]
    public async Task Should_update_request()
    {
        var request = await sut.StartAsync(AppId.Id, CancellationToken);

        var isActive = await sut.UpdateAsync(AppId.Id, request.Id, 42, TextIndexRebuildStatus.Completing, CancellationToken);

        var actual = await GetRequestAsync();

        Assert.True(isActive);
        Assert.Equal(42, actual?.ProcessedSequence);
        Assert.Equal(TextIndexRebuildStatus.Completing, actual?.Status);
    }

    [Fact]
    public async Task Should_not_update_request_if_replaced()
    {
        var request = await sut.StartAsync(AppId.Id, CancellationToken);

        await sut.StartAsync(AppId.Id, CancellationToken);

        var isActive = await sut.UpdateAsync(AppId.Id, request.Id, 42, TextIndexRebuildStatus.Completing, CancellationToken);

        Assert.False(isActive);
    }

    [Fact]
    public async Task Should_only_remove_matching_request()
    {
        var request = await sut.StartAsync(AppId.Id, CancellationToken);

        await sut.RemoveAsync(AppId.Id, DomainId.NewGuid(), CancellationToken);
        Assert.NotNull(await GetRequestAsync());

        await sut.RemoveAsync(AppId.Id, request.Id, CancellationToken);
        Assert.Null(await GetRequestAsync());
    }

    [Fact]
    public async Task Should_return_null_if_skip_list_does_not_exist()
    {
        var actual = await sut.GetSkipListAsync(AppId.Id, CancellationToken);

        Assert.Null(actual);
    }

    [Fact]
    public async Task Should_remove_request_and_skip_list_if_app_deleted()
    {
        await sut.StartAsync(AppId.Id, CancellationToken);

        var skipList = sut.GetSkipList(AppId.Id);
        await skipList.WriteAsync(CancellationToken);

        await ((IDeleter)sut).DeleteAppAsync(App, CancellationToken);

        Assert.Null(await GetRequestAsync());
        Assert.Null(await sut.GetSkipListAsync(AppId.Id, CancellationToken));
    }

    private async Task<TextIndexRebuildRequest?> GetRequestAsync()
    {
        var requests = sut.GetRequests();

        await requests.LoadAsync(CancellationToken);

        return requests.Value.Find(AppId.Id);
    }
}
