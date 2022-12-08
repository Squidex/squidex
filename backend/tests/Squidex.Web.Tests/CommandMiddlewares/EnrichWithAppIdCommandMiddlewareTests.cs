// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Domain.Apps.Entities;
using Squidex.Domain.Apps.Entities.Contents.Commands;
using Squidex.Domain.Apps.Entities.TestHelpers;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Commands;

namespace Squidex.Web.CommandMiddlewares;

public class EnrichWithAppIdCommandMiddlewareTests
{
    private readonly IContextProvider contextProvider = A.Fake<IContextProvider>();
    private readonly NamedId<DomainId> appId = NamedId.Of(DomainId.NewGuid(), "my-app");
    private readonly Context requestContext;
    private readonly EnrichWithAppIdCommandMiddleware sut;

    public EnrichWithAppIdCommandMiddlewareTests()
    {
        requestContext = Context.Anonymous(Mocks.App(appId));

        A.CallTo(() => contextProvider.Context)
            .Returns(requestContext);

        sut = new EnrichWithAppIdCommandMiddleware(contextProvider);
    }

    [Fact]
    public async Task Should_throw_exception_if_app_not_found()
    {
        A.CallTo(() => contextProvider.Context)
            .Returns(Context.Anonymous(null!));

        await Assert.ThrowsAsync<InvalidOperationException>(() => HandleAsync(new CreateContent()));
    }

    [Fact]
    public async Task Should_assign_named_id_to_command()
    {
        var context = await HandleAsync(new CreateContent());

        Assert.Equal(appId, ((IAppCommand)context.Command).AppId);
    }

    [Fact]
    public async Task Should_not_override_existing_named_id()
    {
        var customId = NamedId.Of(DomainId.NewGuid(), "other-app");

        var context = await HandleAsync(new CreateContent { AppId = customId });

        Assert.Equal(customId, ((IAppCommand)context.Command).AppId);
    }

    private async Task<CommandContext> HandleAsync(ICommand command)
    {
        var commandContext = new CommandContext(command, A.Fake<ICommandBus>());

        await sut.HandleAsync(commandContext, default);

        return commandContext;
    }
}
