// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Threading.Tasks;
using Squidex.Shared.Users;

namespace Squidex.Domain.Apps.Entities.Apps.Invitation.Notifications
{
    public sealed class NoopNotificationEmailSender : INotificationEmailSender
    {
        public bool IsActive
        {
            get { return false; }
        }

        public Task SendContributorEmailAsync(IUser assigner, IUser assignee, string appName, bool isCreated)
        {
            return Task.CompletedTask;
        }
    }
}
