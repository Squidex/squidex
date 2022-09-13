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

namespace Squidex.Domain.Apps.Entities.Invitation
{
    public sealed class InvitationEventConsumer : IEventConsumer
    {
        private static readonly Duration MaxAge = Duration.FromDays(2);
        private readonly INotificationSender emailSender;
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

        public InvitationEventConsumer(INotificationSender emailSender, IUserResolver userResolver, IAppProvider appProvider,
            ILogger<InvitationEventConsumer> log)
        {
            this.emailSender = emailSender;
            this.userResolver = userResolver;
            this.appProvider = appProvider;
            this.log = log;
        }

        public async Task On(Envelope<IEvent> @event)
        {
            if (!emailSender.IsActive)
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

                        await emailSender.SendInviteAsync(assigner, assignee, assigned.AppId.Name);
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

                        await emailSender.SendTeamInviteAsync(assigner, assignee, team.Name);
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
}
