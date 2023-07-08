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

internal sealed class UserWithClaims : IUser
{
    private readonly IdentityUser snapshot;

    public IdentityUser Identity { get; }

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

    public IReadOnlyList<Claim> Claims { get; }

    object IUser.Identity => Identity;

    public UserWithClaims(IdentityUser user, IReadOnlyList<Claim> claims)
    {
        Identity = user;

        // Clone the user so that we capture the previous values, even when the user is updated.
        snapshot = SimpleMapper.Map(user, new IdentityUser());

        // Claims are immutable so we do not need a copy of them.
        Claims = claims;
    }
}
