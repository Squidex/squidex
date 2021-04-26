// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Security.Claims;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication.OAuth;
using Squidex.Shared.Identity;

namespace Squidex.Config.Authentication
{
    public sealed class MicrosoftHandler : OAuthEvents
    {
        public override Task CreatingTicket(OAuthCreatingTicketContext context)
        {
            string? displayName = null;

            if (context.User.TryGetProperty("displayName", out var element1) && element1.ValueKind == JsonValueKind.String)
            {
                displayName = element1.GetString();
            }

            if (!string.IsNullOrEmpty(displayName))
            {
                context.Identity.AddClaim(new Claim(SquidexClaimTypes.DisplayName, displayName));
            }

            string? id = null;

            if (context.User.TryGetProperty("id", out var element2) && element2.ValueKind == JsonValueKind.String)
            {
                id = element2.GetString();
            }

            if (!string.IsNullOrEmpty(id))
            {
                var pictureUrl = $"https://apis.live.net/v5.0/{id}/picture";

                context.Identity.AddClaim(new Claim(SquidexClaimTypes.PictureUrl, pictureUrl));
            }

            return base.CreatingTicket(context);
        }
    }
}
