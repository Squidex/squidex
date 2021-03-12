// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Threading.Tasks;
using FakeItEasy;
using Squidex.Caching;
using Squidex.Domain.Apps.Entities.Assets.Commands;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Commands;
using Xunit;

namespace Squidex.Domain.Apps.Entities.Assets.DomainObject
{
    public class AssetFolderResolverCommandMiddlewareTests
    {
        private readonly ILocalCache localCache = new AsyncLocalCache();
        private readonly IAssetQueryService assetQuery = A.Fake<IAssetQueryService>();
        private readonly ICommandBus commandBus = A.Fake<ICommandBus>();
        private readonly NamedId<DomainId> appId = NamedId.Of(DomainId.NewGuid(), "my-app");
        private readonly AssetFolderResolverCommandMiddleware sut;

        public AssetFolderResolverCommandMiddlewareTests()
        {
            localCache.StartContext();

            sut = new AssetFolderResolverCommandMiddleware(localCache, assetQuery);
        }

        [Theory]
        [InlineData("")]
        [InlineData(" ")]
        [InlineData("/")]
        [InlineData("\\")]
        public async Task Should_resolve_root_id_for_empty_path(string path)
        {
            var folderId = await ResolveAssync(path);

            Assert.Equal(DomainId.Empty, folderId);
        }

        [Fact]
        public async Task Should_create_and_cache_level1_folder()
        {
            var folderId11_1 = await ResolveAssync("level1");
            var folderId11_2 = await ResolveAssync("level1");

            Assert.NotEqual(DomainId.Empty, folderId11_1);
            Assert.NotEqual(DomainId.Empty, folderId11_2);

            Assert.Equal(folderId11_2, folderId11_1);

            A.CallTo(() => commandBus.PublishAsync(A<CreateAssetFolder>.That.Matches(x => x.FolderName == "level1")))
                .MustHaveHappenedOnceExactly();
        }

        [Fact]
        public async Task Should_create_and_cache_recursively()
        {
            var folderId21_1 = await ResolveAssync("level1/level2");
            var folderId21_2 = await ResolveAssync("level1/level2");

            Assert.NotEqual(DomainId.Empty, folderId21_1);
            Assert.NotEqual(DomainId.Empty, folderId21_2);

            Assert.Equal(folderId21_1, folderId21_2);

            A.CallTo(() => commandBus.PublishAsync(A<CreateAssetFolder>.That.Matches(x => x.FolderName == "level1")))
                .MustHaveHappenedOnceExactly();

            A.CallTo(() => commandBus.PublishAsync(A<CreateAssetFolder>.That.Matches(x => x.FolderName == "level2")))
                .MustHaveHappenedOnceExactly();
        }

        [Fact]
        public async Task Should_cache_folders_on_same_level()
        {
            var folder11 = CreateFolder("level1_1");
            var folder12 = CreateFolder("level1_2");

            A.CallTo(() => assetQuery.QueryAssetFoldersAsync(appId.Id, DomainId.Empty))
                .Returns(ResultList.CreateFrom(2, folder11, folder12));

            var folderId11 = await ResolveAssync(folder11.FolderName);
            var folderId12 = await ResolveAssync(folder12.FolderName);

            Assert.Equal(folder11.Id, folderId11);
            Assert.Equal(folder12.Id, folderId12);

            A.CallTo(() => assetQuery.QueryAssetFoldersAsync(appId.Id, A<DomainId>._))
                .MustHaveHappenedOnceExactly();

            A.CallTo(() => commandBus.PublishAsync(A<ICommand>._))
                .MustNotHaveHappened();
        }

        [Fact]
        public async Task Should_resolve_recursively()
        {
            var folder11 = CreateFolder("level1");
            var folder21 = CreateFolder("level2");

            A.CallTo(() => assetQuery.QueryAssetFoldersAsync(appId.Id, DomainId.Empty))
                .Returns(ResultList.CreateFrom(1, folder11));

            A.CallTo(() => assetQuery.QueryAssetFoldersAsync(appId.Id, folder11.Id))
                .Returns(ResultList.CreateFrom(1, folder21));

            var folderId2 = await ResolveAssync("level1/level2");

            Assert.Equal(folder21.Id, folderId2);

            A.CallTo(() => commandBus.PublishAsync(A<ICommand>._))
                .MustNotHaveHappened();
        }

        [Fact]
        public async Task Should_resolve_recursively_and_create_folder()
        {
            var folder11 = CreateFolder("level1");
            var folder21 = CreateFolder("level2");

            A.CallTo(() => assetQuery.QueryAssetFoldersAsync(appId.Id, DomainId.Empty))
                .Returns(ResultList.CreateFrom(1, folder11));

            A.CallTo(() => assetQuery.QueryAssetFoldersAsync(appId.Id, folder11.Id))
                .Returns(ResultList.CreateFrom(1, folder21));

            await ResolveAssync("level1/level2");

            var folderId3 = await ResolveAssync("level1/level2/level3");

            Assert.NotEqual(DomainId.Empty, folderId3);

            A.CallTo(() => assetQuery.QueryAssetFoldersAsync(appId.Id, DomainId.Empty))
                .MustHaveHappenedOnceExactly();

            A.CallTo(() => assetQuery.QueryAssetFoldersAsync(appId.Id, folder11.Id))
                .MustHaveHappenedOnceExactly();

            A.CallTo(() => commandBus.PublishAsync(A<CreateAssetFolder>.That.Matches(x => x.FolderName == "level3" && x.ParentId == folder21.Id)))
                .MustHaveHappenedOnceExactly();
        }

        private async Task<DomainId> ResolveAssync(string path)
        {
            var command = new CreateAsset
            {
                ParentPath = path, AppId = appId
            };

            var commandContext = new CommandContext(command, commandBus);

            await sut.HandleAsync(commandContext);

            return command.ParentId;
        }

        private static IAssetFolderEntity CreateFolder(string name)
        {
            var assetFolder = A.Fake<IAssetFolderEntity>();

            A.CallTo(() => assetFolder.FolderName)
                .Returns(name);

            A.CallTo(() => assetFolder.Id)
                .Returns(DomainId.NewGuid());

            return assetFolder;
        }
    }
}
