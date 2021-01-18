// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Linq;
using System.Threading.Tasks;
using IdentityModel;
using IdentityServer4;
using IdentityServer4.Extensions;
using IdentityServer4.Models;
using IdentityServer4.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Squidex.Config;
using Squidex.Domain.Users;
using Squidex.Hosting;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Security;
using Squidex.Infrastructure.Translations;
using Squidex.Log;
using Squidex.Shared.Identity;
using Squidex.Shared.Users;
using Squidex.Web;

namespace Squidex.Areas.IdentityServer.Controllers.Account
{
    public sealed class AccountController : IdentityServerController
    {
        private readonly SignInManager<IdentityUser> signInManager;
        private readonly IUserService userService;
        private readonly IUrlGenerator urlGenerator;
        private readonly MyIdentityOptions identityOptions;
        private readonly ISemanticLog log;
        private readonly IIdentityServerInteractionService interactions;

        public AccountController(
            SignInManager<IdentityUser> signInManager,
            IUserService userService,
            IUrlGenerator urlGenerator,
            IOptions<MyIdentityOptions> identityOptions,
            ISemanticLog log,
            IIdentityServerInteractionService interactions)
        {
            this.identityOptions = identityOptions.Value;
            this.interactions = interactions;
            this.signInManager = signInManager;
            this.urlGenerator = urlGenerator;
            this.userService = userService;
            this.log = log;
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

            var user = await userService.GetAsync(User);

            if (user == null)
            {
                throw new DomainException(T.Get("users.userNotFound"));
            }

            var update = new UserValues
            {
                Consent = true,
                ConsentForEmails = model.ConsentToAutomatedEmails
            };

            await userService.UpdateAsync(user.Id, update);

            return RedirectToReturnUrl(returnUrl);
        }

        [HttpGet]
        [Route("account/logout/")]
        public async Task<IActionResult> Logout(string logoutId)
        {
            await signInManager.SignOutAsync();

            if (User.Identity?.IsAuthenticated == true)
            {
                var provider = User.FindFirst(JwtClaimTypes.IdentityProvider)?.Value;

                if (provider != null && provider != IdentityServerConstants.LocalIdentityProvider)
                {
                    var providerSupportsSignout = await HttpContext.GetSchemeSupportsSignOutAsync(provider);

                    if (providerSupportsSignout)
                    {
                        return SignOut(provider);
                    }
                }
            }

            var context = await interactions.GetLogoutContextAsync(logoutId);

            return RedirectToLogoutUrl(context);
        }

        [HttpGet]
        [Route("account/logout-redirect/")]
        public async Task<IActionResult> LogoutRedirect()
        {
            await signInManager.SignOutAsync();

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

            var result = await signInManager.PasswordSignInAsync(model.Email, model.Password, true, true);

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

            var externalProviders = await signInManager.GetExternalProvidersAsync();

            if (externalProviders.Count == 1 && !allowPasswordAuth)
            {
                var provider = externalProviders[0].AuthenticationScheme;

                var properties =
                    signInManager.ConfigureExternalAuthenticationProperties(provider,
                        Url.Action(nameof(ExternalCallback), new { ReturnUrl = returnUrl }));

                return Challenge(properties, provider);
            }

            var vm = new LoginVM
            {
                ExternalProviders = externalProviders,
                IsLogin = isLogin,
                IsFailed = isFailed,
                HasPasswordAuth = allowPasswordAuth,
                HasPasswordAndExternal = allowPasswordAuth && externalProviders.Any(),
                ReturnUrl = returnUrl
            };

            return View(nameof(Login), vm);
        }

        [HttpPost]
        [Route("account/external/")]
        public IActionResult External(string provider, string? returnUrl = null)
        {
            var properties =
                signInManager.ConfigureExternalAuthenticationProperties(provider,
                    Url.Action(nameof(ExternalCallback), new { ReturnUrl = returnUrl }));

            return Challenge(properties, provider);
        }

        [HttpGet]
        [Route("account/external-callback/")]
        public async Task<IActionResult> ExternalCallback(string? returnUrl = null)
        {
            var externalLogin = await signInManager.GetExternalLoginInfoWithDisplayNameAsync();

            if (externalLogin == null)
            {
                return RedirectToAction(nameof(Login));
            }

            var result = await signInManager.ExternalLoginSignInAsync(externalLogin.LoginProvider, externalLogin.ProviderKey, true);

            if (!result.Succeeded && result.IsLockedOut)
            {
                return View(nameof(LockedOut));
            }

            var isLoggedIn = result.Succeeded;

            IUser? user;

            if (isLoggedIn)
            {
                user = await userService.FindByLoginAsync(externalLogin.LoginProvider, externalLogin.ProviderKey);
            }
            else
            {
                var email = externalLogin.Principal.GetEmail();

                if (string.IsNullOrWhiteSpace(email))
                {
                    throw new DomainException("User has no exposed email address.");
                }

                user = await userService.FindByEmailAsync(email);

                if (user != null)
                {
                    var update = CreateUserValues(externalLogin, email, user: user);

                    await userService.UpdateAsync(user.Id, update);
                }
                else
                {
                    var update = CreateUserValues(externalLogin, email);

                    user = await userService.CreateAsync(email, update, identityOptions.LockAutomatically);
                }

                await userService.AddLoginAsync(user.Id, externalLogin);

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
            var result = await signInManager.ExternalLoginSignInAsync(externalLogin.LoginProvider, externalLogin.ProviderKey, true);

            return (result.Succeeded, result.IsLockedOut);
        }

        private IActionResult RedirectToLogoutUrl(LogoutRequest context)
        {
            if (!string.IsNullOrWhiteSpace(context.PostLogoutRedirectUri))
            {
                return Redirect(context.PostLogoutRedirectUri);
            }
            else
            {
                return Redirect("~/../");
            }
        }

        private IActionResult RedirectToReturnUrl(string? returnUrl)
        {
            if (urlGenerator.IsAllowedHost(returnUrl) || interactions.IsValidReturnUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }
            else
            {
                return Redirect("~/../");
            }
        }
    }
}
