// ==========================================================================
//  AccountController.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.MongoDB;
using Microsoft.AspNetCore.Mvc;

// ReSharper disable RedundantIfElseBlock
// ReSharper disable ConvertIfStatementToReturnStatement

namespace Squidex.Modules.UI.Account
{
    public sealed class AccountController : Controller
    {
        private readonly SignInManager<IdentityUser> signInManager;
        private readonly UserManager<IdentityUser> userManager;

        public AccountController(SignInManager<IdentityUser> signInManager, UserManager<IdentityUser> userManager)
        {
            this.signInManager = signInManager;
            this.userManager = userManager;
        }

        [Authorize]
        [HttpGet]
        [Route("account/forbidden")]
        public IActionResult Forbidden()
        {
            return View();
        }

        [HttpGet]
        [Route("client-callback-silent/")]
        public IActionResult ClientSilent()
        {
            return View();
        }

        [HttpGet]
        [Route("client-callback-popup/")]
        public IActionResult ClientPopup()
        {
            return View();
        }

        [HttpGet]
        [Route("account/error/")]
        public IActionResult Error()
        {
            return View();
        }

        [HttpGet]
        [Route("account/login/")]
        public IActionResult Login(string returnUrl = null)
        {
            var providers = 
                signInManager.GetExternalAuthenticationSchemes()
                    .Select(x => new ExternalProvider(x.AuthenticationScheme, x.DisplayName))
                    .ToList();

            return View(new LoginVM { ExternalProviders = providers, ReturnUrl = returnUrl });
        }

        [HttpPost]
        [Route("account/external/")]
        public IActionResult External(string provider, string returnUrl = null)
        {
            var properties = 
                signInManager.ConfigureExternalAuthenticationProperties(provider,
                    Url.Action(nameof(Callback), new { ReturnUrl = returnUrl }));

            return Challenge(properties, provider);
        }

        [HttpGet]
        [Route("account/callback/")]
        public async Task<IActionResult> Callback(string returnUrl = null, string remoteError = null)
        {
            var externalLogin = await signInManager.GetExternalLoginInfoAsync();

            if (externalLogin == null)
            {
                return RedirectToAction(nameof(Login));
            }

            var isLoggedIn = await LoginAsync(externalLogin);

            if (!isLoggedIn)
            {
                var user = CreateUser(externalLogin);

                isLoggedIn =
                    await AddUserAsync(user) &&
                    await AddLoginAsync(user, externalLogin) &&
                    await LoginAsync(externalLogin);
            }

            if (!isLoggedIn)
            {
                return RedirectToAction(nameof(Login));
            }
            else if (!string.IsNullOrWhiteSpace(returnUrl))
            {
                return Redirect(returnUrl);
            }
            else
            {
                return Redirect("~/");
            }
        }

        private async Task<bool> AddLoginAsync(IdentityUser user, UserLoginInfo externalLogin)
        {
            var result = await userManager.AddLoginAsync(user, externalLogin);

            return result.Succeeded;
        }

        private async Task<bool> AddUserAsync(IdentityUser user)
        {
            var result = await userManager.CreateAsync(user);

            return result.Succeeded;
        }

        private async Task<bool> LoginAsync(UserLoginInfo externalLogin)
        {
            var result = await signInManager.ExternalLoginSignInAsync(externalLogin.LoginProvider, externalLogin.ProviderKey, true);

            return result.Succeeded;
        }

        private static IdentityUser CreateUser(ExternalLoginInfo externalLogin)
        {
            var mail = externalLogin.Principal.FindFirst(ClaimTypes.Email).Value;

            return new IdentityUser { Email = mail, UserName = mail };
        }
    }
}
