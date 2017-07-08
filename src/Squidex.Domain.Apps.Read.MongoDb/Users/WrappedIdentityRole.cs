// ==========================================================================
//  WrappedIdentityRole.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using Microsoft.AspNetCore.Identity.MongoDB;
using Squidex.Domain.Apps.Read.Users;

namespace Squidex.Domain.Apps.Read.MongoDb.Users
{
    public sealed class WrappedIdentityRole : IdentityRole, IRole
    {
    }
}
