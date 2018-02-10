// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication.OAuth;
using Squidex.Shared.Identity;

namespace Squidex.Config.Authentication
{
    public sealed class MicrosoftHandler : OAuthEvents
    {
        public override Task CreatingTicket(OAuthCreatingTicketContext context)
        {
            var displayName = context.User.Value<string>("displayName");

            if (!string.IsNullOrEmpty(displayName))
            {
                context.Identity.SetDisplayName(displayName);
            }

            var id = context.User.Value<string>("id");

            if (!string.IsNullOrEmpty(id))
            {
                var pictureUrl = $"https://apis.live.net/v5.0/{id}/picture";

                context.Identity.SetPictureUrl(pictureUrl);
            }

            return base.CreatingTicket(context);
        }
    }
}
