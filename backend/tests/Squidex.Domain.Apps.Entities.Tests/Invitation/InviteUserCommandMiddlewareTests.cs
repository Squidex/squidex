// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Domain.Apps.Core.TestHelpers;
using Squidex.Domain.Apps.Entities.Apps;
using Squidex.Domain.Apps.Entities.Teams;
using Squidex.Domain.Apps.Entities.TestHelpers;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Commands;
using Squidex.Shared.Users;
using AssignAppContributor = Squidex.Domain.Apps.Entities.Apps.Commands.AssignContributor;
using AssignTeamContributor = Squidex.Domain.Apps.Entities.Teams.Commands.AssignContributor;

namespace Squidex.Domain.Apps.Entities.Invitation;

public class InviteUserCommandMiddlewareTests
{
    private readonly CancellationTokenSource cts = new CancellationTokenSource();
    private readonly CancellationToken ct;
    private readonly IUserResolver userResolver = A.Fake<IUserResolver>();
    private readonly IAppEntity app = Mocks.App(NamedId.Of(DomainId.NewGuid(), "my-app"));
    private readonly ITeamEntity team = Mocks.Team(DomainId.NewGuid());
    private readonly ICommandBus commandBus = A.Fake<ICommandBus>();
    private readonly InviteUserCommandMiddleware sut;

    public InviteUserCommandMiddlewareTests()
    {
        ct = cts.Token;

        sut = new InviteUserCommandMiddleware(userResolver);
    }

    [Fact]
    public async Task Should_invite_user_to_app_and_update_command()
    {
        var command = new AssignAppContributor { ContributorId = "me@email.com", Invite = true };

        var context =
            new CommandContext(command, commandBus)
                .Complete(app);

        var user = UserMocks.User("123", command.ContributorId);

        A.CallTo(() => userResolver.CreateUserIfNotExistsAsync(user.Email, true, ct))
            .Returns((user, true));

        await sut.HandleAsync(context, ct);

        Assert.Same(context.Result<InvitedResult<IAppEntity>>().Entity, app);
        Assert.Equal(user.Id, command.ContributorId);

        A.CallTo(() => userResolver.CreateUserIfNotExistsAsync(user.Email, true, ct))
            .MustHaveHappened();
    }

    [Fact]
    public async Task Should_invite_user_to_team_and_update_command()
    {
        var command = new AssignTeamContributor { ContributorId = "me@email.com", Invite = true };

        var context =
            new CommandContext(command, commandBus)
                .Complete(team);

        var user = UserMocks.User("123", command.ContributorId);

        A.CallTo(() => userResolver.CreateUserIfNotExistsAsync(user.Email, true, ct))
            .Returns((user, true));

        await sut.HandleAsync(context, ct);

        Assert.Same(context.Result<InvitedResult<ITeamEntity>>().Entity, team);
        Assert.Equal(user.Id, command.ContributorId);

        A.CallTo(() => userResolver.CreateUserIfNotExistsAsync(user.Email, true, ct))
            .MustHaveHappened();
    }

    [Fact]
    public async Task Should_invite_user_to_app_but_do_not_change_command_if_not_created()
    {
        var command = new AssignAppContributor { ContributorId = "me@email.com", Invite = true };

        var context =
            new CommandContext(command, commandBus)
                .Complete(app);

        var user = UserMocks.User("123", command.ContributorId);

        A.CallTo(() => userResolver.CreateUserIfNotExistsAsync(user.Email, true, ct))
            .Returns((user, false));

        await sut.HandleAsync(context, ct);

        Assert.Same(context.Result<IAppEntity>(), app);
        Assert.Equal(user.Id, command.ContributorId);

        A.CallTo(() => userResolver.CreateUserIfNotExistsAsync(user.Email, true, ct))
            .MustHaveHappened();
    }

    [Fact]
    public async Task Should_invite_user_to_team_but_do_not_change_command_if_not_created()
    {
        var command = new AssignTeamContributor { ContributorId = "me@email.com", Invite = true };

        var context =
            new CommandContext(command, commandBus)
                .Complete(team);

        var user = UserMocks.User("123", command.ContributorId);

        A.CallTo(() => userResolver.CreateUserIfNotExistsAsync(user.Email, true, ct))
            .Returns((user, false));

        await sut.HandleAsync(context, ct);

        Assert.Same(context.Result<ITeamEntity>(), team);
        Assert.Equal(user.Id, command.ContributorId);

        A.CallTo(() => userResolver.CreateUserIfNotExistsAsync(user.Email, true, ct))
            .MustHaveHappened();
    }

    [Fact]
    public async Task Should_not_call_user_resolver_if_not_email()
    {
        var command = new AssignAppContributor { ContributorId = "123", Invite = true };

        var context =
            new CommandContext(command, commandBus)
                .Complete(app);

        await sut.HandleAsync(context, ct);

        A.CallTo(() => userResolver.CreateUserIfNotExistsAsync(A<string>._, A<bool>._, A<CancellationToken>._))
            .MustNotHaveHappened();
    }

    [Fact]
    public async Task Should_not_call_user_resolver_if_not_inviting()
    {
        var command = new AssignAppContributor { ContributorId = "123", Invite = false };

        var context =
            new CommandContext(command, commandBus)
                .Complete(app);

        await sut.HandleAsync(context, ct);

        A.CallTo(() => userResolver.CreateUserIfNotExistsAsync(A<string>._, A<bool>._, A<CancellationToken>._))
            .MustNotHaveHappened();
    }
}
