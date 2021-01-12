// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Collections.Generic;
using System.Threading.Tasks;
using FakeItEasy;
using Squidex.Domain.Apps.Entities.Assets.Commands;
using Squidex.Domain.Apps.Entities.TestHelpers;
using Squidex.Domain.Apps.Events.Assets;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Commands;
using Squidex.Log;
using Xunit;

namespace Squidex.Domain.Apps.Entities.Assets.DomainObject
{
    public class AssetFolderDomainObjectTests : HandlerTestBase<AssetFolderDomainObject.State>
    {
        private readonly IAssetQueryService assetQuery = A.Fake<IAssetQueryService>();
        private readonly DomainId parentId = DomainId.NewGuid();
        private readonly DomainId assetFolderId = DomainId.NewGuid();
        private readonly AssetFolderDomainObject sut;

        protected override DomainId Id
        {
            get { return DomainId.Combine(AppId, assetFolderId); }
        }

        public AssetFolderDomainObjectTests()
        {
            A.CallTo(() => assetQuery.FindAssetFolderAsync(AppId, parentId))
                .Returns(new List<IAssetFolderEntity> { A.Fake<IAssetFolderEntity>() });

            sut = new AssetFolderDomainObject(Store, A.Dummy<ISemanticLog>(), assetQuery);
            sut.Setup(Id);
        }

        [Fact]
        public async Task Command_should_throw_exception_if_rule_is_deleted()
        {
            await ExecuteCreateAsync();
            await ExecuteDeleteAsync();

            await Assert.ThrowsAsync<DomainException>(ExecuteUpdateAsync);
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

            result.ShouldBeEquivalent(new EntitySavedResult(1));

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

        protected T CreateAssetFolderEvent<T>(T @event) where T : AssetFolderEvent
        {
            @event.AssetFolderId = assetFolderId;

            return CreateEvent(@event);
        }

        protected T CreateAssetFolderCommand<T>(T command) where T : AssetFolderCommand
        {
            command.AssetFolderId = assetFolderId;

            return CreateCommand(command);
        }

        private async Task<object?> PublishIdempotentAsync(AssetFolderCommand command)
        {
            var result = await PublishAsync(command);

            var previousSnapshot = sut.Snapshot;
            var previousVersion = sut.Snapshot.Version;

            await PublishAsync(command);

            Assert.Same(previousSnapshot, sut.Snapshot);
            Assert.Equal(previousVersion, sut.Snapshot.Version);

            return result;
        }

        private async Task<object?> PublishAsync(AssetFolderCommand command)
        {
            var result = await sut.ExecuteAsync(CreateAssetFolderCommand(command));

            return result;
        }
    }
}
