// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication.OAuth;
using Squidex.Shared.Identity;

namespace Squidex.Config.Authentication
{
    public sealed class GithubHandler : OAuthEvents
    {
        public override Task CreatingTicket(OAuthCreatingTicketContext context)
        {
            var displayNameClaim = context.Identity.Claims.FirstOrDefault(x => x.Type == ClaimTypes.Name);
            if (displayNameClaim != null)
            {
                context.Identity.SetDisplayName(displayNameClaim.Value);
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
                context.Identity.SetPictureUrl(pictureUrl);
            }

            return base.CreatingTicket(context);
        }
    }
}
