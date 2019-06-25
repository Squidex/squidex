// ==========================================================================
//  ICIS Copyright
// ==========================================================================
//  Copyright (c) ICIS
//  All rights reserved.
// ==========================================================================

using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using Squidex.ICIS.Extensions;
using Squidex.ICIS.Interfaces;
using Squidex.ICIS.Models;
using Squidex.Infrastructure.Security;

namespace Squidex.ICIS
{
    public class ClaimsTransformer : IClaimsTransformation
    {
        private readonly IClaimsManager claimsManager;
        private readonly MemoryCache _memoryCache = new MemoryCache(Options.Create(new MemoryCacheOptions()));


        public ClaimsTransformer(IClaimsManager claimsManager)
        {
            this.claimsManager = claimsManager;
        }

        public Task<ClaimsPrincipal> TransformAsync(ClaimsPrincipal principal)
        {
            var identity = principal.Identities.First();

            var key = identity.Claims.FirstOrDefault(claim =>
                claim.Type.Equals(OpenIdClaims.Subject, StringComparison.OrdinalIgnoreCase))?.Value;

            if (!_memoryCache.TryGetValue(key, out UserInfo cachedEntry))
            {
                var userInfo = claimsManager.CreateUserWithClaims(identity);
                _memoryCache.Set(key, userInfo);
                CreateClaims(identity, userInfo);
            }
            else
            {
                CreateClaims(identity, cachedEntry);
            }

            return Task.FromResult(principal);
        }

        private void CreateClaims(ClaimsIdentity identity, UserInfo userInfo)
        {
            var squidexClaims = userInfo.ToUserValues().ToClaims();
            foreach (var squidexClaim in from squidexClaim in squidexClaims
                let found = identity.HasClaim((claim) => AreClaimsEqual(claim, squidexClaim))
                where !found
                select squidexClaim)
            {
                identity.AddClaim(squidexClaim);
            }
        }

        private bool AreClaimsEqual(Claim identityClaim, Claim squidexClaim)
        {
            return identityClaim.Type.Equals(squidexClaim.Type, StringComparison.OrdinalIgnoreCase) &&
                   identityClaim.Value.Equals(squidexClaim.Value, StringComparison.OrdinalIgnoreCase);
        }
    }
}