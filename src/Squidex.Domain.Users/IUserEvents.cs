// ==========================================================================
//  IUserEvents.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using Squidex.Shared.Users;

namespace Squidex.Domain.Users
{
    public interface IUserEvents
    {
        void OnUserRegistered(IUser user);
    }
}
