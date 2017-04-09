// ==========================================================================
//  AssetCommandHandlerTests.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System;
using System.Threading.Tasks;
using Squidex.Infrastructure.CQRS.Commands;
using Squidex.Write.Assets.Commands;
using Squidex.Write.TestHelpers;
using Xunit;

// ReSharper disable ConvertToConstant.Local

namespace Squidex.Write.Assets
{
    public class AssetCommandHandlerTests : HandlerTestBase<AssetDomainObject>
    {
        private readonly AssetCommandHandler sut;
        private readonly AssetDomainObject asset;
        private readonly Guid assetId = Guid.NewGuid();
        private readonly string fileName = "my-image.png";
        private readonly string mimeType = "image/png";
        private readonly long fileSize = 1024;

        public AssetCommandHandlerTests()
        {
            asset = new AssetDomainObject(assetId, 0);

            sut = new AssetCommandHandler(Handler);
        }

        [Fact]
        public async Task Create_should_create_asset()
        {
            var context = CreateContextForCommand(new CreateAsset { AssetId = assetId, FileName = fileName, FileSize = fileSize, MimeType = mimeType });

            await TestCreate(asset, async _ =>
            {
                await sut.HandleAsync(context);
            });

            Assert.Equal(assetId, context.Result<EntityCreatedResult<Guid>>().IdOrValue);
        }

        [Fact]
        public async Task Update_should_update_domain_object()
        {
            CreateAsset();

            var context = CreateContextForCommand(new UpdateAsset { AssetId = assetId, FileSize = fileSize, MimeType = mimeType });

            await TestUpdate(asset, async _ =>
            {
                await sut.HandleAsync(context);
            });
        }

        [Fact]
        public async Task Rename_should_update_domain_object()
        {
            CreateAsset();

            var context = CreateContextForCommand(new RenameAsset { AssetId = assetId, FileName = "my-new-image.png" });

            await TestUpdate(asset, async _ =>
            {
                await sut.HandleAsync(context);
            });
        }

        [Fact]
        public async Task Delete_should_update_domain_object()
        {
            CreateAsset();

            var command = CreateContextForCommand(new DeleteAsset { AssetId = assetId });

            await TestUpdate(asset, async _ =>
            {
                await sut.HandleAsync(command);
            });
        }

        private void CreateAsset()
        {
            asset.Create(new CreateAsset { FileName = fileName, FileSize = fileSize, MimeType = mimeType });
        }
    }
}
