// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;
using Microsoft.Net.Http.Headers;
using Squidex.Domain.Apps.Entities.Contents;
using Squidex.Domain.Apps.Entities.Contents.Commands;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Commands;

namespace Squidex.Web.CommandMiddlewares;

public class ETagCommandMiddlewareTests
{
    private readonly IHttpContextAccessor httpContextAccessor = A.Fake<IHttpContextAccessor>();
    private readonly HttpContext httpContext = new DefaultHttpContext();
    private readonly ETagCommandMiddleware sut;

    public ETagCommandMiddlewareTests()
    {
        A.CallTo(() => httpContextAccessor.HttpContext)
            .Returns(httpContext);

        sut = new ETagCommandMiddleware(httpContextAccessor);
    }

    [Fact]
    public async Task Should_do_nothing_if_context_is_null()
    {
        A.CallTo(() => httpContextAccessor.HttpContext)
            .Returns(null!);

        await HandleAsync(new CreateContent(), true);
    }

    [Fact]
    public async Task Should_do_nothing_if_command_has_etag_defined()
    {
        httpContext.Request.Headers[HeaderNames.IfMatch] = "13";

        var context = await HandleAsync(new CreateContent { ExpectedVersion = 1 }, true);

        Assert.Equal(1, ((IAggregateCommand)context.Command).ExpectedVersion);
    }

    [Fact]
    public async Task Should_add_expected_version_to_command()
    {
        httpContext.Request.Headers[HeaderNames.IfMatch] = "13";

        var context = await HandleAsync(new CreateContent(), true);

        Assert.Equal(13, ((IAggregateCommand)context.Command).ExpectedVersion);
    }

    [Fact]
    public async Task Should_add_weak_etag_as_expected_version_to_command()
    {
        httpContext.Request.Headers[HeaderNames.IfMatch] = "W/13";

        var context = await HandleAsync(new CreateContent(), true);

        Assert.Equal(13, ((IAggregateCommand)context.Command).ExpectedVersion);
    }

    [Fact]
    public async Task Should_add_version_from_actual_as_etag_to_response()
    {
        var actual = CommandResult.Empty(DomainId.Empty, 17, 16);

        await HandleAsync(new CreateContent(), actual);

        Assert.Equal(new StringValues("17"), httpContextAccessor.HttpContext!.Response.Headers[HeaderNames.ETag]);
    }

    [Fact]
    public async Task Should_add_version_from_entity_as_etag_to_response()
    {
        var actual = new ContentEntity { Version = 17 };

        await HandleAsync(new CreateContent(), actual);

        Assert.Equal(new StringValues("17"), httpContextAccessor.HttpContext!.Response.Headers[HeaderNames.ETag]);
    }

    private async Task<CommandContext> HandleAsync(ICommand command, object actual)
    {
        var commandContext = new CommandContext(command, A.Fake<ICommandBus>()).Complete(actual);

        await sut.HandleAsync(commandContext, default);

        return commandContext;
    }
}
