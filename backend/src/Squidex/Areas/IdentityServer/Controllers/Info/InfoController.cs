// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Microsoft.AspNetCore.Mvc;
using Squidex.Shared.Identity;

namespace Squidex.Areas.IdentityServer.Controllers.Info;

public sealed class InfoController : IdentityServerController
{
    [Route("info")]
    [HttpGet]
    public IActionResult Info()
    {
        var displayName = User.Claims.DisplayName();

        return Ok(new { displayName });
    }
}
