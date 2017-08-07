// ==========================================================================
//  AccountController.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Security;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using IdentityServer4.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using NSwag.Annotations;
using Squidex.Config;
using Squidex.Config.Identity;
using Squidex.Domain.Users;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Log;
using Squidex.Infrastructure.Tasks;
using Squidex.Shared.Identity;
using Squidex.Shared.Users;

// ReSharper disable InvertIf
// ReSharper disable RedundantIfElseBlock
// ReSharper disable ConvertIfStatementToReturnStatement

namespace Squidex.Controllers.UI.Account
{
    [SwaggerIgnore]
    public sealed class AccountController : Controller
    {
        private readonly SignInManager<IUser> signInManager;
        private readonly UserManager<IUser> userManager;
        private readonly IUserFactory userFactory;
        private readonly IOptions<MyIdentityOptions> identityOptions;
        private readonly IOptions<MyUrlsOptions> urlOptions;
        private readonly ISemanticLog log;
        private readonly IIdentityServerInteractionService interactions;

        public AccountController(
            SignInManager<IUser> signInManager,
            UserManager<IUser> userManager,
            IUserFactory userFactory,
            IOptions<MyIdentityOptions> identityOptions,
            IOptions<MyUrlsOptions> urlOptions,
            ISemanticLog log,
            IIdentityServerInteractionService interactions)
        {
            this.log = log;
            this.urlOptions = urlOptions;
            this.userManager = userManager;
            this.userFactory = userFactory;
            this.interactions = interactions;
            this.identityOptions = identityOptions;
            this.signInManager = signInManager;
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
        [Route("account/forbidden")]
        public IActionResult Forbidden()
        {
            throw new SecurityException("User is not allowed to login.");
        }

        [HttpGet]
        [Route("account/accessdenied")]
        public IActionResult AccessDenied()
        {
            return View("AccessDenied");
        }

        [HttpGet]
        [Route("account/logout-completed/")]
        public IActionResult LogoutCompleted()
        {
            return View();
        }

        [HttpGet]
        [Route("account/logout/")]
        public async Task<IActionResult> Logout(string logoutId)
        {
            var context = await interactions.GetLogoutContextAsync(logoutId);

            await signInManager.SignOutAsync();

            var logoutUrl = context.PostLogoutRedirectUri;

            if (string.IsNullOrWhiteSpace(logoutUrl))
            {
                logoutUrl = urlOptions.Value.BuildUrl("logout");
            }

            return Redirect(logoutUrl);
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
        public IActionResult Signup(string returnUrl = null)
        {
            return LoginView(returnUrl, false, false);
        }

        [HttpGet]
        [Route("account/login/")]
        public IActionResult Login(string returnUrl = null)
        {
            return LoginView(returnUrl, true, false);
        }

        [HttpPost]
        [Route("account/login/")]
        public async Task<IActionResult> Login(LoginModel model, string returnUrl = null)
        {
            if (!ModelState.IsValid)
            {
                return LoginView(returnUrl, true, true);
            }

            var result = await signInManager.PasswordSignInAsync(model.Email, model.Password, true, true);

            if (!result.Succeeded)
            {
                return LoginView(returnUrl, true, true);
            }
            else if (!string.IsNullOrWhiteSpace(returnUrl))
            {
                return Redirect(returnUrl);
            }
            else
            {
                return Redirect("~/../");
            }
        }

        private IActionResult LoginView(string returnUrl, bool isLogin, bool isFailed)
        {
            var allowPasswordAuth = identityOptions.Value.AllowPasswordAuth;

            var providers =
                signInManager.GetExternalAuthenticationSchemes()
                    .Select(x => new ExternalProvider(x.AuthenticationScheme, x.DisplayName)).ToList();

            var vm = new LoginVM
            {
                ExternalProviders = providers,
                IsLogin = isLogin,
                IsFailed = isFailed,
                HasPasswordAuth = allowPasswordAuth,
                HasPasswordAndExternal = allowPasswordAuth && providers.Any(),
                ReturnUrl = returnUrl
            };

            return View("Login", vm);
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
        public async Task<IActionResult> ExternalCallback(string returnUrl = null, string remoteError = null)
        {
            var externalLogin = await signInManager.GetExternalLoginInfoWithDisplayNameAsync();

            if (externalLogin == null)
            {
                return RedirectToAction(nameof(Login));
            }

            var result = await signInManager.ExternalLoginSignInAsync(externalLogin.LoginProvider, externalLogin.ProviderKey, true);

            if (!result.Succeeded && result.IsLockedOut)
            {
                return View("LockedOut");
            }

            var isLoggedIn = result.Succeeded;

            if (!isLoggedIn)
            {
                var email = externalLogin.Principal.FindFirst(ClaimTypes.Email).Value;

                var user = await userManager.FindByEmailAsync(email);

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

                    if (user.IsLocked)
                    {
                        return View("LockedOut");
                    }
                }
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
                return Redirect("~/../");
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

            if (!externalLogin.Principal.HasClaim(x => x.Type == SquidexClaimTypes.SquidexPictureUrl))
            {
                user.SetClaim(SquidexClaimTypes.SquidexPictureUrl, GravatarHelper.CreatePictureUrl(email));
            }

            if (!externalLogin.Principal.HasClaim(x => x.Type == SquidexClaimTypes.SquidexDisplayName))
            {
                user.SetClaim(SquidexClaimTypes.SquidexDisplayName, email);
            }

            foreach (var squidexClaim in externalLogin.Principal.Claims.Where(c => c.Type.StartsWith(SquidexClaimTypes.Prefix)))
            {
                user.AddClaim(squidexClaim);
            }

            return user;
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
