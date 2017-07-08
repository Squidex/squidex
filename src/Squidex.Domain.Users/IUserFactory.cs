// ==========================================================================
//  IUserFactory.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using Squidex.Shared.Users;

namespace Squidex.Domain.Users
{
    public interface IUserFactory
    {
        IUser Create(string email);
    }
}
