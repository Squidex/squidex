﻿// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using FakeItEasy;
using Squidex.Domain.Apps.Core.Assets;
using Squidex.Domain.Apps.Core.Tags;
using Squidex.Domain.Apps.Entities.Assets.Commands;
using Squidex.Domain.Apps.Entities.Assets.State;
using Squidex.Domain.Apps.Entities.TestHelpers;
using Squidex.Domain.Apps.Events.Assets;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Assets;
using Squidex.Infrastructure.Commands;
using Squidex.Infrastructure.Log;
using Squidex.Infrastructure.Orleans;
using Xunit;

namespace Squidex.Domain.Apps.Entities.Assets
{
    public class AssetGrainTests : HandlerTestBase<AssetState>
    {
        private readonly ITagService tagService = A.Fake<ITagService>();
        private readonly IAssetQueryService assetQuery = A.Fake<IAssetQueryService>();
        private readonly IActivationLimit limit = A.Fake<IActivationLimit>();
        private readonly Guid parentId = Guid.NewGuid();
        private readonly Guid assetId = Guid.NewGuid();
        private readonly AssetFile file = new AssetFile("my-image.png", "image/png", 1024, () => new MemoryStream());
        private readonly AssetGrain sut;

        protected override Guid Id
        {
            get { return assetId; }
        }

        public AssetGrainTests()
        {
            A.CallTo(() => assetQuery.FindAssetFolderAsync(parentId))
                .Returns(new List<IAssetFolderEntity> { A.Fake<IAssetFolderEntity>() });

            A.CallTo(() => tagService.NormalizeTagsAsync(AppId, TagGroups.Assets, A<HashSet<string>>.Ignored, A<HashSet<string>>.Ignored))
                .ReturnsLazily(x => Task.FromResult(x.GetArgument<HashSet<string>>(2)?.ToDictionary(x => x)!));

            sut = new AssetGrain(Store, tagService, assetQuery, limit, A.Dummy<ISemanticLog>());
            sut.ActivateAsync(Id).Wait();
        }

        [Fact]
        public void Should_set_limit()
        {
            A.CallTo(() => limit.SetLimit(5000, TimeSpan.FromMinutes(5)))
                .MustHaveHappened();
        }

        [Fact]
        public async Task Command_should_throw_exception_if_asset_is_deleted()
        {
            await ExecuteCreateAsync();
            await ExecuteDeleteAsync();

            await Assert.ThrowsAsync<DomainException>(ExecuteUpdateAsync);
        }

        [Fact]
        public async Task Create_should_create_events_and_update_state()
        {
            var command = new CreateAsset { File = file, FileHash = "NewHash" };

            var result = await PublishAsync(command);

            result.ShouldBeEquivalent2(sut.Snapshot);

            Assert.Equal(0, sut.Snapshot.FileVersion);
            Assert.Equal(command.FileHash, sut.Snapshot.FileHash);

            LastEvents
                .ShouldHaveSameEvents(
                    CreateAssetEvent(new AssetCreated
                    {
                        FileName = file.FileName,
                        FileSize = file.FileSize,
                        FileHash = command.FileHash,
                        FileVersion = 0,
                        Metadata = new AssetMetadata(),
                        MimeType = file.MimeType,
                        Tags = new HashSet<string>(),
                        Slug = file.FileName.ToAssetSlug()
                    })
                );
        }

        [Fact]
        public async Task Update_should_create_events_and_update_state()
        {
            var command = new UpdateAsset { File = file, FileHash = "NewHash" };

            await ExecuteCreateAsync();

            var result = await PublishAsync(command);

            result.ShouldBeEquivalent2(sut.Snapshot);

            Assert.Equal(1, sut.Snapshot.FileVersion);
            Assert.Equal(command.FileHash, sut.Snapshot.FileHash);

            LastEvents
                .ShouldHaveSameEvents(
                    CreateAssetEvent(new AssetUpdated
                    {
                        FileSize = file.FileSize,
                        FileHash = command.FileHash,
                        FileVersion = 1,
                        Metadata = new AssetMetadata(),
                        MimeType = file.MimeType
                    })
                );
        }

        [Fact]
        public async Task AnnotateName_should_create_events_and_update_state()
        {
            var command = new AnnotateAsset { FileName = "My New Image.png" };

            await ExecuteCreateAsync();

            var result = await PublishIdempotentAsync(command);

            result.ShouldBeEquivalent2(sut.Snapshot);

            Assert.Equal(command.FileName, sut.Snapshot.FileName);

            LastEvents
                .ShouldHaveSameEvents(
                    CreateAssetEvent(new AssetAnnotated { FileName = command.FileName })
                );
        }

        [Fact]
        public async Task AnnotateSlug_should_create_events_and_update_state()
        {
            var command = new AnnotateAsset { Slug = "my-new-image.png" };

            await ExecuteCreateAsync();

            var result = await PublishIdempotentAsync(command);

            result.ShouldBeEquivalent2(sut.Snapshot);

            Assert.Equal(command.Slug, sut.Snapshot.Slug);

            LastEvents
                .ShouldHaveSameEvents(
                    CreateAssetEvent(new AssetAnnotated { Slug = command.Slug })
                );
        }

        [Fact]
        public async Task AnnotateMetadata_should_create_events_and_update_state()
        {
            var command = new AnnotateAsset { Metadata = new AssetMetadata().SetPixelWidth(800) };

            await ExecuteCreateAsync();

            var result = await PublishIdempotentAsync(command);

            result.ShouldBeEquivalent2(sut.Snapshot);

            Assert.Equal(command.Metadata, sut.Snapshot.Metadata);

            LastEvents
                .ShouldHaveSameEvents(
                    CreateAssetEvent(new AssetAnnotated { Metadata = command.Metadata })
                );
        }

        [Fact]
        public async Task AnnotateTags_should_create_events_and_update_state()
        {
            var command = new AnnotateAsset { Tags = new HashSet<string> { "tag1" } };

            await ExecuteCreateAsync();

            var result = await PublishIdempotentAsync(command);

            result.ShouldBeEquivalent2(sut.Snapshot);

            LastEvents
                .ShouldHaveSameEvents(
                    CreateAssetEvent(new AssetAnnotated { Tags = new HashSet<string> { "tag1" } })
                );
        }

        [Fact]
        public async Task Move_should_create_events_and_update_state()
        {
            var command = new MoveAsset { ParentId = parentId };

            await ExecuteCreateAsync();

            var result = await PublishIdempotentAsync(command);

            result.ShouldBeEquivalent2(sut.Snapshot);

            Assert.Equal(parentId, sut.Snapshot.ParentId);

            LastEvents
                .ShouldHaveSameEvents(
                    CreateAssetEvent(new AssetMoved { ParentId = parentId })
                );
        }

        [Fact]
        public async Task Delete_should_create_events_with_total_file_size()
        {
            var command = new DeleteAsset();

            await ExecuteCreateAsync();
            await ExecuteUpdateAsync();

            var result = await PublishAsync(command);

            result.ShouldBeEquivalent2(new EntitySavedResult(2));

            Assert.True(sut.Snapshot.IsDeleted);

            LastEvents
                .ShouldHaveSameEvents(
                    CreateAssetEvent(new AssetDeleted { DeletedSize = 2048 })
                );
        }

        private Task ExecuteCreateAsync()
        {
            return PublishAsync(new CreateAsset { File = file });
        }

        private Task ExecuteUpdateAsync()
        {
            return PublishAsync(new UpdateAsset { File = file });
        }

        private Task ExecuteDeleteAsync()
        {
            return PublishAsync(new DeleteAsset());
        }

        protected T CreateAssetEvent<T>(T @event) where T : AssetEvent
        {
            @event.AssetId = assetId;

            return CreateEvent(@event);
        }

        protected T CreateAssetCommand<T>(T command) where T : AssetCommand
        {
            command.AssetId = assetId;

            return CreateCommand(command);
        }

        private async Task<object?> PublishIdempotentAsync(AssetCommand command)
        {
            var result = await PublishAsync(command);

            var previousSnapshot = sut.Snapshot;
            var previousVersion = sut.Snapshot.Version;

            await PublishAsync(command);

            Assert.Same(previousSnapshot, sut.Snapshot);
            Assert.Equal(previousVersion, sut.Snapshot.Version);

            return result;
        }

        private async Task<object?> PublishAsync(AssetCommand command)
        {
            var result = await sut.ExecuteAsync(CreateAssetCommand(command));

            return result.Value;
        }
    }
}
