// ==========================================================================
//  ICIS Copyright
// ==========================================================================
//  Copyright (c) ICIS
//  All rights reserved.
// ==========================================================================

using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Squidex.ICIS.Interfaces;

namespace Squidex.ICIS
{
    public class ClaimsTransformer : IClaimsTransformation
    {
        private readonly IClaimsManager claimsManager;

        public ClaimsTransformer(IClaimsManager claimsManager)
        {
            this.claimsManager = claimsManager;
        }

        public Task<ClaimsPrincipal> TransformAsync(ClaimsPrincipal principal)
        {
            var identity = principal.Identities.First();
            claimsManager.CreateUserWithClaims(identity);

            return Task.FromResult(principal);
        }
    }
}