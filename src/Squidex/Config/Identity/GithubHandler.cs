// ==========================================================================
//  GithubHandler.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication.OAuth;
using Squidex.Shared.Identity;

namespace Squidex.Config.Identity
{
    public sealed class GitHubHandler : OAuthEvents
    {
        public override Task CreatingTicket(OAuthCreatingTicketContext context)
        {
            var userLogin = context.User.Value<string>("login");
            var userName = context.User.Value<string>("name");

            if (!string.IsNullOrEmpty(userName))
            {
                context.Identity.AddClaim(new Claim(SquidexClaimTypes.SquidexDisplayName, userName));
            }
            else if (!string.IsNullOrWhiteSpace(userLogin))
            {
                context.Identity.AddClaim(new Claim(SquidexClaimTypes.SquidexDisplayName, userName));
            }

            var pictureUrl = context.User.Value<string>("avatar_url");

            if (!string.IsNullOrEmpty(pictureUrl))
            {
                context.Identity.AddClaim(new Claim(SquidexClaimTypes.SquidexPictureUrl, pictureUrl));
            }

            return base.CreatingTicket(context);
        }
    }
}
