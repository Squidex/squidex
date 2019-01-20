// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication.OAuth;
using Squidex.Infrastructure;
using Squidex.Shared.Identity;

namespace Squidex.Config.Authentication
{
    public sealed class GithubHandler : OAuthEvents
    {
        private const string NoEmail = "Your email address is set to private in Github. Please set it to public to use Github login.";

        public override Task CreatingTicket(OAuthCreatingTicketContext context)
        {
            var nameClaim = context.Identity.FindFirst(ClaimTypes.Name)?.Value;

            if (!string.IsNullOrWhiteSpace(nameClaim))
            {
                context.Identity.SetDisplayName(nameClaim);
            }

            if (string.IsNullOrWhiteSpace(context.Identity.FindFirst(ClaimTypes.Email)?.Value))
            {
                throw new DomainException(NoEmail);
            }

            return base.CreatingTicket(context);
        }
    }
}
