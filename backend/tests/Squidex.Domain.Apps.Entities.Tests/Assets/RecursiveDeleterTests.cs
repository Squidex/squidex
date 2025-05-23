﻿// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Microsoft.Extensions.Logging;
using Squidex.Domain.Apps.Core.TestHelpers;
using Squidex.Domain.Apps.Entities.Assets.Commands;
using Squidex.Domain.Apps.Entities.Assets.Repositories;
using Squidex.Domain.Apps.Entities.TestHelpers;
using Squidex.Domain.Apps.Events.Assets;
using Squidex.Events;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Commands;
using Squidex.Infrastructure.EventSourcing;

namespace Squidex.Domain.Apps.Entities.Assets;

public class RecursiveDeleterTests : GivenContext
{
    private readonly ILogger<RecursiveDeleter> log = A.Fake<ILogger<RecursiveDeleter>>();
    private readonly IAssetRepository assetRepository = A.Fake<IAssetRepository>();
    private readonly IAssetFolderRepository assetFolderRepository = A.Fake<IAssetFolderRepository>();
    private readonly ICommandBus commandBus = A.Fake<ICommandBus>();
    private readonly RecursiveDeleter sut;

    public RecursiveDeleterTests()
    {
        sut = new RecursiveDeleter(commandBus, assetRepository, assetFolderRepository, TestUtils.TypeRegistry, log);
    }

    [Fact]
    public void Should_return_assets_filter_for_events_filter()
    {
        Assert.Equal(StreamFilter.Prefix("assetFolder-"), sut.EventsFilter);
    }

    [Fact]
    public async Task Should_do_nothing_on_clear()
    {
        await ((IEventConsumer)sut).ClearAsync();
    }

    [Fact]
    public void Should_return_type_name_for_name()
    {
        Assert.Equal(nameof(RecursiveDeleter), ((IEventConsumer)sut).Name);
    }

    [Fact]
    public async Task Should_Not_invoke_delete_commands_if_event_restored()
    {
        var @event = new AssetFolderDeleted { AppId = AppId, AssetFolderId = DomainId.NewGuid() };

        await sut.On(Envelope.Create(@event).SetRestored());

        A.CallTo(() => commandBus.PublishAsync(A<ICommand>._, A<CancellationToken>._))
            .MustNotHaveHappened();
    }

    [Fact]
    public async Task Should_invoke_delete_commands_for_all_subfolders()
    {
        var @event = new AssetFolderDeleted { AppId = AppId, AssetFolderId = DomainId.NewGuid() };

        var childFolderId1 = DomainId.NewGuid();
        var childFolderId2 = DomainId.NewGuid();

        A.CallTo(() => assetFolderRepository.QueryChildIdsAsync(AppId.Id, @event.AssetFolderId, default))
            .Returns([childFolderId1, childFolderId2]);

        await sut.On(Envelope.Create(@event));

        A.CallTo(() => commandBus.PublishAsync(A<DeleteAssetFolder>.That.Matches(x => x.AssetFolderId == childFolderId1), default))
            .MustHaveHappened();

        A.CallTo(() => commandBus.PublishAsync(A<DeleteAssetFolder>.That.Matches(x => x.AssetFolderId == childFolderId2), default))
            .MustHaveHappened();
    }

    [Fact]
    public async Task Should_invoke_delete_commands_for_all_assets()
    {
        var @event = new AssetFolderDeleted { AppId = AppId, AssetFolderId = DomainId.NewGuid() };

        var childId1 = DomainId.NewGuid();
        var childId2 = DomainId.NewGuid();

        A.CallTo(() => assetRepository.QueryChildIdsAsync(AppId.Id, @event.AssetFolderId, default))
            .Returns([childId1, childId2]);

        await sut.On(Envelope.Create(@event));

        A.CallTo(() => commandBus.PublishAsync(A<DeleteAsset>.That.Matches(x => x.AssetId == childId1), default))
            .MustHaveHappened();

        A.CallTo(() => commandBus.PublishAsync(A<DeleteAsset>.That.Matches(x => x.AssetId == childId2), default))
            .MustHaveHappened();
    }

    [Fact]
    public async Task Should_ignore_exceptions()
    {
        var @event = new AssetFolderDeleted { AppId = AppId, AssetFolderId = DomainId.NewGuid() };

        var childId1 = DomainId.NewGuid();
        var childId2 = DomainId.NewGuid();

        A.CallTo(() => commandBus.PublishAsync(A<DeleteAsset>.That.Matches(x => x.AssetId == childId1), default))
            .Throws(new InvalidOperationException());

        A.CallTo(() => assetRepository.QueryChildIdsAsync(AppId.Id, @event.AssetFolderId, default))
            .Returns([childId1, childId2]);

        await sut.On(Envelope.Create(@event));

        A.CallTo(() => commandBus.PublishAsync(A<DeleteAsset>.That.Matches(x => x.AssetId == childId2), default))
            .MustHaveHappened();

        A.CallTo(log).Where(x => x.Method.Name == "Log")
            .MustHaveHappened();
    }
}
