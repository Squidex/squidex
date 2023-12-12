// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Assets;
using Squidex.Domain.Apps.Core.Apps;
using Squidex.Domain.Apps.Entities.Apps.Commands;
using Squidex.Domain.Apps.Entities.TestHelpers;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Commands;
using Squidex.Infrastructure.Validation;

namespace Squidex.Domain.Apps.Entities.Apps.DomainObject;

public class AppCommandMiddlewareTests : HandlerTestBase<App>
{
    private readonly IDomainObjectFactory domainObjectFactory = A.Fake<IDomainObjectFactory>();
    private readonly IAppImageStore appImageStore = A.Fake<IAppImageStore>();
    private readonly IAssetThumbnailGenerator assetGenerator = A.Fake<IAssetThumbnailGenerator>();
    private readonly AppCommandMiddleware sut;

    public sealed class MyCommand : SquidexCommand
    {
    }

    protected override DomainId Id
    {
        get => AppId.Id;
    }

    public AppCommandMiddlewareTests()
    {
        sut = new AppCommandMiddleware(domainObjectFactory, appImageStore, assetGenerator, ApiContextProvider);
    }

    [Fact]
    public async Task Should_replace_context_app_with_domain_object_result()
    {
        var replaced = new App();

        await HandleAsync(new UpdateApp(), replaced);

        Assert.Same(replaced, ApiContext.App);
    }

    [Fact]
    public async Task Should_upload_image_to_store()
    {
        var file = new NoopAssetFile();

        A.CallTo(() => assetGenerator.GetImageInfoAsync(A<Stream>._, file.MimeType, CancellationToken))
            .Returns(new ImageInfo(ImageFormat.PNG, 100, 100, ImageOrientation.None, false));

        await HandleAsync(new UploadAppImage { File = file }, None.Value);

        A.CallTo(() => appImageStore.UploadAsync(AppId.Id, A<Stream>._, CancellationToken))
            .MustHaveHappened();
    }

    [Fact]
    public async Task Should_throw_exception_if_file_to_upload_is_not_an_image()
    {
        var file = new NoopAssetFile();

        var command = new UploadAppImage { File = file };

        A.CallTo(() => assetGenerator.GetImageInfoAsync(A<Stream>._, file.MimeType, CancellationToken))
            .Returns(Task.FromResult<ImageInfo?>(null));

        await Assert.ThrowsAsync<ValidationException>(() => HandleAsync(sut, command, CancellationToken));
    }

    private Task<CommandContext> HandleAsync(AppCommand command, object actual)
    {
        command.AppId = AppId;

        var domainObject = A.Fake<AppDomainObject>();

        A.CallTo(() => domainObject.ExecuteAsync(A<IAggregateCommand>._, A<CancellationToken>._))
            .Returns(new CommandResult(command.AggregateId, 1, 0, actual));

        A.CallTo(() => domainObjectFactory.Create<AppDomainObject>(command.AggregateId))
            .Returns(domainObject);

        return HandleAsync(sut, command, CancellationToken);
    }
}
