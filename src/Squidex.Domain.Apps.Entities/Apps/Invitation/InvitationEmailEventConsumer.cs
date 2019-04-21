// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Threading.Tasks;
using Squidex.Domain.Apps.Events.Apps;
using Squidex.Infrastructure;
using Squidex.Infrastructure.EventSourcing;
using Squidex.Infrastructure.Log;
using Squidex.Infrastructure.Tasks;
using Squidex.Shared.Users;

namespace Squidex.Domain.Apps.Entities.Apps.Invitation
{
    public sealed class InvitationEmailEventConsumer : IEventConsumer
    {
        private readonly IInvitationEmailSender emailSender;
        private readonly IUserResolver userResolver;
        private readonly ISemanticLog log;

        public string Name
        {
            get { return "InvitationEmailSender"; }
        }

        public string EventsFilter
        {
            get { return "^app-";  }
        }

        public InvitationEmailEventConsumer(IInvitationEmailSender emailSender, IUserResolver userResolver, ISemanticLog log)
        {
            Guard.NotNull(emailSender, nameof(emailSender));
            Guard.NotNull(userResolver, nameof(userResolver));
            Guard.NotNull(log, nameof(log));

            this.emailSender = emailSender;
            this.userResolver = userResolver;

            this.log = log;
        }

        public bool Handles(StoredEvent @event)
        {
            return true;
        }

        public Task ClearAsync()
        {
            return TaskHelper.Done;
        }

        public async Task On(Envelope<IEvent> @event)
        {
            if (@event.Payload is AppContributorAssigned appContributorAssigned && appContributorAssigned.Actor.IsClient)
            {
                var assigner = await userResolver.FindByIdOrEmailAsync(appContributorAssigned.Actor.Identifier);

                if (assigner == null)
                {
                    LogWarning($"Assigner {appContributorAssigned.Actor.Identifier} not found");
                    return;
                }

                var assignee = await userResolver.FindByIdOrEmailAsync(appContributorAssigned.ContributorId);

                if (assignee == null)
                {
                    LogWarning($"Assignee {appContributorAssigned.ContributorId} not found");
                    return;
                }

                var appName = appContributorAssigned.AppId.Name;

                if (appContributorAssigned.IsCreated)
                {
                    await emailSender.SendNewUserEmailAsync(assigner, assignee, appName);
                }
                else
                {
                    await emailSender.SendExistingUserEmailAsync(assigner, assignee, appName);
                }
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
