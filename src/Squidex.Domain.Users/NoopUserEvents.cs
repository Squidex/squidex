// ==========================================================================
//  NoopUserEvents.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using Squidex.Shared.Users;

namespace Squidex.Domain.Users
{
    public sealed class NoopUserEvents : IUserEvents
    {
        public void OnUserRegistered(IUser user)
        {
        }
    }
}
