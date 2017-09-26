// ==========================================================================
//  GoogleHandler.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication.OAuth;
using Squidex.Infrastructure.Tasks;
using Squidex.Shared.Identity;

namespace Squidex.Config.Identity
{
    public sealed class GoogleHandler : OAuthEvents
    {
        public override Task RedirectToAuthorizationEndpoint(OAuthRedirectToAuthorizationContext context)
        {
            context.Response.Redirect(context.RedirectUri + "&prompt=select_account");

            return TaskHelper.Done;
        }

        public override Task CreatingTicket(OAuthCreatingTicketContext context)
        {
            var displayNameClaim = context.Identity.Claims.FirstOrDefault(x => x.Type == ClaimTypes.Name);
            if (displayNameClaim != null)
            {
                context.Identity.AddClaim(new Claim(SquidexClaimTypes.SquidexDisplayName, displayNameClaim.Value));
            }

            var pictureUrl = context.User?.Value<string>("picture");

            if (string.IsNullOrWhiteSpace(pictureUrl))
            {
                pictureUrl = context.User?["image"]?.Value<string>("url");

                if (pictureUrl != null && pictureUrl.EndsWith("?sz=50", System.StringComparison.Ordinal))
                {
                    pictureUrl = pictureUrl.Substring(0, pictureUrl.Length - 6);
                }
            }

            if (!string.IsNullOrWhiteSpace(pictureUrl))
            {
                context.Identity.AddClaim(new Claim(SquidexClaimTypes.SquidexPictureUrl, pictureUrl));
            }

            return base.CreatingTicket(context);
        }
    }
}
