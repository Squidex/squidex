// ==========================================================================
//  MicrosoftHandler.cs
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
    public sealed class MicrosoftHandler : OAuthEvents
    {
        public override Task CreatingTicket(OAuthCreatingTicketContext context)
        {
            var displayName = context.User.Value<string>("displayName");

            if (!string.IsNullOrEmpty(displayName))
            {
                context.Identity.AddClaim(new Claim(SquidexClaimTypes.SquidexDisplayName, displayName));
            }

            var id = context.User.Value<string>("id");

            if (!string.IsNullOrEmpty(id))
            {
                var pictureUrl = $"https://apis.live.net/v5.0/{id}/picture";

                context.Identity.AddClaim(new Claim(SquidexClaimTypes.SquidexPictureUrl, pictureUrl));
            }

            return base.CreatingTicket(context);
        }
    }
}
