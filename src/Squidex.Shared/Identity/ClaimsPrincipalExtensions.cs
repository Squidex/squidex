// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;

namespace Squidex.Shared.Identity
{
    public static class ClaimsPrincipalExtensions
    {
        public static void SetDisplayName(this ClaimsIdentity identity, string displayName)
        {
            identity.AddClaim(new Claim(SquidexClaimTypes.SquidexDisplayName, displayName));
        }

        public static void SetPictureUrl(this ClaimsIdentity identity, string pictureUrl)
        {
            identity.AddClaim(new Claim(SquidexClaimTypes.SquidexPictureUrl, pictureUrl));
        }

        public static IEnumerable<Claim> GetSquidexClaims(this ClaimsPrincipal principal)
        {
            return principal.Claims.Where(c => c.Type.StartsWith(SquidexClaimTypes.Prefix, StringComparison.Ordinal));
        }
    }
}
