// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Threading.Tasks;
using NodaTime;
using Squidex.Domain.Apps.Entities.Notifications;
using Squidex.Domain.Apps.Events.Apps;
using Squidex.Infrastructure.EventSourcing;
using Squidex.Log;
using Squidex.Shared.Users;

namespace Squidex.Domain.Apps.Entities.Apps.Invitation
{
    public sealed class InvitationEventConsumer : IEventConsumer
    {
        private static readonly Duration MaxAge = Duration.FromDays(2);
        private readonly INotificationSender emailSender;
        private readonly IUserResolver userResolver;
        private readonly ISemanticLog log;

        public string Name
        {
            get => "NotificationEmailSender";
        }

        public string EventsFilter
        {
            get { return "^app-";  }
        }

        public InvitationEventConsumer(INotificationSender emailSender, IUserResolver userResolver, ISemanticLog log)
        {
            this.emailSender = emailSender;
            this.userResolver = userResolver;

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

            if (@event.Payload is AppContributorAssigned appContributorAssigned)
            {
                if (!appContributorAssigned.Actor.IsUser || !appContributorAssigned.IsAdded)
                {
                    return;
                }

                var assignerId = appContributorAssigned.Actor.Identifier;
                var assigneeId = appContributorAssigned.ContributorId;

                var assigner = await userResolver.FindByIdAsync(assignerId);

                if (assigner == null)
                {
                    LogWarning($"Assigner {assignerId} not found");
                    return;
                }

                var assignee = await userResolver.FindByIdAsync(appContributorAssigned.ContributorId);

                if (assignee == null)
                {
                    LogWarning($"Assignee {assigneeId} not found");
                    return;
                }

                var appName = appContributorAssigned.AppId.Name;

                await emailSender.SendInviteAsync(assigner, assignee, appName);
            }
        }

        private void LogWarning(string reason)
        {
            log.LogWarning(w => w
                .WriteProperty("action", "InviteUser")
                .WriteProperty("status", "Failed")
                .WriteProperty("reason", reason));
        }
    }
}
