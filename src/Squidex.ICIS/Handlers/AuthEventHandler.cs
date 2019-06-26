// ==========================================================================
//  ICIS Copyright
// ==========================================================================
//  Copyright (c) ICIS
//  All rights reserved.
// ==========================================================================

using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Squidex.Infrastructure.Security;

namespace Squidex.ICIS.Handlers
{
    internal class AuthEventsHandler : JwtBearerEvents
    {
        public override Task TokenValidated(TokenValidatedContext context)
        {
            if (context.SecurityToken is JwtSecurityToken accessToken)
            {
                if (context.Principal.Identity is ClaimsIdentity identity)
                {
                    var nameClaim = identity.FindFirst(ClaimTypes.GivenName)?.Value;
                    if (string.IsNullOrWhiteSpace(nameClaim))
                    {
                        identity.AddClaim(new Claim(ClaimTypes.GivenName, accessToken.Payload[OpenIdClaims.ClientId].ToString()));
                    }

                    if (!string.IsNullOrWhiteSpace(accessToken.Subject))
                    {
                        identity.AddClaim(new Claim(OpenIdClaims.Subject, accessToken.Subject));
                    }
                    else
                    {
                        identity.AddClaim(new Claim(OpenIdClaims.Subject, accessToken.Payload[OpenIdClaims.ClientId].ToString()));
                    }

                    if (accessToken.Payload.Keys.Contains(OpenIdClaims.Email) && accessToken.Payload[OpenIdClaims.Email] != null)
                    {
                        var email = accessToken.Payload[OpenIdClaims.Email].ToString();
                        if (!string.IsNullOrWhiteSpace(email))
                        {
                            identity.AddClaim(new Claim(OpenIdClaims.Email, email));
                        }
                    }
                    else
                    {
                        identity.AddClaim(new Claim(OpenIdClaims.Email, accessToken.Payload[OpenIdClaims.ClientId].ToString()));
                    }

                }
            }

            return base.TokenValidated(context);
        }

        
    }
}