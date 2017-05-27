// ==========================================================================
//  IUserFactory.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

namespace Squidex.Read.Users
{
    public interface IUserFactory
    {
        IUser Create(string email);
    }
}
