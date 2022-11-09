// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OpenIddict.Abstractions;
using OpenIddict.Server.AspNetCore;
using Squidex.Domain.Users;
using static OpenIddict.Abstractions.OpenIddictConstants;

namespace Squidex.Areas.IdentityServer.Controllers.UserInfo;

public class UserInfoController : IdentityServerController
{
    private readonly IUserService userService;

    public UserInfoController(IUserService userService)
    {
        this.userService = userService;
    }

    [Authorize(AuthenticationSchemes = OpenIddictServerAspNetCoreDefaults.AuthenticationScheme)]
    [HttpGet]
    [HttpPost]
    [Route("connect/userinfo")]
    [Produces("application/json")]
    public async Task<IActionResult> UserInfo()
    {
        var user = await userService.GetAsync(User, HttpContext.RequestAborted);

        if (user == null)
        {
            return Challenge(
                new AuthenticationProperties(new Dictionary<string, string?>
                {
                    [OpenIddictServerAspNetCoreConstants.Properties.Error] = Errors.InvalidToken,
                    [OpenIddictServerAspNetCoreConstants.Properties.ErrorDescription] = "The specified access token is bound to an account that no longer exists."
                }),
                OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
        }

        var claims = new Dictionary<string, object>(StringComparer.Ordinal)
        {
            [Claims.Subject] = user.Id
        };

        if (User.HasScope(Scopes.Email))
        {
            claims[Claims.Email] = user.Email;
            claims[Claims.EmailVerified] = true;
        }

        if (User.HasScope(Scopes.Roles))
        {
            claims[Claims.Role] = Array.Empty<string>();
        }

        return Ok(claims);
    }
}
