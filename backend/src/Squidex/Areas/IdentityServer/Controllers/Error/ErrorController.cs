// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Threading.Tasks;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace Squidex.Areas.IdentityServer.Controllers.Error
{
    public sealed class ErrorController : IdentityServerController
    {
        [Route("error/")]
        public async Task<IActionResult> Error(string? errorId = null)
        {
            await SignInManager.SignOutAsync();

            var vm = new ErrorVM();

            var response = HttpContext.GetOpenIddictServerResponse();

            vm.ErrorMessage = response?.ErrorDescription;
            vm.ErrorCode = response?.Error;

            if (string.IsNullOrWhiteSpace(vm.ErrorMessage))
            {
                var exception = HttpContext.Features.Get<IExceptionHandlerFeature>()?.Error;

                vm.ErrorMessage = exception?.Message;
            }

            return View("Error", vm);
        }
    }
}
