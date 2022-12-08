// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Assets;
using Squidex.Domain.Apps.Entities.Apps.Commands;
using Squidex.Domain.Apps.Entities.TestHelpers;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Commands;
using Squidex.Infrastructure.Validation;

namespace Squidex.Domain.Apps.Entities.Apps.DomainObject;

public class AppCommandMiddlewareTests : HandlerTestBase<AppDomainObject.State>
{
    private readonly IDomainObjectFactory domainObjectFactory = A.Fake<IDomainObjectFactory>();
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

        sut = new AppCommandMiddleware(domainObjectFactory, appImageStore, assetThumbnailGenerator, contextProvider);
    }

    [Fact]
    public async Task Should_replace_context_app_with_domain_object_actual()
    {
        var actual = A.Fake<IAppEntity>();

        await HandleAsync(new UpdateApp(), actual);

        Assert.Same(actual, requestContext.App);
    }

    [Fact]
    public async Task Should_upload_image_to_store()
    {
        var file = new NoopAssetFile();

        A.CallTo(() => assetThumbnailGenerator.GetImageInfoAsync(A<Stream>._, file.MimeType, default))
            .Returns(new ImageInfo(100, 100, ImageOrientation.None, ImageFormat.PNG));

        await HandleAsync(new UploadAppImage { File = file }, None.Value);

        A.CallTo(() => appImageStore.UploadAsync(appId.Id, A<Stream>._, A<CancellationToken>._))
            .MustHaveHappened();
    }

    [Fact]
    public async Task Should_throw_exception_if_file_to_upload_is_not_an_image()
    {
        var file = new NoopAssetFile();

        var command = new UploadAppImage { File = file };

        A.CallTo(() => assetThumbnailGenerator.GetImageInfoAsync(A<Stream>._, file.MimeType, default))
            .Returns(Task.FromResult<ImageInfo?>(null));

        await Assert.ThrowsAsync<ValidationException>(() => HandleAsync(sut, command));
    }

    private Task<CommandContext> HandleAsync(AppCommand command, object actual)
    {
        command.AppId = appId;

        var domainObject = A.Fake<AppDomainObject>();

        A.CallTo(() => domainObject.ExecuteAsync(A<IAggregateCommand>._, A<CancellationToken>._))
            .Returns(new CommandResult(command.AggregateId, 1, 0, actual));

        A.CallTo(() => domainObjectFactory.Create<AppDomainObject>(command.AggregateId))
            .Returns(domainObject);

        return HandleAsync(sut, command);
    }
}
