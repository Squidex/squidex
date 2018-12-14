// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.OAuth;
using Squidex.Infrastructure.Tasks;
using Squidex.Shared.Identity;

namespace Squidex.Config.Authentication
{
    public sealed class GoogleHandler : OAuthEvents
    {
        public override Task RedirectToAuthorizationEndpoint(RedirectContext<OAuthOptions> context)
        {
            context.Response.Redirect(context.RedirectUri + "&prompt=select_account");

            return TaskHelper.Done;
        }

        public override Task CreatingTicket(OAuthCreatingTicketContext context)
        {
            var nameClaim = context.Identity.FindFirst(ClaimTypes.Name)?.Value;

            if (!string.IsNullOrWhiteSpace(nameClaim))
            {
                context.Identity.SetDisplayName(nameClaim);
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
