// ==========================================================================
//  WrappedIdentityRole.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using Microsoft.AspNetCore.Identity.MongoDB;
using Squidex.Read.Users;

namespace Squidex.Read.MongoDb.Users
{
    public sealed class WrappedIdentityRole : IdentityRole, IRole
    {
    }
}
