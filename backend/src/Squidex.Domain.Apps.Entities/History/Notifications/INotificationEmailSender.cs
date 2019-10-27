// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Threading.Tasks;
using Squidex.Shared.Users;

namespace Squidex.Domain.Apps.Entities.History.Notifications
{
    public interface INotificationEmailSender
    {
        bool IsActive { get; }

        Task SendContributorEmailAsync(IUser assigner, IUser assignee, string appName, bool isCreated);
    }
}
