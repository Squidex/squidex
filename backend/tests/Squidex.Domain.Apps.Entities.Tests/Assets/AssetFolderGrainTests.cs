// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FakeItEasy;
using Squidex.Domain.Apps.Entities.Assets.Commands;
using Squidex.Domain.Apps.Entities.Assets.State;
using Squidex.Domain.Apps.Entities.TestHelpers;
using Squidex.Domain.Apps.Events.Assets;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Commands;
using Squidex.Infrastructure.Log;
using Squidex.Infrastructure.Orleans;
using Xunit;

namespace Squidex.Domain.Apps.Entities.Assets
{
    public class AssetFolderGrainTests : HandlerTestBase<AssetFolderState>
    {
        private readonly IAssetQueryService assetQuery = A.Fake<IAssetQueryService>();
        private readonly IActivationLimit limit = A.Fake<IActivationLimit>();
        private readonly Guid parentId = Guid.NewGuid();
        private readonly Guid assetFolderId = Guid.NewGuid();
        private readonly AssetFolderGrain sut;

        protected override Guid Id
        {
            get { return assetFolderId; }
        }

        public AssetFolderGrainTests()
        {
            A.CallTo(() => assetQuery.FindAssetFolderAsync(parentId))
                .Returns(new List<IAssetFolderEntity> { A.Fake<IAssetFolderEntity>() });

            sut = new AssetFolderGrain(Store, assetQuery, limit, A.Dummy<ISemanticLog>());
            sut.ActivateAsync(Id).Wait();
        }

        [Fact]
        public void Should_set_limit()
        {
            A.CallTo(() => limit.SetLimit(5000, TimeSpan.FromMinutes(5)))
                .MustHaveHappened();
        }

        [Fact]
        public async Task Command_should_throw_exception_if_rule_is_deleted()
        {
            await ExecuteCreateAsync();
            await ExecuteDeleteAsync();

            await Assert.ThrowsAsync<DomainException>(ExecuteUpdateAsync);
        }

        [Fact]
        public async Task Create_should_create_events_and_update_state()
        {
            var command = new CreateAssetFolder { FolderName = "New Name" };

            var result = await sut.ExecuteAsync(CreateAssetFolderCommand(command));

            result.ShouldBeEquivalent(sut.Snapshot);

            Assert.Equal(command.FolderName, sut.Snapshot.FolderName);

            LastEvents
                .ShouldHaveSameEvents(
                    CreateAssetFolderEvent(new AssetFolderCreated
                    {
                        FolderName = command.FolderName
                    })
                );
        }

        [Fact]
        public async Task Update_should_create_events_and_update_state()
        {
            var command = new RenameAssetFolder { FolderName = "New Name" };

            await ExecuteCreateAsync();

            var result = await sut.ExecuteAsync(CreateAssetFolderCommand(command));

            result.ShouldBeEquivalent(sut.Snapshot);

            Assert.Equal(command.FolderName, sut.Snapshot.FolderName);

            LastEvents
                .ShouldHaveSameEvents(
                    CreateAssetFolderEvent(new AssetFolderRenamed
                    {
                        FolderName = command.FolderName
                    })
                );
        }

        [Fact]
        public async Task Move_should_create_events_and_update_state()
        {
            var command = new MoveAssetFolder { ParentId = parentId };

            await ExecuteCreateAsync();

            var result = await sut.ExecuteAsync(CreateAssetFolderCommand(command));

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

            var result = await sut.ExecuteAsync(CreateAssetFolderCommand(command));

            result.ShouldBeEquivalent(new EntitySavedResult(1));

            Assert.True(sut.Snapshot.IsDeleted);

            LastEvents
                .ShouldHaveSameEvents(
                    CreateAssetFolderEvent(new AssetFolderDeleted())
                );
        }

        private Task ExecuteCreateAsync()
        {
            return sut.ExecuteAsync(CreateAssetFolderCommand(new CreateAssetFolder { FolderName = "My Folder" }));
        }

        private Task ExecuteUpdateAsync()
        {
            return sut.ExecuteAsync(CreateAssetFolderCommand(new RenameAssetFolder { FolderName = "My Folder" }));
        }

        private Task ExecuteDeleteAsync()
        {
            return sut.ExecuteAsync(CreateAssetFolderCommand(new DeleteAssetFolder()));
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
    }
}
