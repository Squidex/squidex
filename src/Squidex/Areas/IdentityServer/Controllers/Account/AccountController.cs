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
using System.Security;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using IdentityServer4.Models;
using IdentityServer4.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Squidex.Config;
using Squidex.Domain.Users;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Log;
using Squidex.Infrastructure.Tasks;
using Squidex.Shared;
using Squidex.Shared.Identity;
using Squidex.Shared.Users;
using Squidex.Web;

namespace Squidex.Areas.IdentityServer.Controllers.Account
{
    public sealed class AccountController : IdentityServerController
    {
        private readonly SignInManager<IdentityUser> signInManager;
        private readonly UserManager<IdentityUser> userManager;
        private readonly IUserFactory userFactory;
        private readonly IUserEvents userEvents;
        private readonly IOptions<MyIdentityOptions> identityOptions;
        private readonly ISemanticLog log;
        private readonly IIdentityServerInteractionService interactions;

        public AccountController(
            SignInManager<IdentityUser> signInManager,
            UserManager<IdentityUser> userManager,
            IUserFactory userFactory,
            IUserEvents userEvents,
            IOptions<MyIdentityOptions> identityOptions,
            ISemanticLog log,
            IIdentityServerInteractionService interactions)
        {
            this.log = log;
            this.userEvents = userEvents;
            this.userManager = userManager;
            this.userFactory = userFactory;
            this.interactions = interactions;
            this.identityOptions = identityOptions;
            this.signInManager = signInManager;
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
            throw new SecurityException("User is not allowed to login.");
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
        public IActionResult Consent(string returnUrl = null)
        {
            return View(new ConsentVM { PrivacyUrl = identityOptions.Value.PrivacyUrl, ReturnUrl = returnUrl });
        }

        [HttpPost]
        [Route("account/consent/")]
        public async Task<IActionResult> Consent(ConsentModel model, string returnUrl = null)
        {
            if (!model.ConsentToCookies)
            {
                ModelState.AddModelError(nameof(model.ConsentToCookies), "You have to give consent.");
            }

            if (!model.ConsentToPersonalInformation)
            {
                ModelState.AddModelError(nameof(model.ConsentToPersonalInformation), "You have to give consent.");
            }

            if (!ModelState.IsValid)
            {
                var vm = new ConsentVM { PrivacyUrl = identityOptions.Value.PrivacyUrl, ReturnUrl = returnUrl };

                return View(vm);
            }

            var user = await userManager.GetUserWithClaimsAsync(User);

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
            var context = await interactions.GetLogoutContextAsync(logoutId);

            await signInManager.SignOutAsync();

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
        public Task<IActionResult> Signup(string returnUrl = null)
        {
            return LoginViewAsync(returnUrl, false, false);
        }

        [HttpGet]
        [Route("account/login/")]
        [ClearCookies]
        public Task<IActionResult> Login(string returnUrl = null)
        {
            return LoginViewAsync(returnUrl, true, false);
        }

        [HttpPost]
        [Route("account/login/")]
        public async Task<IActionResult> Login(LoginModel model, string returnUrl = null)
        {
            if (!ModelState.IsValid)
            {
                return await LoginViewAsync(returnUrl, true, true);
            }

            var result = await signInManager.PasswordSignInAsync(model.Email, model.Password, true, true);

            if (!result.Succeeded)
            {
                return await LoginViewAsync(returnUrl, true, true);
            }
            else
            {
                return RedirectToReturnUrl(returnUrl);
            }
        }

        private async Task<IActionResult> LoginViewAsync(string returnUrl, bool isLogin, bool isFailed)
        {
            var allowPasswordAuth = identityOptions.Value.AllowPasswordAuth;

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
        public IActionResult External(string provider, string returnUrl = null)
        {
            var properties =
                signInManager.ConfigureExternalAuthenticationProperties(provider,
                    Url.Action(nameof(ExternalCallback), new { ReturnUrl = returnUrl }));

            return Challenge(properties, provider);
        }

        [HttpGet]
        [Route("account/external-callback/")]
        public async Task<IActionResult> ExternalCallback(string returnUrl = null)
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

            UserWithClaims user;

            if (isLoggedIn)
            {
                user = await userManager.FindByLoginWithClaimsAsync(externalLogin.LoginProvider, externalLogin.ProviderKey);
            }
            else
            {
                var email = externalLogin.Principal.FindFirst(ClaimTypes.Email).Value;

                user = await userManager.FindByEmailWithClaimsAsyncAsync(email);

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
            else if (user != null && !user.HasConsent())
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
            if (isFirst || !identityOptions.Value.LockAutomatically)
            {
                return TaskHelper.True;
            }

            return MakeIdentityOperation(() => userManager.SetLockoutEndDateAsync(user.Identity, DateTimeOffset.UtcNow.AddYears(100)));
        }

        private Task<bool> AddClaimsAsync(UserWithClaims user, ExternalLoginInfo externalLogin, string email, bool isFirst = false)
        {
            var newClaims = new List<Claim>();

            void AddClaim(Claim claim)
            {
                newClaims.Add(claim);

                user.Claims.Add(claim);
            }

            foreach (var squidexClaim in externalLogin.Principal.GetSquidexClaims())
            {
                AddClaim(squidexClaim);
            }

            if (!user.HasPictureUrl())
            {
                AddClaim(new Claim(SquidexClaimTypes.PictureUrl, GravatarHelper.CreatePictureUrl(email)));
            }

            if (!user.HasDisplayName())
            {
                AddClaim(new Claim(SquidexClaimTypes.DisplayName, email));
            }

            if (isFirst)
            {
                AddClaim(new Claim(SquidexClaimTypes.Permissions, Permissions.Admin));
            }

            return MakeIdentityOperation(() => userManager.SyncClaimsAsync(user.Identity, newClaims));
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

        private IActionResult RedirectToReturnUrl(string returnUrl)
        {
            if (!string.IsNullOrWhiteSpace(returnUrl))
            {
                return Redirect(returnUrl);
            }
            else
            {
                return Redirect("~/../");
            }
        }

        private async Task<bool> MakeIdentityOperation(Func<Task<IdentityResult>> action, [CallerMemberName] string operationName = null)
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
