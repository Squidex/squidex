// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using Squidex.Infrastructure.Reflection;
using Squidex.Shared.Users;

namespace Squidex.Domain.Users;

internal sealed class UserWithClaims(IdentityUser user, IReadOnlyList<Claim> claims) : IUser
{
    private readonly IdentityUser snapshot = SimpleMapper.Map(user, new IdentityUser());

    public IdentityUser Identity { get; } = user;

    public string Id
    {
        get => snapshot.Id;
    }

    public string Email
    {
        get => snapshot.Email!;
    }

    public bool IsLocked
    {
        get => snapshot.LockoutEnd > DateTimeOffset.UtcNow;
    }

    public IReadOnlyList<Claim> Claims { get; } = claims;

    object IUser.Identity => Identity;
}
