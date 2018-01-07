// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Shared.Users;

namespace Squidex.Domain.Users
{
    public interface IUserEvents
    {
        void OnUserRegistered(IUser user);
    }
}
