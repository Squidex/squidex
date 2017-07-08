// ==========================================================================
//  WrappedIdentityRole.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using Microsoft.AspNetCore.Identity.MongoDB;

namespace Squidex.Domain.Users.MongoDb
{
    public sealed class WrappedIdentityRole : IdentityRole, IRole
    {
    }
}
