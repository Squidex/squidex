// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Microsoft.AspNetCore.Mvc;
using Squidex.Shared.Identity;

namespace Squidex.Areas.IdentityServer.Controllers.Info
{
    public sealed class InfoController : IdentityServerController
    {
        [Route("/status.png")]
        [HttpGet]
        public IActionResult Info()
        {
            var displayName = User.Claims.DisplayName();

            if (!string.IsNullOrWhiteSpace(displayName))
            {
                var stream = typeof(InfoController).Assembly.GetManifestResourceStream("Squidex.Areas.IdentityServer.Controllers.Info.TrackingPixel.png")!;

                return File(stream, "image/png");
            }
            else
            {
                return NotFound();
            }
        }
    }
}
