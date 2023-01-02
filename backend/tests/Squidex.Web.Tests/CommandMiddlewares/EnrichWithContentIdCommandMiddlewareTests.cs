// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Domain.Apps.Entities.Contents.Commands;
using Squidex.Domain.Apps.Entities.TestHelpers;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Commands;

namespace Squidex.Web.CommandMiddlewares;

public class EnrichWithContentIdCommandMiddlewareTests : GivenContext
{
    private readonly EnrichWithContentIdCommandMiddleware sut;

    public EnrichWithContentIdCommandMiddlewareTests()
    {
        sut = new EnrichWithContentIdCommandMiddleware();
    }

    [Fact]
    public async Task Should_replace_content_id_with_schema_id_if_placeholder_used()
    {
        var command = new UpdateContent
        {
            ContentId = DomainId.Create("_schemaId_")
        };

        await HandleAsync(command);

        Assert.Equal(SchemaId.Id, command.ContentId);
    }

    [Fact]
    public async Task Should_not_replace_content_id_with_schema_for_create_command()
    {
        var command = new CreateContent
        {
            ContentId = DomainId.Create("_SchemaId_")
        };

        await HandleAsync(command);

        Assert.NotEqual(SchemaId.Id, command.ContentId);
    }

    [Fact]
    public async Task Should_not_replace_content_id_with_schema_id_if_placeholder_not_used()
    {
        var command = new UpdateContent
        {
            ContentId = DomainId.Create("{custom}")
        };

        await HandleAsync(command);

        Assert.NotEqual(SchemaId.Id, command.ContentId);
    }

    private async Task<CommandContext> HandleAsync(ContentCommand command)
    {
        command.AppId = AppId;
        command.SchemaId = SchemaId;

        var commandContext = new CommandContext(command, A.Fake<ICommandBus>());

        await sut.HandleAsync(commandContext, default);

        return commandContext;
    }
}
