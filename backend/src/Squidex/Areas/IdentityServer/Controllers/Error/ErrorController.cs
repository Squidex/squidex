// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Text.RegularExpressions;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Squidex.Config;
using Squidex.Infrastructure;

namespace Squidex.Areas.IdentityServer.Controllers.Error;

public sealed class ErrorController(IOptions<MyIdentityOptions> identityOptions) : IdentityServerController
{
    private readonly MyIdentityOptions identityOptions = identityOptions.Value;

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
        else if (TryGetMappedError(exception, out var mappedError))
        {
            vm.ErrorMessage = mappedError;
        }

        return View("Error", vm);
    }

    private bool TryGetMappedError(Exception? exception, out string message)
    {
        message = null!;

        if (exception == null || identityOptions.OidcErrorMap == null || identityOptions.OidcErrorMap.Count == 0)
        {
            return false;
        }

        foreach (var (pattern, value) in identityOptions.OidcErrorMap)
        {
            try
            {
                if (Regex.IsMatch(exception.Message, pattern))
                {
                    message = value;
                    return true;
                }
            }
            catch
            {
                continue;
            }
        }

        return false;
    }

    private static bool IsTestEndpoint(IExceptionHandlerFeature source)
    {
        return source.Endpoint is RouteEndpoint route && route.RoutePattern.RawText == "identity-server/test";
    }
}
