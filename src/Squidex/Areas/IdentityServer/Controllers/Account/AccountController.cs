// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
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
using NSwag.Annotations;
using Squidex.Config;
using Squidex.Domain.Users;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Log;
using Squidex.Infrastructure.Tasks;
using Squidex.Shared.Identity;
using Squidex.Shared.Users;

namespace Squidex.Areas.IdentityServer.Controllers.Account
{
    [SwaggerIgnore]
    public sealed class AccountController : IdentityServerController
    {
        private readonly SignInManager<IUser> signInManager;
        private readonly UserManager<IUser> userManager;
        private readonly IUserFactory userFactory;
        private readonly IUserEvents userEvents;
        private readonly IOptions<MyIdentityOptions> identityOptions;
        private readonly IOptions<MyUrlsOptions> urlOptions;
        private readonly ISemanticLog log;
        private readonly IIdentityServerInteractionService interactions;

        public AccountController(
            SignInManager<IUser> signInManager,
            UserManager<IUser> userManager,
            IUserFactory userFactory,
            IUserEvents userEvents,
            IOptions<MyIdentityOptions> identityOptions,
            IOptions<MyUrlsOptions> urlOptions,
            ISemanticLog log,
            IIdentityServerInteractionService interactions)
        {
            this.log = log;
            this.urlOptions = urlOptions;
            this.userEvents = userEvents;
            this.userManager = userManager;
            this.userFactory = userFactory;
            this.interactions = interactions;
            this.identityOptions = identityOptions;
            this.signInManager = signInManager;
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

            var user = await userManager.GetUserAsync(User);

            user.SetConsentForEmails(model.ConsentToAutomatedEmails);
            user.SetConsent();

            await userManager.UpdateAsync(user);

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

            IUser user = null;

            if (!isLoggedIn)
            {
                var email = externalLogin.Principal.FindFirst(ClaimTypes.Email).Value;

                user = await userManager.FindByEmailAsync(email);

                if (user != null)
                {
                    isLoggedIn =
                        await AddLoginAsync(user, externalLogin) &&
                        await LoginAsync(externalLogin);
                }
                else
                {
                    user = CreateUser(externalLogin, email);

                    var isFirst = userManager.Users.LongCount() == 0;

                    isLoggedIn =
                        await AddUserAsync(user) &&
                        await AddLoginAsync(user, externalLogin) &&
                        await MakeAdminAsync(user, isFirst) &&
                        await LockAsync(user, isFirst) &&
                        await LoginAsync(externalLogin);

                    userEvents.OnUserRegistered(user);

                    if (user.IsLocked)
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

        private Task<bool> AddLoginAsync(IUser user, UserLoginInfo externalLogin)
        {
            return MakeIdentityOperation(() => userManager.AddLoginAsync(user, externalLogin));
        }

        private Task<bool> AddUserAsync(IUser user)
        {
            return MakeIdentityOperation(() => userManager.CreateAsync(user));
        }

        private async Task<bool> LoginAsync(UserLoginInfo externalLogin)
        {
            var result = await signInManager.ExternalLoginSignInAsync(externalLogin.LoginProvider, externalLogin.ProviderKey, true);

            return result.Succeeded;
        }

        private Task<bool> LockAsync(IUser user, bool isFirst)
        {
            if (isFirst || !identityOptions.Value.LockAutomatically)
            {
                return TaskHelper.True;
            }

            return MakeIdentityOperation(() => userManager.SetLockoutEndDateAsync(user, DateTimeOffset.UtcNow.AddYears(100)));
        }

        private Task<bool> MakeAdminAsync(IUser user, bool isFirst)
        {
            if (!isFirst)
            {
                return TaskHelper.True;
            }

            return MakeIdentityOperation(() => userManager.AddToRoleAsync(user, SquidexRoles.Administrator));
        }

        private IUser CreateUser(ExternalLoginInfo externalLogin, string email)
        {
            var user = userFactory.Create(email);

            foreach (var squidexClaim in externalLogin.Principal.GetSquidexClaims())
            {
                user.AddClaim(squidexClaim);
            }

            if (!user.HasPictureUrl())
            {
                user.SetPictureUrl(GravatarHelper.CreatePictureUrl(email));
            }

            if (!user.HasDisplayName())
            {
                user.SetDisplayName(email);
            }

            return user;
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

                    log.LogError(w => w
                        .WriteProperty("action", operationName)
                        .WriteProperty("status", "Failed")
                        .WriteProperty("message", errorMessageBuilder.ToString()));
                }

                return result.Succeeded;
            }
            catch (Exception ex)
            {
                log.LogError(ex, w => w
                    .WriteProperty("action", operationName)
                    .WriteProperty("status", "Failed"));

                return false;
            }
        }
    }
}
