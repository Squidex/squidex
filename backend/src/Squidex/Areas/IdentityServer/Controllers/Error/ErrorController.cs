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

        if (!string.IsNullOrWhiteSpace(vm.ErrorMessage))
        {
            return View("Error", vm);
        }

        var source = HttpContext.Features.Get<IExceptionHandlerFeature>();

        if (source == null)
        {
            return View("Error", vm);
        }

        var exception = source.Error;

        while (exception?.InnerException != null)
        {
            exception = exception.InnerException;
        }

        if (exception is DomainException || IsTestEndpoint(source))
        {
            vm.ErrorMessage = exception?.Message;
        }

        return View("Error", vm);
    }

    private static bool IsTestEndpoint(IExceptionHandlerFeature source)
    {
        return source.Endpoint is RouteEndpoint route && route.RoutePattern.RawText == "identity-server/test";
    }
}
