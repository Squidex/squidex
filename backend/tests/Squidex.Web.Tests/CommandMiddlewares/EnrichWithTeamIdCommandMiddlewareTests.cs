﻿// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Microsoft.AspNetCore.Http;
using Squidex.Domain.Apps.Core.Teams;
using Squidex.Domain.Apps.Entities;
using Squidex.Domain.Apps.Entities.Teams.Commands;
using Squidex.Domain.Apps.Entities.TestHelpers;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Commands;

namespace Squidex.Web.CommandMiddlewares;

public class EnrichWithTeamIdCommandMiddlewareTests : GivenContext
{
    private readonly IHttpContextAccessor httpContextAccessor = A.Fake<IHttpContextAccessor>();
    private readonly HttpContext httpContext = new DefaultHttpContext();
    private readonly EnrichWithTeamIdCommandMiddleware sut;

    public EnrichWithTeamIdCommandMiddlewareTests()
    {
        httpContext.Features.Set(Team);

        A.CallTo(() => httpContextAccessor.HttpContext)
            .Returns(httpContext);

        sut = new EnrichWithTeamIdCommandMiddleware(httpContextAccessor);
    }

    [Fact]
    public async Task Should_throw_exception_if_team_not_found()
    {
        httpContext.Features.Set<Team>(null);

        await Assert.ThrowsAsync<InvalidOperationException>(() => HandleAsync(new UpdateTeam()));
    }

    [Fact]
    public async Task Should_assign_id_to_command()
    {
        var context = await HandleAsync(new UpdateTeam());

        Assert.Equal(TeamId, ((ITeamCommand)context.Command).TeamId);
    }

    [Fact]
    public async Task Should_not_override_existing_id()
    {
        var customId = DomainId.NewGuid();

        var context = await HandleAsync(new UpdateTeam { TeamId = customId });

        Assert.Equal(customId, ((ITeamCommand)context.Command).TeamId);
    }

    private async Task<CommandContext> HandleAsync(ITeamCommand command)
    {
        var commandContext = new CommandContext(command, A.Fake<ICommandBus>());

        await sut.HandleAsync(commandContext, default);

        return commandContext;
    }
}
