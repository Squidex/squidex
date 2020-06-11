// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Squidex.Shared.Users;

namespace Squidex.Domain.Users
{
    public interface IUserEventHandler
    {
        Task<IEnumerable<Claim>> OnUserRegisteringAsync(IUser user)
        {
            return Task.FromResult(Enumerable.Empty<Claim>());
        }

        void OnUserRegistered(IUser user)
        {
        }

        void OnUserUpdated(IUser user);

        void OnConsentGiven(IUser user)
        {
        }
    }
}
