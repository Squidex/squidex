// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FakeItEasy;
using Squidex.Domain.Apps.Entities.Assets.Commands;
using Squidex.Domain.Apps.Entities.Assets.Repositories;
using Squidex.Domain.Apps.Events.Assets;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Commands;
using Squidex.Infrastructure.EventSourcing;
using Squidex.Infrastructure.Log;
using Squidex.Infrastructure.Reflection;
using Xunit;

namespace Squidex.Domain.Apps.Entities.Assets
{
    public class RecursiveDeleterTests
    {
        private readonly TypeNameRegistry typeNameRegistry = new TypeNameRegistry();
        private readonly ISemanticLog log = A.Fake<ISemanticLog>();
        private readonly IAssetRepository assetRepository = A.Fake<IAssetRepository>();
        private readonly IAssetFolderRepository assetFolderRepository = A.Fake<IAssetFolderRepository>();
        private readonly ICommandBus commandBus = A.Fake<ICommandBus>();
        private readonly NamedId<Guid> appId = NamedId.Of(Guid.NewGuid(), "my-app");
        private readonly RecursiveDeleter sut;

        public RecursiveDeleterTests()
        {
            typeNameRegistry.Map(typeof(AssetFolderDeleted));

            sut = new RecursiveDeleter(commandBus, assetRepository, assetFolderRepository, typeNameRegistry, log);
        }

        [Fact]
        public async Task Should_do_nothing_on_clear()
        {
            await sut.ClearAsync();
        }

        [Fact]
        public async Task Should_invoke_delete_commands_for_all_subfolders()
        {
            var @event = new AssetFolderDeleted { AppId = appId, AssetFolderId = Guid.NewGuid() };

            var childFolderId1 = Guid.NewGuid();
            var childFolderId2 = Guid.NewGuid();

            A.CallTo(() => assetFolderRepository.QueryChildIdsAsync(appId.Id, @event.AssetFolderId))
                .Returns(new List<Guid> { childFolderId1, childFolderId2 });

            await sut.On(Envelope.Create(@event));

            A.CallTo(() => commandBus.PublishAsync(A<DeleteAssetFolder>.That.Matches(x => x.AssetFolderId == childFolderId1)))
                .MustHaveHappened();

            A.CallTo(() => commandBus.PublishAsync(A<DeleteAssetFolder>.That.Matches(x => x.AssetFolderId == childFolderId2)))
                .MustHaveHappened();
        }

        [Fact]
        public async Task Should_invoke_delete_commands_for_all_assets()
        {
            var @event = new AssetFolderDeleted { AppId = appId, AssetFolderId = Guid.NewGuid() };

            var childId1 = Guid.NewGuid();
            var childId2 = Guid.NewGuid();

            A.CallTo(() => assetRepository.QueryChildIdsAsync(appId.Id, @event.AssetFolderId))
                .Returns(new List<Guid> { childId1, childId2 });

            await sut.On(Envelope.Create(@event));

            A.CallTo(() => commandBus.PublishAsync(A<DeleteAsset>.That.Matches(x => x.AssetId == childId1)))
                .MustHaveHappened();

            A.CallTo(() => commandBus.PublishAsync(A<DeleteAsset>.That.Matches(x => x.AssetId == childId2)))
                .MustHaveHappened();
        }

        [Fact]
        public async Task Should_ignore_exceptions()
        {
            var @event = new AssetFolderDeleted { AppId = appId, AssetFolderId = Guid.NewGuid() };

            var childId1 = Guid.NewGuid();
            var childId2 = Guid.NewGuid();

            A.CallTo(() => commandBus.PublishAsync(A<DeleteAsset>.That.Matches(x => x.AssetId == childId1)))
                .Throws(new InvalidOperationException());

            A.CallTo(() => assetRepository.QueryChildIdsAsync(appId.Id, @event.AssetFolderId))
                .Returns(new List<Guid> { childId1, childId2 });

            await sut.On(Envelope.Create(@event));

            A.CallTo(() => commandBus.PublishAsync(A<DeleteAsset>.That.Matches(x => x.AssetId == childId2)))
                .MustHaveHappened();

            A.CallTo(() => log.Log(A<SemanticLogLevel>._, A<Exception?>._, A<LogFormatter>._!))
                .MustHaveHappened();
        }
    }
}
