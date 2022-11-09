// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Microsoft.Extensions.Logging;
using NodaTime;
using Squidex.Domain.Apps.Entities.Notifications;
using Squidex.Domain.Apps.Events.Apps;
using Squidex.Domain.Apps.Events.Teams;
using Squidex.Infrastructure;
using Squidex.Infrastructure.EventSourcing;
using Squidex.Shared.Users;

namespace Squidex.Domain.Apps.Entities.Invitation;

public sealed class InvitationEventConsumer : IEventConsumer
{
    private static readonly Duration MaxAge = Duration.FromDays(2);
    private readonly IUserNotifications userNotifications;
    private readonly IUserResolver userResolver;
    private readonly IAppProvider appProvider;
    private readonly ILogger<InvitationEventConsumer> log;

    public string Name
    {
        get => "NotificationEmailSender";
    }

    public string EventsFilter
    {
        get { return "^app-|^app-"; }
    }

    public InvitationEventConsumer(
        IAppProvider appProvider,
        IUserNotifications userNotifications,
        IUserResolver userResolver,
        ILogger<InvitationEventConsumer> log)
    {
        this.appProvider = appProvider;
        this.userNotifications = userNotifications;
        this.userResolver = userResolver;
        this.log = log;
    }

    public async Task On(Envelope<IEvent> @event)
    {
        if (!userNotifications.IsActive)
        {
            return;
        }

        if (@event.Headers.EventStreamNumber() <= 1)
        {
            return;
        }

        var now = SystemClock.Instance.GetCurrentInstant();

        var timestamp = @event.Headers.Timestamp();

        if (now - timestamp > MaxAge)
        {
            return;
        }

        switch (@event.Payload)
        {
            case AppContributorAssigned assigned when assigned.IsAdded:
                {
                    var (assigner, assignee) = await ResolveUsersAsync(assigned.Actor, assigned.ContributorId, default);

                    if (assigner == null || assignee == null)
                    {
                        return;
                    }

                    var app = await appProvider.GetAppAsync(assigned.AppId.Id, true);

                    if (app == null)
                    {
                        return;
                    }

                    await userNotifications.SendInviteAsync(assigner, assignee, app);
                    return;
                }

            case TeamContributorAssigned assigned when assigned.IsAdded:
                {
                    var (assigner, assignee) = await ResolveUsersAsync(assigned.Actor, assigned.ContributorId, default);

                    if (assigner == null || assignee == null)
                    {
                        return;
                    }

                    var team = await appProvider.GetTeamAsync(assigned.TeamId);

                    if (team == null)
                    {
                        return;
                    }

                    await userNotifications.SendInviteAsync(assigner, assignee, team);
                    break;
                }
        }
    }

    private async Task<(IUser? Assignee, IUser? Assigner)> ResolveUsersAsync(RefToken assignerId, string assigneeId,
        CancellationToken ct)
    {
        if (!assignerId.IsUser)
        {
            return default;
        }

        var assigner = await userResolver.FindByIdAsync(assignerId.Identifier, ct);

        if (assigner == null)
        {
            log.LogWarning("Failed to invite user: Assigner {assignerId} not found.", assignerId);
            return default;
        }

        var assignee = await userResolver.FindByIdAsync(assigneeId, ct);

        if (assignee == null)
        {
            log.LogWarning("Failed to invite user: Assignee {assigneeId} not found.", assigneeId);
            return default;
        }

        return (assigner, assignee);
    }
}
