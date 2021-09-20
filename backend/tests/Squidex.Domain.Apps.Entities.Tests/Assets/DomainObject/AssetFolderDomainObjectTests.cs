// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using FakeItEasy;
using Microsoft.Extensions.DependencyInjection;
using Squidex.Domain.Apps.Entities.Apps;
using Squidex.Domain.Apps.Entities.Assets.Commands;
using Squidex.Domain.Apps.Entities.Contents.Repositories;
using Squidex.Domain.Apps.Entities.TestHelpers;
using Squidex.Domain.Apps.Events.Assets;
using Squidex.Infrastructure;
using Squidex.Log;
using Xunit;

namespace Squidex.Domain.Apps.Entities.Assets.DomainObject
{
    public class AssetFolderDomainObjectTests : HandlerTestBase<AssetFolderDomainObject.State>
    {
        private readonly IAppEntity app;
        private readonly IAppProvider appProvider = A.Fake<IAppProvider>();
        private readonly IAssetQueryService assetQuery = A.Fake<IAssetQueryService>();
        private readonly IContentRepository contentRepository = A.Fake<IContentRepository>();
        private readonly DomainId parentId = DomainId.NewGuid();
        private readonly DomainId assetFolderId = DomainId.NewGuid();
        private readonly AssetFolderDomainObject sut;

        protected override DomainId Id
        {
            get => DomainId.Combine(AppId, assetFolderId);
        }

        public AssetFolderDomainObjectTests()
        {
            app = Mocks.App(AppNamedId, Language.DE);

            A.CallTo(() => appProvider.GetAppAsync(AppId, false, default))
                .Returns(app);

            A.CallTo(() => assetQuery.FindAssetFolderAsync(AppId, parentId, A<CancellationToken>._))
                .Returns(new List<IAssetFolderEntity> { A.Fake<IAssetFolderEntity>() });

            var log = A.Fake<ISemanticLog>();

            var serviceProvider =
                new ServiceCollection()
                    .AddSingleton(appProvider)
                    .AddSingleton(assetQuery)
                    .AddSingleton(contentRepository)
                    .AddSingleton(log)
                    .BuildServiceProvider();

            sut = new AssetFolderDomainObject(PersistenceFactory, log, serviceProvider);
#pragma warning disable MA0056 // Do not call overridable members in constructor
            sut.Setup(Id);
#pragma warning restore MA0056 // Do not call overridable members in constructor
        }

        [Fact]
        public async Task Command_should_throw_exception_if_rule_is_deleted()
        {
            await ExecuteCreateAsync();
            await ExecuteDeleteAsync();

            await Assert.ThrowsAsync<DomainObjectDeletedException>(ExecuteUpdateAsync);
        }

        [Fact]
        public async Task Create_should_create_events_and_set_intitial_state()
        {
            var command = new CreateAssetFolder { FolderName = "New Name" };

            var result = await PublishAsync(command);

            result.ShouldBeEquivalent(sut.Snapshot);

            Assert.Equal(command.FolderName, sut.Snapshot.FolderName);

            LastEvents
                .ShouldHaveSameEvents(
                    CreateAssetFolderEvent(new AssetFolderCreated { FolderName = command.FolderName })
                );
        }

        [Fact]
        public async Task Update_should_create_events_and_update_state()
        {
            var command = new RenameAssetFolder { FolderName = "New Name" };

            await ExecuteCreateAsync();

            var result = await PublishIdempotentAsync(command);

            result.ShouldBeEquivalent(sut.Snapshot);

            Assert.Equal(command.FolderName, sut.Snapshot.FolderName);

            LastEvents
                .ShouldHaveSameEvents(
                    CreateAssetFolderEvent(new AssetFolderRenamed { FolderName = command.FolderName })
                );
        }

        [Fact]
        public async Task Move_should_create_events_and_update_state()
        {
            var command = new MoveAssetFolder { ParentId = parentId };

            await ExecuteCreateAsync();

            var result = await PublishIdempotentAsync(command);

            result.ShouldBeEquivalent(sut.Snapshot);

            Assert.Equal(parentId, sut.Snapshot.ParentId);

            LastEvents
                .ShouldHaveSameEvents(
                    CreateAssetFolderEvent(new AssetFolderMoved { ParentId = parentId })
                );
        }

        [Fact]
        public async Task Delete_should_create_events_with_total_file_size()
        {
            var command = new DeleteAssetFolder();

            await ExecuteCreateAsync();

            var result = await PublishAsync(command);

            result.ShouldBeEquivalent(None.Value);

            Assert.True(sut.Snapshot.IsDeleted);

            LastEvents
                .ShouldHaveSameEvents(
                    CreateAssetFolderEvent(new AssetFolderDeleted())
                );
        }

        private Task ExecuteCreateAsync()
        {
            return PublishAsync(new CreateAssetFolder { FolderName = "My Folder" });
        }

        private Task ExecuteUpdateAsync()
        {
            return PublishAsync(new RenameAssetFolder { FolderName = "My Folder" });
        }

        private Task ExecuteDeleteAsync()
        {
            return PublishAsync(new DeleteAssetFolder());
        }

        private T CreateAssetFolderEvent<T>(T @event) where T : AssetFolderEvent
        {
            @event.AssetFolderId = assetFolderId;

            return CreateEvent(@event);
        }

        private T CreateAssetFolderCommand<T>(T command) where T : AssetFolderCommand
        {
            command.AssetFolderId = assetFolderId;

            return CreateCommand(command);
        }

        private Task<object> PublishIdempotentAsync(AssetFolderCommand command)
        {
            return PublishIdempotentAsync(sut, CreateAssetFolderCommand(command));
        }

        private async Task<object> PublishAsync(AssetFolderCommand command)
        {
            var result = await sut.ExecuteAsync(CreateAssetFolderCommand(command));

            return result.Payload;
        }
    }
}
