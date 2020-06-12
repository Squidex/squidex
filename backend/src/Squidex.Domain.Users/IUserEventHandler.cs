// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Shared.Users;

namespace Squidex.Domain.Users
{
    public interface IUserEventHandler
    {
        void OnUserRegistered(IUser user)
        {
        }

        void OnUserUpdated(IUser user)
        {
        }

        void OnConsentGiven(IUser user)
        {
        }
    }
}
