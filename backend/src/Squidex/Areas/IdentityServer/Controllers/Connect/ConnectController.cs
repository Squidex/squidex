// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using OpenIddict.Abstractions;
using OpenIddict.Server.AspNetCore;
using Squidex.Areas.IdentityServer.Config;
using Squidex.Areas.IdentityServer.Controllers;
using Squidex.Domain.Users;
using Squidex.Shared.Identity;
using Squidex.Web;
using static OpenIddict.Abstractions.OpenIddictConstants;

namespace Notifo.Areas.Account.Controllers
{
    public class AuthorizationController : IdentityServerController
    {
        private readonly IOpenIddictScopeManager scopeManager;
        private readonly IOpenIddictApplicationManager applicationManager;
        private readonly IUserService userService;

        public AuthorizationController(
            IOpenIddictScopeManager scopeManager,
            IOpenIddictApplicationManager applicationManager,
            IUserService userService)
        {
            this.scopeManager = scopeManager;
            this.applicationManager = applicationManager;
            this.userService = userService;
        }

        [HttpPost("connect/token")]
        [Produces("application/json")]
        public async Task<IActionResult> Exchange()
        {
            var request = HttpContext.GetOpenIddictServerRequest();

            if (request == null)
            {
                throw new InvalidOperationException("The OpenID Connect request cannot be retrieved.");
            }

            if (request.IsAuthorizationCodeGrantType() || request.IsRefreshTokenGrantType() || request.IsImplicitFlow())
            {
                var principal = (await HttpContext.AuthenticateAsync(OpenIddictServerAspNetCoreDefaults.AuthenticationScheme)).Principal;

                if (principal == null)
                {
                    throw new InvalidOperationException("The user details cannot be retrieved.");
                }

                var user = await userService.GetAsync(principal, HttpContext.RequestAborted);

                if (user == null)
                {
                    return Forbid(
                        new AuthenticationProperties(new Dictionary<string, string?>
                        {
                            [OpenIddictServerAspNetCoreConstants.Properties.Error] = Errors.InvalidGrant,
                            [OpenIddictServerAspNetCoreConstants.Properties.ErrorDescription] = "The token is no longer valid."
                        }),
                        OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
                }

                if (!await SignInManager.CanSignInAsync((IdentityUser)user.Identity))
                {
                    return Forbid(
                        new AuthenticationProperties(new Dictionary<string, string?>
                        {
                            [OpenIddictServerAspNetCoreConstants.Properties.Error] = Errors.InvalidGrant,
                            [OpenIddictServerAspNetCoreConstants.Properties.ErrorDescription] = "The user is no longer allowed to sign in."
                        }),
                        OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
                }

                foreach (var claim in principal.Claims)
                {
                    claim.SetDestinations(GetDestinations(claim, principal, false));
                }

                return SignIn(principal, OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
            }

            if (request.IsClientCredentialsGrantType())
            {
                if (request.ClientId == null)
                {
                    throw new InvalidOperationException("The OpenID Connect request cannot be retrieved.");
                }

                var application = await applicationManager.FindByClientIdAsync(request.ClientId, HttpContext.RequestAborted);

                if (application == null)
                {
                    throw new InvalidOperationException("The application details cannot be found in the database.");
                }

                var principal = await CreateApplicationPrinicpalAsync(request, application);

                return SignIn(principal, OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
            }

            throw new InvalidOperationException("The specified grant type is not supported.");
        }

        [HttpGet("connect/authorize")]
        public async Task<IActionResult> Authorize()
        {
            var request = HttpContext.GetOpenIddictServerRequest();
            if (request == null)
            {
                throw new InvalidOperationException("The OpenID Connect request cannot be retrieved.");
            }

            if (User.Identity?.IsAuthenticated != true)
            {
                if (request.HasPrompt(Prompts.None))
                {
                    var properties = new AuthenticationProperties(new Dictionary<string, string?>
                    {
                        [OpenIddictServerAspNetCoreConstants.Properties.Error] = Errors.LoginRequired,
                        [OpenIddictServerAspNetCoreConstants.Properties.ErrorDescription] = "The user is not logged in."
                    });

                    return Forbid(properties, OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
                }

                var query = QueryString.Create(
                    Request.HasFormContentType ?
                    Request.Form.ToList() :
                    Request.Query.ToList());

                var redirectUri = Request.PathBase + Request.Path + query;

                return Challenge(
                   new AuthenticationProperties
                   {
                       RedirectUri = redirectUri
                   });
            }

            var user = await userService.GetAsync(User, HttpContext.RequestAborted);

            if (user == null)
            {
                throw new InvalidOperationException("The user details cannot be retrieved.");
            }

            var principal = await SignInManager.CreateUserPrincipalAsync((IdentityUser)user.Identity);

            await EnrichPrincipalAsync(request, principal, false);

            return SignIn(principal, OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
        }

        [HttpGet("connect/logout")]
        public async Task<IActionResult> Logout()
        {
            await SignInManager.SignOutAsync();

            return SignOut(OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
        }

        private async Task<ClaimsPrincipal> CreateApplicationPrinicpalAsync(OpenIddictRequest request, object application)
        {
            var identity = new ClaimsIdentity(
                TokenValidationParameters.DefaultAuthenticationType,
                Claims.Name,
                Claims.Role);

            var principal = new ClaimsPrincipal(identity);

            if (request.ClientId != null)
            {
                identity.AddClaim(Claims.Subject, request.ClientId,
                    Destinations.AccessToken,
                    Destinations.IdentityToken);
            }

            var properties = await applicationManager.GetPropertiesAsync(application, HttpContext.RequestAborted);

            foreach (var claim in properties.Claims())
            {
                identity.AddClaim(claim);
            }

            await EnrichPrincipalAsync(request, principal, true);

            return principal;
        }

        private async Task EnrichPrincipalAsync(OpenIddictRequest request, ClaimsPrincipal principal, bool alwaysDeliverPermissions)
        {
            var scopes = request.GetScopes();

            principal.SetScopes(scopes);
            principal.SetResources(await scopeManager.ListResourcesAsync(scopes, HttpContext.RequestAborted).ToListAsync(HttpContext.RequestAborted));

            foreach (var claim in principal.Claims)
            {
                claim.SetDestinations(GetDestinations(claim, principal, alwaysDeliverPermissions));
            }
        }

        private static IEnumerable<string> GetDestinations(Claim claim, ClaimsPrincipal principal, bool alwaysDeliverPermissions)
        {
            switch (claim.Type)
            {
                case SquidexClaimTypes.DisplayName:
                    yield return Destinations.IdentityToken;
                    yield break;

                case SquidexClaimTypes.PictureUrl when principal.HasScope(Scopes.Profile):
                    yield return Destinations.IdentityToken;
                    yield break;

                case SquidexClaimTypes.NotifoKey when principal.HasScope(Scopes.Profile):
                    yield return Destinations.IdentityToken;
                    yield break;

                case SquidexClaimTypes.Permissions when principal.HasScope(Constants.ScopePermissions) || alwaysDeliverPermissions:
                    yield return Destinations.AccessToken;
                    yield return Destinations.IdentityToken;
                    yield break;

                case Claims.Name:
                    yield return Destinations.AccessToken;

                    if (principal.HasScope(Scopes.Profile))
                    {
                        yield return Destinations.IdentityToken;
                    }

                    yield break;

                case Claims.Email:
                    yield return Destinations.AccessToken;

                    if (principal.HasScope(Scopes.Email))
                    {
                        yield return Destinations.IdentityToken;
                    }

                    yield break;

                case Claims.Role:
                    yield return Destinations.AccessToken;

                    if (principal.HasScope(Scopes.Roles))
                    {
                        yield return Destinations.IdentityToken;
                    }

                    yield break;
            }
        }
    }
}
