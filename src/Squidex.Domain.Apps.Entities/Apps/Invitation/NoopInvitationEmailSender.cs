// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Threading.Tasks;
using Squidex.Infrastructure.Tasks;
using Squidex.Shared.Users;

namespace Squidex.Domain.Apps.Entities.Apps.Invitation
{
    public sealed class NoopInvitationEmailSender : IInvitationEmailSender
    {
        public bool IsActive
        {
            get { return false; }
        }

        public Task SendExistingUserEmailAsync(IUser assigner, IUser assignee, string appName)
        {
            return TaskHelper.Done;
        }

        public Task SendNewUserEmailAsync(IUser assigner, IUser assignee, string appName)
        {
            return TaskHelper.Done;
        }
    }
}
