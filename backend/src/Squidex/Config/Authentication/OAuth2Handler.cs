// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.OAuth;
using Newtonsoft.Json.Linq;

namespace Squidex.Config.Authentication
{
    public sealed class OAuth2Handler : OAuthEvents
    {
        public override Task RedirectToAuthorizationEndpoint(RedirectContext<OAuthOptions> context)
        {
            context.Response.Redirect(context.RedirectUri);

            return Task.CompletedTask;
        }

        public override Task TicketReceived(TicketReceivedContext context)
        {
            var test = context.Request.Body;
            return base.TicketReceived(context);
        }

        public override Task RemoteFailure(RemoteFailureContext context)
        {
            return base.RemoteFailure(context);
        }

        public override Task AccessDenied(AccessDeniedContext context)
        {
            return base.AccessDenied(context);
        }

        public override async Task CreatingTicket(OAuthCreatingTicketContext context)
        {
            if (context.Options.UserInformationEndpoint != null)
            {
                var request = new HttpRequestMessage(HttpMethod.Get, context.Options.UserInformationEndpoint);
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", context.AccessToken);
                request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                var response = await context.Backchannel.SendAsync(request, context.HttpContext.RequestAborted);
                response.EnsureSuccessStatusCode();
                var user = JObject.Parse(await response.Content.ReadAsStringAsync());
                // Store the received authentication token somewhere. In a cookie for example
                context.HttpContext.Response.Cookies.Append("token", context.AccessToken);

                // Execute defined mapping action to create the claims from the received user object
                // Add the Name Identifier claim
                // example:
                var userId = user.Value<string>("username");
                if (!string.IsNullOrEmpty(userId))
                {
                    context.Identity.AddClaim(new Claim(ClaimTypes.NameIdentifier, userId, ClaimValueTypes.String, context.Options.ClaimsIssuer));
                }
            }
        }
    }
}
