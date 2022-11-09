// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Squidex.Infrastructure;

namespace Squidex.Areas.IdentityServer.Controllers.Error;

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

            if (exception is DomainException domainException1)
            {
                vm.ErrorMessage = domainException1.Message;
            }
            else if (exception?.InnerException is DomainException domainException2)
            {
                vm.ErrorMessage = domainException2.Message;
            }
        }

        return View("Error", vm);
    }
}
