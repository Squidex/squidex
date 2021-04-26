// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Threading.Tasks;
using Squidex.Shared.Users;

namespace Squidex.Domain.Users
{
    public interface IUserEvents
    {
        Task OnUserRegisteredAsync(IUser user)
        {
            return Task.CompletedTask;
        }

        Task OnUserUpdatedAsync(IUser user, IUser previous)
        {
            return Task.CompletedTask;
        }

        Task OnUserDeletedAsync(IUser user)
        {
            return Task.CompletedTask;
        }

        Task OnConsentGivenAsync(IUser user)
        {
            return Task.CompletedTask;
        }
    }
}
