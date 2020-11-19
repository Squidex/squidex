// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Security.Claims;
using System.Text;
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
using Squidex.Infrastructure;
using Squidex.Infrastructure.Security;
using Squidex.Infrastructure.Translations;
using Squidex.Log;
using Squidex.Shared;
using Squidex.Shared.Identity;
using Squidex.Shared.Users;
using Squidex.Web;

#pragma warning disable CA1827 // Do not use Count() or LongCount() when Any() can be used

namespace Squidex.Areas.IdentityServer.Controllers.Account
{
    public sealed class AccountController : IdentityServerController
    {
        private readonly SignInManager<IdentityUser> signInManager;
        private readonly UserManager<IdentityUser> userManager;
        private readonly IUserFactory userFactory;
        private readonly IUserEvents userEvents;
        private readonly UrlsOptions urlsOptions;
        private readonly MyIdentityOptions identityOptions;
        private readonly ISemanticLog log;
        private readonly IIdentityServerInteractionService interactions;

        public AccountController(
            SignInManager<IdentityUser> signInManager,
            UserManager<IdentityUser> userManager,
            IUserFactory userFactory,
            IUserEvents userEvents,
            IOptions<UrlsOptions> urlsOptions,
            IOptions<MyIdentityOptions> identityOptions,
            ISemanticLog log,
            IIdentityServerInteractionService interactions)
        {
            this.identityOptions = identityOptions.Value;
            this.interactions = interactions;
            this.signInManager = signInManager;
            this.urlsOptions = urlsOptions.Value;
            this.userEvents = userEvents;
            this.userFactory = userFactory;
            this.userManager = userManager;
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

            var user = await userManager.GetUserWithClaimsAsync(User);

            if (user == null)
            {
                throw new DomainException(T.Get("users.userNotFound"));
            }

            var update = new UserValues
            {
                Consent = true,
                ConsentForEmails = model.ConsentToAutomatedEmails
            };

            await userManager.UpdateAsync(user.Id, update);

            userEvents.OnConsentGiven(user);

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

            UserWithClaims? user;

            if (isLoggedIn)
            {
                user = await userManager.FindByLoginWithClaimsAsync(externalLogin.LoginProvider, externalLogin.ProviderKey);
            }
            else
            {
                var email = externalLogin.Principal.FindFirst(ClaimTypes.Email)?.Value!;

                user = await userManager.FindByEmailWithClaimsAsync(email);

                if (user != null)
                {
                    isLoggedIn =
                        await AddLoginAsync(user, externalLogin) &&
                        await AddClaimsAsync(user, externalLogin, email) &&
                        await LoginAsync(externalLogin);
                }
                else
                {
                    user = new UserWithClaims(userFactory.Create(email), new List<Claim>());

                    var isFirst = userManager.Users.LongCount() == 0;

                    isLoggedIn =
                        await AddUserAsync(user) &&
                        await AddLoginAsync(user, externalLogin) &&
                        await AddClaimsAsync(user, externalLogin, email, isFirst) &&
                        await LockAsync(user, isFirst) &&
                        await LoginAsync(externalLogin);

                    userEvents.OnUserRegistered(user);

                    if (await userManager.IsLockedOutAsync(user.Identity))
                    {
                        return View(nameof(LockedOut));
                    }
                }
            }

            if (!isLoggedIn)
            {
                return RedirectToAction(nameof(Login));
            }
            else if (user != null && !user.HasConsent() && !identityOptions.NoConsent)
            {
                return RedirectToAction(nameof(Consent), new { returnUrl });
            }
            else
            {
                return RedirectToReturnUrl(returnUrl);
            }
        }

        private Task<bool> AddLoginAsync(UserWithClaims user, UserLoginInfo externalLogin)
        {
            return MakeIdentityOperation(() => userManager.AddLoginAsync(user.Identity, externalLogin));
        }

        private Task<bool> AddUserAsync(UserWithClaims user)
        {
            return MakeIdentityOperation(() => userManager.CreateAsync(user.Identity));
        }

        private async Task<bool> LoginAsync(UserLoginInfo externalLogin)
        {
            var result = await signInManager.ExternalLoginSignInAsync(externalLogin.LoginProvider, externalLogin.ProviderKey, true);

            return result.Succeeded;
        }

        private Task<bool> LockAsync(UserWithClaims user, bool isFirst)
        {
            if (isFirst || !identityOptions.LockAutomatically)
            {
                return Task.FromResult(true);
            }

            return MakeIdentityOperation(() => userManager.SetLockoutEndDateAsync(user.Identity, DateTimeOffset.UtcNow.AddYears(100)));
        }

        private async Task<bool> AddClaimsAsync(UserWithClaims user, ExternalLoginInfo externalLogin, string email, bool isFirst = false)
        {
            var update = new UserValues
            {
                CustomClaims = externalLogin.Principal.GetSquidexClaims().ToList()
            };

            if (!user.HasPictureUrl())
            {
                update.PictureUrl = GravatarHelper.CreatePictureUrl(email);
            }

            if (!user.HasDisplayName())
            {
                update.DisplayName = email;
            }

            if (isFirst)
            {
                update.Permissions = new PermissionSet(Permissions.Admin);
            }

            return await MakeIdentityOperation(() => userManager.SyncClaims(user.Identity, update));
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
            if (urlsOptions.IsAllowedHost(returnUrl) || interactions.IsValidReturnUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }
            else
            {
                return Redirect("~/../");
            }
        }

        private async Task<bool> MakeIdentityOperation(Func<Task<IdentityResult>> action, [CallerMemberName] string? operationName = null)
        {
            try
            {
                var result = await action();

                if (!result.Succeeded)
                {
                    var errorMessageBuilder = new StringBuilder();

                    foreach (var error in result.Errors)
                    {
                        errorMessageBuilder.Append(error.Code);
                        errorMessageBuilder.Append(": ");
                        errorMessageBuilder.AppendLine(error.Description);
                    }

                    var errorMessage = errorMessageBuilder.ToString();

                    log.LogError((operationName, errorMessage), (ctx, w) => w
                        .WriteProperty("action", ctx.operationName)
                        .WriteProperty("status", "Failed")
                        .WriteProperty("message", ctx.errorMessage));
                }

                return result.Succeeded;
            }
            catch (Exception ex)
            {
                log.LogError(ex, operationName, (logOperationName, w) => w
                    .WriteProperty("action", logOperationName)
                    .WriteProperty("status", "Failed"));

                return false;
            }
        }
    }
}
