// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Domain.Apps.Core.Apps;
using Squidex.Domain.Apps.Core.Teams;
using Squidex.Domain.Apps.Core.TestHelpers;
using Squidex.Domain.Apps.Entities.TestHelpers;
using Squidex.Infrastructure.Commands;
using Squidex.Shared.Users;
using AssignAppContributor = Squidex.Domain.Apps.Entities.Apps.Commands.AssignContributor;
using AssignTeamContributor = Squidex.Domain.Apps.Entities.Teams.Commands.AssignContributor;

namespace Squidex.Domain.Apps.Entities.Invitation;

public class InviteUserCommandMiddlewareTests : GivenContext
{
    private readonly IUserResolver userResolver = A.Fake<IUserResolver>();
    private readonly ICommandBus commandBus = A.Fake<ICommandBus>();
    private readonly InviteUserCommandMiddleware sut;

    public InviteUserCommandMiddlewareTests()
    {
        sut = new InviteUserCommandMiddleware(userResolver);
    }

    [Fact]
    public async Task Should_invite_user_to_app_and_update_command()
    {
        var command = new AssignAppContributor { ContributorId = "me@email.com", Invite = true };

        var context =
            new CommandContext(command, commandBus)
                .Complete(App);

        var user = UserMocks.User("123", command.ContributorId);

        A.CallTo(() => userResolver.CreateUserIfNotExistsAsync(user.Email, true, CancellationToken))
            .Returns((user, true));

        await sut.HandleAsync(context, CancellationToken);

        Assert.Same(context.Result<InvitedResult<App>>().Entity, App);
        Assert.Equal(user.Id, command.ContributorId);

        A.CallTo(() => userResolver.CreateUserIfNotExistsAsync(user.Email, true, CancellationToken))
            .MustHaveHappened();
    }

    [Fact]
    public async Task Should_invite_user_to_team_and_update_command()
    {
        var command = new AssignTeamContributor { ContributorId = "me@email.com", Invite = true };

        var context =
            new CommandContext(command, commandBus)
                .Complete(Team);

        var user = UserMocks.User("123", command.ContributorId);

        A.CallTo(() => userResolver.CreateUserIfNotExistsAsync(user.Email, true, CancellationToken))
            .Returns((user, true));

        await sut.HandleAsync(context, CancellationToken);

        Assert.Same(context.Result<InvitedResult<Team>>().Entity, Team);
        Assert.Equal(user.Id, command.ContributorId);

        A.CallTo(() => userResolver.CreateUserIfNotExistsAsync(user.Email, true, CancellationToken))
            .MustHaveHappened();
    }

    [Fact]
    public async Task Should_invite_user_to_app_but_do_not_change_command_if_not_created()
    {
        var command = new AssignAppContributor { ContributorId = "me@email.com", Invite = true };

        var context =
            new CommandContext(command, commandBus)
                .Complete(App);

        var user = UserMocks.User("123", command.ContributorId);

        A.CallTo(() => userResolver.CreateUserIfNotExistsAsync(user.Email, true, CancellationToken))
            .Returns((user, false));

        await sut.HandleAsync(context, CancellationToken);

        Assert.Same(context.Result<App>(), App);
        Assert.Equal(user.Id, command.ContributorId);

        A.CallTo(() => userResolver.CreateUserIfNotExistsAsync(user.Email, true, CancellationToken))
            .MustHaveHappened();
    }

    [Fact]
    public async Task Should_invite_user_to_team_but_do_not_change_command_if_not_created()
    {
        var command = new AssignTeamContributor { ContributorId = "me@email.com", Invite = true };

        var context =
            new CommandContext(command, commandBus)
                .Complete(Team);

        var user = UserMocks.User("123", command.ContributorId);

        A.CallTo(() => userResolver.CreateUserIfNotExistsAsync(user.Email, true, CancellationToken))
            .Returns((user, false));

        await sut.HandleAsync(context, CancellationToken);

        Assert.Same(context.Result<Team>(), Team);
        Assert.Equal(user.Id, command.ContributorId);

        A.CallTo(() => userResolver.CreateUserIfNotExistsAsync(user.Email, true, CancellationToken))
            .MustHaveHappened();
    }

    [Fact]
    public async Task Should_not_call_user_resolver_if_not_email()
    {
        var command = new AssignAppContributor { ContributorId = "123", Invite = true };

        var context =
            new CommandContext(command, commandBus)
                .Complete(App);

        await sut.HandleAsync(context, CancellationToken);

        A.CallTo(() => userResolver.CreateUserIfNotExistsAsync(A<string>._, A<bool>._, A<CancellationToken>._))
            .MustNotHaveHappened();
    }

    [Fact]
    public async Task Should_not_call_user_resolver_if_not_inviting()
    {
        var command = new AssignAppContributor { ContributorId = "123", Invite = false };

        var context =
            new CommandContext(command, commandBus)
                .Complete(App);

        await sut.HandleAsync(context, CancellationToken);

        A.CallTo(() => userResolver.CreateUserIfNotExistsAsync(A<string>._, A<bool>._, A<CancellationToken>._))
            .MustNotHaveHappened();
    }
}
