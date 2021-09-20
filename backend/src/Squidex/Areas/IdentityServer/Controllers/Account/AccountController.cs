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
using Squidex.Config;
using Squidex.Domain.Users;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Security;
using Squidex.Infrastructure.Translations;
using Squidex.Shared.Identity;
using Squidex.Shared.Users;
using Squidex.Web;

namespace Squidex.Areas.IdentityServer.Controllers.Account
{
    public sealed class AccountController : IdentityServerController
    {
        private readonly IUserService userService;
        private readonly MyIdentityOptions identityOptions;

        public AccountController(
            IUserService userService,
            IOptions<MyIdentityOptions> identityOptions)
        {
            this.identityOptions = identityOptions.Value;
            this.userService = userService;
        }

        [HttpGet]
        [Route("account/error/")]
        public IActionResult LoginError()
        {
            throw new InvalidOperationException();
        }

        [HttpGet]
        [Route("account/forbidden/")]
        public IActionResult Forbidden()
        {
            throw new DomainForbiddenException(T.Get("users.userLocked"));
        }

        [HttpGet]
        [Route("account/lockedout/")]
        public IActionResult LockedOut()
        {
            return View();
        }

        [HttpGet]
        [Route("account/accessdenied/")]
        public IActionResult AccessDenied()
        {
            return View();
        }

        [HttpGet]
        [Route("account/logout-completed/")]
        public IActionResult LogoutCompleted()
        {
            return View();
        }

        [HttpGet]
        [Route("account/consent/")]
        public IActionResult Consent(string? returnUrl = null)
        {
            return View(new ConsentVM { PrivacyUrl = identityOptions.PrivacyUrl, ReturnUrl = returnUrl });
        }

        [HttpPost]
        [Route("account/consent/")]
        public async Task<IActionResult> Consent(ConsentModel model, string? returnUrl = null)
        {
            if (!model.ConsentToCookies)
            {
                ModelState.AddModelError(nameof(model.ConsentToCookies), T.Get("users.consent.needed"));
            }

            if (!model.ConsentToPersonalInformation)
            {
                ModelState.AddModelError(nameof(model.ConsentToPersonalInformation), T.Get("users.consent.needed"));
            }

            if (!ModelState.IsValid)
            {
                var vm = new ConsentVM { PrivacyUrl = identityOptions.PrivacyUrl, ReturnUrl = returnUrl };

                return View(vm);
            }

            var user = await userService.GetAsync(User, HttpContext.RequestAborted);

            if (user == null)
            {
                throw new DomainException(T.Get("users.userNotFound"));
            }

            var update = new UserValues
            {
                Consent = true,
                ConsentForEmails = model.ConsentToAutomatedEmails
            };

            await userService.UpdateAsync(user.Id, update, ct: HttpContext.RequestAborted);

            return RedirectToReturnUrl(returnUrl);
        }

        [HttpGet]
        [Route("account/logout/")]
        public async Task<IActionResult> Logout(string logoutId)
        {
            await SignInManager.SignOutAsync();

            return Redirect("~/../");
        }

        [HttpGet]
        [Route("account/logout-redirect/")]
        public async Task<IActionResult> LogoutRedirect()
        {
            await SignInManager.SignOutAsync();

            return RedirectToAction(nameof(LogoutCompleted));
        }

        [HttpGet]
        [Route("account/signup/")]
        public Task<IActionResult> Signup(string? returnUrl = null)
        {
            return LoginViewAsync(returnUrl, false, false);
        }

        [HttpGet]
        [Route("account/login/")]
        [ClearCookies]
        public Task<IActionResult> Login(string? returnUrl = null)
        {
            return LoginViewAsync(returnUrl, true, false);
        }

        [HttpPost]
        [Route("account/login/")]
        public async Task<IActionResult> Login(LoginModel model, string? returnUrl = null)
        {
            if (!ModelState.IsValid)
            {
                return await LoginViewAsync(returnUrl, true, true);
            }

            var result = await SignInManager.PasswordSignInAsync(model.Email, model.Password, true, true);

            if (!result.Succeeded && result.IsLockedOut)
            {
                return View(nameof(LockedOut));
            }
            else if (!result.Succeeded)
            {
                return await LoginViewAsync(returnUrl, true, true);
            }
            else
            {
                return RedirectToReturnUrl(returnUrl);
            }
        }

        private async Task<IActionResult> LoginViewAsync(string? returnUrl, bool isLogin, bool isFailed)
        {
            var allowPasswordAuth = identityOptions.AllowPasswordAuth;

            var externalProviders = await SignInManager.GetExternalProvidersAsync();

            if (externalProviders.Count == 1 && !allowPasswordAuth)
            {
                var provider = externalProviders[0].AuthenticationScheme;

                var properties =
                    SignInManager.ConfigureExternalAuthenticationProperties(provider,
                        Url.Action(nameof(ExternalCallback), new { ReturnUrl = returnUrl }));

                return Challenge(properties, provider);
            }

            var vm = new LoginVM
            {
                ExternalProviders = externalProviders,
                IsFailed = isFailed,
                IsLogin = isLogin,
                HasPasswordAuth = allowPasswordAuth,
                ReturnUrl = returnUrl
            };

            return View(nameof(Login), vm);
        }

        [HttpPost]
        [Route("account/external/")]
        public IActionResult External(string provider, string? returnUrl = null)
        {
            var properties =
                SignInManager.ConfigureExternalAuthenticationProperties(provider,
                    Url.Action(nameof(ExternalCallback), new { ReturnUrl = returnUrl }));

            return Challenge(properties, provider);
        }

        [HttpGet]
        [Route("account/external-callback/")]
        public async Task<IActionResult> ExternalCallback(string? returnUrl = null)
        {
            var externalLogin = await SignInManager.GetExternalLoginInfoWithDisplayNameAsync();

            if (externalLogin == null)
            {
                return RedirectToAction(nameof(Login));
            }

            var result = await SignInManager.ExternalLoginSignInAsync(externalLogin.LoginProvider, externalLogin.ProviderKey, true);

            if (!result.Succeeded && result.IsLockedOut)
            {
                return View(nameof(LockedOut));
            }

            var isLoggedIn = result.Succeeded;

            IUser? user;

            if (isLoggedIn)
            {
                user = await userService.FindByLoginAsync(externalLogin.LoginProvider, externalLogin.ProviderKey, HttpContext.RequestAborted);
            }
            else
            {
                var email = externalLogin.Principal.GetEmail();

                if (string.IsNullOrWhiteSpace(email))
                {
                    throw new DomainException("User has no exposed email address.");
                }

                user = await userService.FindByEmailAsync(email, HttpContext.RequestAborted);

                if (user != null)
                {
                    var update = CreateUserValues(externalLogin, email, user);

                    await userService.UpdateAsync(user.Id, update, ct: HttpContext.RequestAborted);
                }
                else
                {
                    var update = CreateUserValues(externalLogin, email);

                    user = await userService.CreateAsync(email, update, identityOptions.LockAutomatically, HttpContext.RequestAborted);
                }

                await userService.AddLoginAsync(user.Id, externalLogin, HttpContext.RequestAborted);

                var (success, locked) = await LoginAsync(externalLogin);

                if (locked)
                {
                    return View(nameof(LockedOut));
                }

                isLoggedIn = success;
            }

            if (!isLoggedIn)
            {
                return RedirectToAction(nameof(Login));
            }
            else if (user != null && !user.Claims.HasConsent() && !identityOptions.NoConsent)
            {
                return RedirectToAction(nameof(Consent), new { returnUrl });
            }
            else
            {
                return RedirectToReturnUrl(returnUrl);
            }
        }

        private static UserValues CreateUserValues(ExternalLoginInfo externalLogin, string email, IUser? user = null)
        {
            var values = new UserValues
            {
                CustomClaims = externalLogin.Principal.Claims.GetSquidexClaims().ToList()
            };

            if (user != null && !user.Claims.HasPictureUrl())
            {
                values.PictureUrl = GravatarHelper.CreatePictureUrl(email);
            }

            if (user != null && !user.Claims.HasDisplayName())
            {
                values.DisplayName = email;
            }

            return values;
        }

        private async Task<(bool Success, bool Locked)> LoginAsync(UserLoginInfo externalLogin)
        {
            var result = await SignInManager.ExternalLoginSignInAsync(externalLogin.LoginProvider, externalLogin.ProviderKey, true);

            return (result.Succeeded, result.IsLockedOut);
        }
    }
}
