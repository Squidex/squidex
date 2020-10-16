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
    public interface INotificationSender
    {
        bool IsActive { get; }

        Task SendUsageAsync(IUser user, string appName, long usage, long usageLimit);

        Task SendInviteAsync(IUser assigner, IUser user, string appName);
    }
}
