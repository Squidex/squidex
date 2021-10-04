// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Squidex.Areas.Api.Controllers.UI;
using Squidex.Assets;
using Squidex.Config;
using Squidex.Domain.Users;
using Squidex.Hosting;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Reflection;
using Squidex.Infrastructure.Translations;
using Squidex.Infrastructure.Validation;
using Squidex.Web;

namespace Squidex.Areas.IdentityServer.Controllers.Setup
{
    public class SetupController : IdentityServerController
    {
        private readonly IAssetStore assetStore;
        private readonly IUrlGenerator urlGenerator;
        private readonly IUserService userService;
        private readonly MyUIOptions uiOptions;
        private readonly MyIdentityOptions identityOptions;

        public SetupController(
            IAssetStore assetStore,
            IOptions<MyUIOptions> uiOptions,
            IOptions<MyIdentityOptions> identityOptions,
            IUrlGenerator urlGenerator,
            IUserService userService)
        {
            this.assetStore = assetStore;
            this.identityOptions = identityOptions.Value;
            this.uiOptions = uiOptions.Value;
            this.urlGenerator = urlGenerator;
            this.userService = userService;
        }

        [HttpGet]
        [Route("webpack/")]
        public IActionResult Webpack()
        {
            return View();
        }

        [HttpGet]
        [Route("setup/")]
        public async Task<IActionResult> Setup()
        {
            if (!await userService.IsEmptyAsync(HttpContext.RequestAborted))
            {
                return RedirectToReturnUrl(null);
            }

            return View(nameof(Setup), await GetVM(None.Value));
        }

        [HttpPost]
        [Route("setup/")]
        public async Task<IActionResult> Setup(CreateUserModel model)
        {
            if (!await userService.IsEmptyAsync(HttpContext.RequestAborted))
            {
                return RedirectToReturnUrl(null);
            }

            if (!ModelState.IsValid)
            {
                return View(nameof(Profile), await GetVM(model));
            }

            string errorMessage;
            try
            {
                var user = await userService.CreateAsync(model.Email, new UserValues
                {
                    Password = model.Password
                }, ct: HttpContext.RequestAborted);

                await SignInManager.SignInAsync((IdentityUser)user.Identity, true);

                return RedirectToReturnUrl(null);
            }
            catch (ValidationException ex)
            {
                errorMessage = ex.Message;
            }
            catch (Exception)
            {
                errorMessage = T.Get("users.errorHappened");
            }

            return View(nameof(Setup), await GetVM(model, errorMessage));
        }

        private async Task<SetupVM> GetVM<TModel>(TModel? model = null, string? errorMessage = null) where TModel : class
        {
            var externalProviders = await SignInManager.GetExternalProvidersAsync();

            var result = new SetupVM
            {
                BaseUrlConfigured = urlGenerator.BuildUrl(string.Empty, false),
                BaseUrlCurrent = GetCurrentUrl(),
                ErrorMessage = errorMessage,
                EverybodyCanCreateApps = !uiOptions.OnlyAdminsCanCreateApps,
                IsValidHttps = HttpContext.Request.IsHttps,
                IsAssetStoreFile = assetStore is FolderAssetStore,
                IsAssetStoreFtp = assetStore is FTPAssetStore,
                HasExternalLogin = externalProviders.Any(),
                HasPasswordAuth = identityOptions.AllowPasswordAuth
            };

            if (model != null)
            {
                SimpleMapper.Map(model, result);
            }

            return result;
        }

        private string GetCurrentUrl()
        {
            var request = HttpContext.Request;

            var url = $"{request.Scheme}://{request.Host}{request.PathBase}";

            if (url.EndsWith(Constants.PrefixIdentityServer, StringComparison.Ordinal))
            {
                url = url[0..^Constants.PrefixIdentityServer.Length];
            }

            return url.TrimEnd('/');
        }
    }
}
