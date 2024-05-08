// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Microsoft.AspNetCore.Mvc;
using Squidex.Areas.IdentityServer.Config;
using Squidex.Domain.Apps.Core.Teams;

namespace Squidex.Areas.IdentityServer.Controllers.Test;

public sealed class TestController : IdentityServerController
{
    private readonly DynamicSchemeProvider schemes;

    public TestController(DynamicSchemeProvider schemes)
    {
        this.schemes = schemes;
    }

    [Route("test/")]
    public async Task<IActionResult> Test(
        [FromQuery] AuthScheme scheme)
    {
        var id = await schemes.AddTemporarySchemeAsync(scheme, default);

        var challengeRedirectUrl = Url.Action(nameof(Success));
        var challengeProperties = SignInManager.ConfigureExternalAuthenticationProperties(id, challengeRedirectUrl);

        return Challenge(challengeProperties, id);
    }

    [Route("test/success/")]
    public IActionResult Success()
    {
        return View();
    }
}
