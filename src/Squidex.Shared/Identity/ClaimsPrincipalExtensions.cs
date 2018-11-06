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
using Squidex.Infrastructure.Security;

namespace Squidex.Shared.Identity
{
    public static class ClaimsPrincipalExtensions
    {
        public static void SetDisplayName(this ClaimsIdentity identity, string displayName)
        {
            identity.AddClaim(new Claim(SquidexClaimTypes.DisplayName, displayName));
        }

        public static void SetPictureUrl(this ClaimsIdentity identity, string pictureUrl)
        {
            identity.AddClaim(new Claim(SquidexClaimTypes.PictureUrl, pictureUrl));
        }

        public static PermissionSet Permissions(this ClaimsPrincipal principal)
        {
            return new PermissionSet(principal.Claims.Where(x => x.Type == SquidexClaimTypes.Permissions).Select(x => new Permission(x.Value)));
        }

        public static IEnumerable<Claim> GetSquidexClaims(this ClaimsPrincipal principal)
        {
            return principal.Claims.Where(c => c.Type.StartsWith(SquidexClaimTypes.Prefix, StringComparison.Ordinal));
        }
    }
}
