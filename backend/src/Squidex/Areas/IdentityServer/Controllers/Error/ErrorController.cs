// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Threading.Tasks;
using IdentityServer4.Models;
using IdentityServer4.Services;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Squidex.Infrastructure;

namespace Squidex.Areas.IdentityServer.Controllers.Error
{
    public sealed class ErrorController : IdentityServerController
    {
        private readonly IIdentityServerInteractionService interaction;

        public ErrorController(IIdentityServerInteractionService interaction)
        {
            this.interaction = interaction;
        }

        [Route("error/")]
        public async Task<IActionResult> Error(string? errorId = null)
        {
            await SignInManager.SignOutAsync();

            var vm = new ErrorVM();

            if (!string.IsNullOrWhiteSpace(errorId))
            {
                var message = await interaction.GetErrorContextAsync(errorId);

                if (message != null)
                {
                    vm.Error = message;
                }
            }

            if (vm.Error == null)
            {
                var error = HttpContext.Features.Get<IExceptionHandlerFeature>()?.Error;

                if (error is DomainException exception)
                {
                    vm.Error = new ErrorMessage { ErrorDescription = exception.Message };
                }
                else if (error?.InnerException is DomainException exception2)
                {
                    vm.Error = new ErrorMessage { ErrorDescription = exception2.Message };
                }
            }

            return View("Error", vm);
        }
    }
}
