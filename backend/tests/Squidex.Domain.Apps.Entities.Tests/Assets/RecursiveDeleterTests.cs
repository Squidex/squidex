// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using FakeItEasy;
using Squidex.Domain.Apps.Entities.Assets.Commands;
using Squidex.Domain.Apps.Entities.Assets.Repositories;
using Squidex.Domain.Apps.Events.Assets;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Commands;
using Squidex.Infrastructure.EventSourcing;
using Squidex.Infrastructure.Reflection;
using Squidex.Log;
using Xunit;

namespace Squidex.Domain.Apps.Entities.Assets
{
    public class RecursiveDeleterTests
    {
        private readonly ISemanticLog log = A.Fake<ISemanticLog>();
        private readonly IAssetRepository assetRepository = A.Fake<IAssetRepository>();
        private readonly IAssetFolderRepository assetFolderRepository = A.Fake<IAssetFolderRepository>();
        private readonly ICommandBus commandBus = A.Fake<ICommandBus>();
        private readonly NamedId<DomainId> appId = NamedId.Of(DomainId.NewGuid(), "my-app");
        private readonly RecursiveDeleter sut;

        public RecursiveDeleterTests()
        {
            var typeNameRegistry = new TypeNameRegistry().Map(typeof(AssetFolderDeleted));

            sut = new RecursiveDeleter(commandBus, assetRepository, assetFolderRepository, typeNameRegistry, log);
        }

        [Fact]
        public void Should_return_assets_filter_for_events_filter()
        {
            IEventConsumer consumer = sut;

            Assert.Equal("^assetFolder-", consumer.EventsFilter);
        }

        [Fact]
        public async Task Should_do_nothing_on_clear()
        {
            IEventConsumer consumer = sut;

            await consumer.ClearAsync();
        }

        [Fact]
        public void Should_return_type_name_for_name()
        {
            IEventConsumer consumer = sut;

            Assert.Equal(nameof(RecursiveDeleter), consumer.Name);
        }

        [Fact]
        public async Task Should_Not_invoke_delete_commands_if_event_restored()
        {
            var @event = new AssetFolderDeleted { AppId = appId, AssetFolderId = DomainId.NewGuid() };

            await sut.On(Envelope.Create(@event).SetRestored());

            A.CallTo(() => commandBus.PublishAsync(A<ICommand>._))
                .MustNotHaveHappened();
        }

        [Fact]
        public async Task Should_invoke_delete_commands_for_all_subfolders()
        {
            var @event = new AssetFolderDeleted { AppId = appId, AssetFolderId = DomainId.NewGuid() };

            var childFolderId1 = DomainId.NewGuid();
            var childFolderId2 = DomainId.NewGuid();

            A.CallTo(() => assetFolderRepository.QueryChildIdsAsync(appId.Id, @event.AssetFolderId, A<CancellationToken>._))
                .Returns(new List<DomainId> { childFolderId1, childFolderId2 });

            await sut.On(Envelope.Create(@event));

            A.CallTo(() => commandBus.PublishAsync(A<DeleteAssetFolder>.That.Matches(x => x.AssetFolderId == childFolderId1)))
                .MustHaveHappened();

            A.CallTo(() => commandBus.PublishAsync(A<DeleteAssetFolder>.That.Matches(x => x.AssetFolderId == childFolderId2)))
                .MustHaveHappened();
        }

        [Fact]
        public async Task Should_invoke_delete_commands_for_all_assets()
        {
            var @event = new AssetFolderDeleted { AppId = appId, AssetFolderId = DomainId.NewGuid() };

            var childId1 = DomainId.NewGuid();
            var childId2 = DomainId.NewGuid();

            A.CallTo(() => assetRepository.QueryChildIdsAsync(appId.Id, @event.AssetFolderId, A<CancellationToken>._))
                .Returns(new List<DomainId> { childId1, childId2 });

            await sut.On(Envelope.Create(@event));

            A.CallTo(() => commandBus.PublishAsync(A<DeleteAsset>.That.Matches(x => x.AssetId == childId1)))
                .MustHaveHappened();

            A.CallTo(() => commandBus.PublishAsync(A<DeleteAsset>.That.Matches(x => x.AssetId == childId2)))
                .MustHaveHappened();
        }

        [Fact]
        public async Task Should_ignore_exceptions()
        {
            var @event = new AssetFolderDeleted { AppId = appId, AssetFolderId = DomainId.NewGuid() };

            var childId1 = DomainId.NewGuid();
            var childId2 = DomainId.NewGuid();

            A.CallTo(() => commandBus.PublishAsync(A<DeleteAsset>.That.Matches(x => x.AssetId == childId1)))
                .Throws(new InvalidOperationException());

            A.CallTo(() => assetRepository.QueryChildIdsAsync(appId.Id, @event.AssetFolderId, A<CancellationToken>._))
                .Returns(new List<DomainId> { childId1, childId2 });

            await sut.On(Envelope.Create(@event));

            A.CallTo(() => commandBus.PublishAsync(A<DeleteAsset>.That.Matches(x => x.AssetId == childId2)))
                .MustHaveHappened();

            A.CallTo(() => log.Log(A<SemanticLogLevel>._, A<Exception?>._, A<LogFormatter>._!))
                .MustHaveHappened();
        }
    }
}
