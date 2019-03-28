// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Squidex.Shared;
using Squidex.Shared.Identity;

namespace Squidex.Web.Pipeline
{
    public sealed class ApiPermissionUnifier : IClaimsTransformation
    {
        private const string AdministratorRole = "ADMINISTRATOR";

        public Task<ClaimsPrincipal> TransformAsync(ClaimsPrincipal principal)
        {
            var identity = principal.Identities.First();

            if (string.Equals(identity.FindFirst(identity.RoleClaimType)?.Value, AdministratorRole, StringComparison.OrdinalIgnoreCase))
            {
                identity.AddClaim(new Claim(SquidexClaimTypes.Permissions, Permissions.Admin));
            }

            return Task.FromResult(principal);
        }
    }
}
