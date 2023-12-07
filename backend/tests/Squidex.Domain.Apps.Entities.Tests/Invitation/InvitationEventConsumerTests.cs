// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Microsoft.Extensions.Logging;
using NodaTime;
using Squidex.Domain.Apps.Core.Apps;
using Squidex.Domain.Apps.Core.Teams;
using Squidex.Domain.Apps.Core.TestHelpers;
using Squidex.Domain.Apps.Entities.Collaboration;
using Squidex.Domain.Apps.Entities.TestHelpers;
using Squidex.Domain.Apps.Events.Apps;
using Squidex.Domain.Apps.Events.Teams;
using Squidex.Infrastructure;
using Squidex.Infrastructure.EventSourcing;
using Squidex.Shared.Users;

namespace Squidex.Domain.Apps.Entities.Invitation;

public class InvitationEventConsumerTests : GivenContext
{
    private readonly ILogger<InvitationEventConsumer> log = A.Fake<ILogger<InvitationEventConsumer>>();
    private readonly IUser assignee = UserMocks.User("2");
    private readonly IUser assigner = UserMocks.User("1");
    private readonly IUserNotifications userNotifications = A.Fake<IUserNotifications>();
    private readonly IUserResolver userResolver = A.Fake<IUserResolver>();
    private readonly string assignerId = DomainId.NewGuid().ToString();
    private readonly string assigneeId = DomainId.NewGuid().ToString();
    private readonly InvitationEventConsumer sut;

    public InvitationEventConsumerTests()
    {
        A.CallTo(() => userNotifications.IsActive)
            .Returns(true);

        A.CallTo(() => userResolver.FindByIdAsync(assignerId, default))
            .Returns(assigner);

        A.CallTo(() => userResolver.FindByIdAsync(assigneeId, default))
            .Returns(assignee);

        sut = new InvitationEventConsumer(AppProvider, userNotifications, userResolver, log);
    }

    [Fact]
    public void Should_return_app_filter_for_events_filter()
    {
        Assert.Equal(StreamFilter.Prefix("app-"), sut.EventsFilter);
    }

    [Fact]
    public async Task Should_do_nothing_on_clear()
    {
        await ((IEventConsumer)sut).ClearAsync();
    }

    [Fact]
    public void Should_return_custom_name()
    {
        Assert.Equal("NotificationEmailSender", sut.Name);
    }

    [Fact]
    public async Task Should_not_send_app_email_if_contributors_assigned_by_clients()
    {
        var @event = CreateAppEvent(RefTokenType.Client, true);

        await sut.On(@event);

        MustNotResolveUser();
        MustNotSendEmail();
    }

    [Fact]
    public async Task Should_not_send_app_email_for_initial_owner()
    {
        var @event = CreateAppEvent(RefTokenType.Subject, false, streamNumber: 1);

        await sut.On(@event);

        MustNotSendEmail();
    }

    [Fact]
    public async Task Should_not_send_team_email_for_initial_owner()
    {
        var @event = CreateTeamEvent(false, streamNumber: 1);

        await sut.On(@event);

        MustNotSendEmail();
    }

    [Fact]
    public async Task Should_not_send_app_email_for_old_events()
    {
        var created = SystemClock.Instance.GetCurrentInstant().Minus(Duration.FromHours(50));

        var @event = CreateAppEvent(RefTokenType.Subject, true, instant: created);

        await sut.On(@event);

        MustNotResolveUser();
        MustNotSendEmail();
    }

    [Fact]
    public async Task Should_not_send_team_email_for_old_events()
    {
        var created = SystemClock.Instance.GetCurrentInstant().Minus(Duration.FromHours(50));

        var @event = CreateTeamEvent(true, instant: created);

        await sut.On(@event);

        MustNotResolveUser();
        MustNotSendEmail();
    }

    [Fact]
    public async Task Should_not_send_app_email_for_old_contributor()
    {
        var @event = CreateAppEvent(RefTokenType.Subject, true, isNewContributor: false);

        await sut.On(@event);

        MustNotResolveUser();
        MustNotSendEmail();
    }

    [Fact]
    public async Task Should_not_send_team_email_for_old_contributor()
    {
        var @event = CreateTeamEvent(true, isNewContributor: false);

        await sut.On(@event);

        MustNotResolveUser();
        MustNotSendEmail();
    }

    [Fact]
    public async Task Should_not_send_app_email_if_sender_not_active()
    {
        var @event = CreateAppEvent(RefTokenType.Subject, true);

        A.CallTo(() => userNotifications.IsActive)
            .Returns(false);

        await sut.On(@event);

        MustNotResolveUser();
        MustNotSendEmail();
    }

    [Fact]
    public async Task Should_not_send_team_email_if_sender_not_active()
    {
        var @event = CreateTeamEvent(true);

        A.CallTo(() => userNotifications.IsActive)
            .Returns(false);

        await sut.On(@event);

        MustNotResolveUser();
        MustNotSendEmail();
    }

    [Fact]
    public async Task Should_not_send_app_email_if_assigner_not_found()
    {
        var @event = CreateAppEvent(RefTokenType.Subject, true);

        A.CallTo(() => userResolver.FindByIdAsync(assignerId, default))
            .Returns(Task.FromResult<IUser?>(null));

        await sut.On(@event);

        MustNotSendEmail();
        MustLogWarning();
    }

    [Fact]
    public async Task Should_not_send_team_email_if_assigner_not_found()
    {
        var @event = CreateTeamEvent(true);

        A.CallTo(() => userResolver.FindByIdAsync(assignerId, default))
            .Returns(Task.FromResult<IUser?>(null));

        await sut.On(@event);

        MustNotSendEmail();
        MustLogWarning();
    }

    [Fact]
    public async Task Should_not_send_app_email_if_assignee_not_found()
    {
        var @event = CreateAppEvent(RefTokenType.Subject, true);

        A.CallTo(() => userResolver.FindByIdAsync(assigneeId, default))
            .Returns(Task.FromResult<IUser?>(null));

        await sut.On(@event);

        MustNotSendEmail();
        MustLogWarning();
    }

    [Fact]
    public async Task Should_not_send_team_email_if_assignee_not_found()
    {
        var @event = CreateTeamEvent(true);

        A.CallTo(() => userResolver.FindByIdAsync(assigneeId, default))
            .Returns(Task.FromResult<IUser?>(null));

        await sut.On(@event);

        MustNotSendEmail();
        MustLogWarning();
    }

    [Fact]
    public async Task Should_send_app_email_for_new_user()
    {
        var @event = CreateAppEvent(RefTokenType.Subject, true);

        await sut.On(@event);

        A.CallTo(() => userNotifications.SendInviteAsync(assigner, assignee, App, default))
            .MustHaveHappened();
    }

    [Fact]
    public async Task Should_send_team_email_for_new_user()
    {
        var @event = CreateTeamEvent(true);

        await sut.On(@event);

        A.CallTo(() => userNotifications.SendInviteAsync(assigner, assignee, Team, default))
            .MustHaveHappened();
    }

    [Fact]
    public async Task Should_send_app_email_for_existing_user()
    {
        var @event = CreateAppEvent(RefTokenType.Subject, false);

        await sut.On(@event);

        A.CallTo(() => userNotifications.SendInviteAsync(assigner, assignee, App, default))
            .MustHaveHappened();
    }

    [Fact]
    public async Task Should_send_team_email_for_existing_user()
    {
        var @event = CreateTeamEvent(false);

        await sut.On(@event);

        A.CallTo(() => userNotifications.SendInviteAsync(assigner, assignee, Team, default))
            .MustHaveHappened();
    }

    [Fact]
    public async Task Should_not_send_team_email_if_team_not_found()
    {
        var @event = CreateTeamEvent(true);

        A.CallTo(() => AppProvider.GetTeamAsync(A<DomainId>._, default))
            .Returns(Task.FromResult<Team?>(null));

        await sut.On(@event);

        MustNotSendEmail();
    }

    private void MustLogWarning()
    {
        A.CallTo(log).Where(x => x.Method.Name == "Log" && x.GetArgument<LogLevel>(0) == LogLevel.Warning)
            .MustHaveHappened();
    }

    private void MustNotResolveUser()
    {
        A.CallTo(() => userResolver.FindByIdAsync(A<string>._, A<CancellationToken>._))
            .MustNotHaveHappened();
    }

    private void MustNotSendEmail()
    {
        A.CallTo(() => userNotifications.SendInviteAsync(A<IUser>._, A<IUser>._, A<App>._, default))
            .MustNotHaveHappened();

        A.CallTo(() => userNotifications.SendInviteAsync(A<IUser>._, A<IUser>._, A<App>._, default))
            .MustNotHaveHappened();
    }

    private Envelope<IEvent> CreateAppEvent(RefTokenType assignerType, bool isNewUser, bool isNewContributor = true, Instant? instant = null, int streamNumber = 2)
    {
        var @event = new AppContributorAssigned
        {
            Actor = new RefToken(assignerType, assignerId),
            AppId = AppId,
            ContributorId = assigneeId,
            IsCreated = isNewUser,
            IsAdded = isNewContributor
        };

        var envelope = Envelope.Create(@event);

        envelope.SetTimestamp(instant ?? SystemClock.Instance.GetCurrentInstant());
        envelope.SetEventStreamNumber(streamNumber);

        return envelope;
    }

    private Envelope<IEvent> CreateTeamEvent(bool isNewUser, bool isNewContributor = true, Instant? instant = null, int streamNumber = 2)
    {
        var @event = new TeamContributorAssigned
        {
            Actor = new RefToken(RefTokenType.Subject, assignerId),
            ContributorId = assigneeId,
            IsCreated = isNewUser,
            IsAdded = isNewContributor,
            TeamId = TeamId
        };

        var envelope = Envelope.Create(@event);

        envelope.SetTimestamp(instant ?? SystemClock.Instance.GetCurrentInstant());
        envelope.SetEventStreamNumber(streamNumber);

        return envelope;
    }
}
