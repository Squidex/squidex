// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Security.Claims;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.OAuth;
using Squidex.Shared.Identity;

namespace Squidex.Config.Authentication
{
    public sealed class GoogleHandler : OAuthEvents
    {
        public override Task RedirectToAuthorizationEndpoint(RedirectContext<OAuthOptions> context)
        {
            context.Response.Redirect(context.RedirectUri + "&prompt=select_account");

            return Task.CompletedTask;
        }

        public override Task CreatingTicket(OAuthCreatingTicketContext context)
        {
            var nameClaim = context.Identity.FindFirst(ClaimTypes.Name)?.Value;

            if (!string.IsNullOrWhiteSpace(nameClaim))
            {
                context.Identity.AddClaim(new Claim(SquidexClaimTypes.DisplayName, nameClaim));
            }

            string? pictureUrl = null;

            if (context.User.TryGetProperty("picture", out var picture) && picture.ValueKind == JsonValueKind.String)
            {
                pictureUrl = picture.GetString();
            }

            if (string.IsNullOrWhiteSpace(pictureUrl))
            {
                if (context.User.TryGetProperty("image", out var image) && image.TryGetProperty("url", out var url) && url.ValueKind == JsonValueKind.String)
                {
                    pictureUrl = url.GetString();
                }

                if (pictureUrl != null && pictureUrl.EndsWith("?sz=50", System.StringComparison.Ordinal))
                {
                    pictureUrl = pictureUrl[..^6];
                }
            }

            if (!string.IsNullOrWhiteSpace(pictureUrl))
            {
                context.Identity.AddClaim(new Claim(SquidexClaimTypes.PictureUrl, pictureUrl));
            }

            return base.CreatingTicket(context);
        }
    }
}
