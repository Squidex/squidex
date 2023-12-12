// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Microsoft.AspNetCore.Http;
using Squidex.Domain.Apps.Core.Schemas;
using Squidex.Domain.Apps.Entities;
using Squidex.Domain.Apps.Entities.Contents.Commands;
using Squidex.Domain.Apps.Entities.TestHelpers;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Commands;

namespace Squidex.Web.CommandMiddlewares;

public class EnrichWithSchemaIdCommandMiddlewareTests : GivenContext
{
    private readonly IHttpContextAccessor httpContextAccessor = A.Fake<IHttpContextAccessor>();
    private readonly HttpContext httpContext = new DefaultHttpContext();
    private readonly EnrichWithSchemaIdCommandMiddleware sut;

    public EnrichWithSchemaIdCommandMiddlewareTests()
    {
        httpContext.Features.Set(App);
        httpContext.Features.Set(Schema);

        A.CallTo(() => httpContextAccessor.HttpContext)
            .Returns(httpContext);

        sut = new EnrichWithSchemaIdCommandMiddleware(httpContextAccessor);
    }

    [Fact]
    public async Task Should_throw_exception_if_schema_not_found()
    {
        httpContext.Features.Set<Schema>(null);

        await Assert.ThrowsAsync<InvalidOperationException>(() => HandleAsync(new CreateContent()));
    }

    [Fact]
    public async Task Should_assign_named_id_to_command()
    {
        var context = await HandleAsync(new CreateContent());

        Assert.Equal(SchemaId, ((ISchemaCommand)context.Command).SchemaId);
    }

    [Fact]
    public async Task Should_not_override_existing_named_id()
    {
        var customId = NamedId.Of(DomainId.NewGuid(), "other-app");

        var context = await HandleAsync(new CreateContent { SchemaId = customId });

        Assert.Equal(customId, ((ISchemaCommand)context.Command).SchemaId);
    }

    private async Task<CommandContext> HandleAsync(IAppCommand command)
    {
        command.AppId = AppId;

        var commandContext = new CommandContext(command, A.Fake<ICommandBus>());

        await sut.HandleAsync(commandContext, default);

        return commandContext;
    }
}
