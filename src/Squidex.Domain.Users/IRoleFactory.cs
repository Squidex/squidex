// ==========================================================================
//  IRoleFactory.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

namespace Squidex.Domain.Users
{
    public interface IRoleFactory
    {
        IRole Create(string name);
    }
}
