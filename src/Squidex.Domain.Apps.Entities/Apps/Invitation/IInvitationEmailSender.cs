// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Threading.Tasks;
using Squidex.Shared.Users;

namespace Squidex.Domain.Apps.Entities.Apps.Invitation
{
    public interface IInvitationEmailSender
    {
        Task SendNewUserEmailAsync(IUser assigner, IUser assignee, string appName);

        Task SendExistingUserEmailAsync(IUser assigner, IUser assignee, string appName);
    }
}
