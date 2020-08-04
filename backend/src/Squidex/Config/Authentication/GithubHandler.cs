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
using Squidex.Infrastructure.Translations;
using Squidex.Shared.Identity;

namespace Squidex.Config.Authentication
{
    public sealed class GithubHandler : OAuthEvents
    {
        public override Task CreatingTicket(OAuthCreatingTicketContext context)
        {
            var nameClaim = context.Identity.FindFirst(ClaimTypes.Name)?.Value;

            if (!string.IsNullOrWhiteSpace(nameClaim))
            {
                context.Identity.SetDisplayName(nameClaim);
            }

            if (string.IsNullOrWhiteSpace(context.Identity.FindFirst(ClaimTypes.Email)?.Value))
            {
                throw new DomainException(T.Get("login.githubPrivateEmail"));
            }

            return base.CreatingTicket(context);
        }
    }
}
