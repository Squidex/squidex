// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.IO;
using System.Threading;
using System.Threading.Tasks;
using FakeItEasy;
using Orleans;
using Squidex.Assets;
using Squidex.Domain.Apps.Entities.Apps.Commands;
using Squidex.Domain.Apps.Entities.Apps.State;
using Squidex.Domain.Apps.Entities.TestHelpers;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Commands;
using Squidex.Infrastructure.Validation;
using Xunit;

namespace Squidex.Domain.Apps.Entities.Apps
{
    public class AppCommandMiddlewareTests : HandlerTestBase<AppState>
    {
        private readonly IContextProvider contextProvider = A.Fake<IContextProvider>();
        private readonly IAppImageStore appImageStore = A.Fake<IAppImageStore>();
        private readonly IAssetThumbnailGenerator assetThumbnailGenerator = A.Fake<IAssetThumbnailGenerator>();
        private readonly NamedId<DomainId> appId = NamedId.Of(DomainId.NewGuid(), "my-app");
        private readonly Context requestContext = Context.Anonymous();
        private readonly AppCommandMiddleware sut;

        public sealed class MyCommand : SquidexCommand
        {
        }

        protected override DomainId Id
        {
            get { return appId.Id; }
        }

        public AppCommandMiddlewareTests()
        {
            A.CallTo(() => contextProvider.Context)
                .Returns(requestContext);

            sut = new AppCommandMiddleware(A.Fake<IGrainFactory>(), appImageStore, assetThumbnailGenerator, contextProvider);
        }

        [Fact]
        public async Task Should_replace_context_app_with_grain_result()
        {
            var result = A.Fake<IAppEntity>();

            var command = CreateCommand(new MyCommand());
            var context = CreateContextForCommand(command);

            context.Complete(result);

            await sut.HandleAsync(context);

            Assert.Same(result, requestContext.App);
        }

        [Fact]
        public async Task Should_upload_image_to_store()
        {
            var file = new NoopAssetFile();

            var command = CreateCommand(new UploadAppImage { AppId = appId, File = file });
            var context = CreateContextForCommand(command);

            A.CallTo(() => assetThumbnailGenerator.GetImageInfoAsync(A<Stream>._))
                .Returns(new ImageInfo(100, 100, false));

            await sut.HandleAsync(context);

            A.CallTo(() => appImageStore.UploadAsync(appId.Id, A<Stream>._, A<CancellationToken>._))
                .MustHaveHappened();
        }

        [Fact]
        public async Task Should_throw_exception_when_file_to_upload_is_not_an_image()
        {
            var stream = new MemoryStream();

            var file = new NoopAssetFile();

            var command = CreateCommand(new UploadAppImage { AppId = appId, File = file });
            var context = CreateContextForCommand(command);

            A.CallTo(() => assetThumbnailGenerator.GetImageInfoAsync(stream))
                .Returns(Task.FromResult<ImageInfo?>(null));

            await Assert.ThrowsAsync<ValidationException>(() => sut.HandleAsync(context));
        }
    }
}
