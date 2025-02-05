// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Globalization;
using Microsoft.AspNetCore.Identity;

namespace Squidex.Domain.Users;

public sealed class EFUserFactory : IUserFactory
{
    public IdentityUser Create(string email)
    {
        return new IdentityUser { Email = email, UserName = email };
    }

    public bool IsId(string id)
    {
        return Guid.TryParse(id, CultureInfo.InvariantCulture, out _);
    }
}
