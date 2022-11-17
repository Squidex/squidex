// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Security.Claims;
using Squidex.Shared.Identity;
using Squidex.Shared.Users;

namespace Squidex.Domain.Apps.Core.TestHelpers;

public static class UserMocks
{
    public static IUser User(string id, string? email = null, string? name = null, bool consent = false)
    {
        var claims = new List<Claim>();

        if (!string.IsNullOrWhiteSpace(name))
        {
            claims.Add(new Claim(SquidexClaimTypes.DisplayName, name));
        }

        if (consent)
        {
            claims.Add(new Claim(SquidexClaimTypes.Consent, "True"));
        }

        var user = A.Fake<IUser>();

        A.CallTo(() => user.Id)
            .Returns(id);

        A.CallTo(() => user.Email)
            .Returns(email ?? id);

        A.CallTo(() => user.Claims)
            .Returns(claims);

        return user;
    }
}
