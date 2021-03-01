// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Threading.Tasks;
using Squidex.Shared.Users;

namespace Squidex.Domain.Apps.Entities.Notifications
{
    public sealed class NoopNotificationSender : INotificationSender
    {
        public bool IsActive
        {
            get => false;
        }

        public Task SendInviteAsync(IUser assigner, IUser user, string appName)
        {
            return Task.CompletedTask;
        }

        public Task SendUsageAsync(IUser user, string appName, long usage, long limit)
        {
            return Task.CompletedTask;
        }
    }
}
