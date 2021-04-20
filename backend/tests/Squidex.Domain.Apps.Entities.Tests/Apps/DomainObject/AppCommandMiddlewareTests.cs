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
using Squidex.Domain.Apps.Entities.TestHelpers;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Commands;
using Squidex.Infrastructure.Orleans;
using Squidex.Infrastructure.Validation;
using Xunit;

namespace Squidex.Domain.Apps.Entities.Apps.DomainObject
{
    public class AppCommandMiddlewareTests : HandlerTestBase<AppDomainObject.State>
    {
        private readonly IGrainFactory grainFactory = A.Fake<IGrainFactory>();
        private readonly IContextProvider contextProvider = A.Fake<IContextProvider>();
        private readonly IAppImageStore appImageStore = A.Fake<IAppImageStore>();
        private readonly IAssetThumbnailGenerator assetThumbnailGenerator = A.Fake<IAssetThumbnailGenerator>();
        private readonly NamedId<DomainId> appId = NamedId.Of(DomainId.NewGuid(), "my-app");
        private readonly Context requestContext;
        private readonly AppCommandMiddleware sut;

        public sealed class MyCommand : SquidexCommand
        {
        }

        protected override DomainId Id
        {
            get => appId.Id;
        }

        public AppCommandMiddlewareTests()
        {
            requestContext = Context.Anonymous(Mocks.App(appId));

            A.CallTo(() => contextProvider.Context)
                .Returns(requestContext);

            sut = new AppCommandMiddleware(grainFactory, appImageStore, assetThumbnailGenerator, contextProvider);
        }

        [Fact]
        public async Task Should_replace_context_app_with_grain_result()
        {
            var result = A.Fake<IAppEntity>();

            await HandleAsync(new UpdateApp(), result);

            Assert.Same(result, requestContext.App);
        }

        [Fact]
        public async Task Should_upload_image_to_store()
        {
            var file = new NoopAssetFile();

            A.CallTo(() => assetThumbnailGenerator.GetImageInfoAsync(A<Stream>._))
                .Returns(new ImageInfo(100, 100, false));

            await HandleAsync(new UploadAppImage { File = file }, None.Value);

            A.CallTo(() => appImageStore.UploadAsync(appId.Id, A<Stream>._, A<CancellationToken>._))
                .MustHaveHappened();
        }

        [Fact]
        public async Task Should_throw_exception_if_file_to_upload_is_not_an_image()
        {
            var file = new NoopAssetFile();

            var command = new UploadAppImage { File = file };

            A.CallTo(() => assetThumbnailGenerator.GetImageInfoAsync(A<Stream>._))
                .Returns(Task.FromResult<ImageInfo?>(null));

            await Assert.ThrowsAsync<ValidationException>(() => HandleAsync(sut, command));
        }

        private Task<CommandContext> HandleAsync(AppUpdateCommand command, object result)
        {
            command.AppId = appId;

            var grain = A.Fake<IAppGrain>();

            A.CallTo(() => grain.ExecuteAsync(A<J<CommandRequest>>._))
                .Returns(new CommandResult(command.AggregateId, 1, 0, result));

            A.CallTo(() => grainFactory.GetGrain<IAppGrain>(command.AggregateId.ToString(), null))
                .Returns(grain);

            return HandleAsync(sut, command);
        }
    }
}
