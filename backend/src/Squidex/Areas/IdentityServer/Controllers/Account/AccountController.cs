// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

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

namespace Squidex.Areas.IdentityServer.Controllers.Account;

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
        // We ask new users to agree to the cookie and privacy agreements and show and error if they do not agree.
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

        // There is almost no case where this could have happened.
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
        // If password authentication is enabled we always show the page.
        var allowPasswordAuth = identityOptions.AllowPasswordAuth;

        var externalProviders = await SignInManager.GetExternalProvidersAsync();

        // If there is only one external authentication provider, we can redirect just directly.
        if (externalProviders.Count == 1 && !allowPasswordAuth)
        {
            var provider = externalProviders[0].AuthenticationScheme;

            var challengeRedirectUrl = Url.Action(nameof(ExternalCallback));
            var challengeProperties = SignInManager.ConfigureExternalAuthenticationProperties(provider, challengeRedirectUrl);

            // Redirect to the external authentication provider.
            return Challenge(challengeProperties, provider);
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
        var challengeRedirectUrl = Url.Action(nameof(ExternalCallback), new { returnUrl });
        var challengeProperties = SignInManager.ConfigureExternalAuthenticationProperties(provider, challengeRedirectUrl);

        return Challenge(challengeProperties, provider);
    }

    [HttpGet]
    [Route("account/external-callback/")]
    public async Task<IActionResult> ExternalCallback(string? returnUrl = null)
    {
        var login = await SignInManager.GetExternalLoginInfoWithDisplayNameAsync();

        if (login == null)
        {
            return RedirectToAction(nameof(Login));
        }

        var result = await SignInManager.ExternalLoginSignInAsync(login.LoginProvider, login.ProviderKey, true);

        if (!result.Succeeded && result.IsLockedOut)
        {
            return View(nameof(LockedOut));
        }

        var isLoggedIn = result.Succeeded;
        var isLocked = false;

        IUser? user = null;

        if (isLoggedIn)
        {
            user = await userService.FindByLoginAsync(login.LoginProvider, login.ProviderKey, HttpContext.RequestAborted);
        }
        else
        {
            var email = login.Principal.GetEmail();

            if (string.IsNullOrWhiteSpace(email))
            {
                throw new DomainException(T.Get("users.noEmailAddress"));
            }

            user = await userService.FindByEmailAsync(email!, HttpContext.RequestAborted);

            // User might not have a login or password if the user got invited.
            if (user != null && await HasLoginAsync(user))
            {
                // If we have a login, we reject this user, otherwise you can login to an account you do not own.
                user = null;
            }

            if (user == null)
            {
                var values = new UserValues
                {
                    CustomClaims = login.Principal.Claims.GetSquidexClaims().ToList()
                };

                var locked = identityOptions.LockAutomatically;

                // Try to create a user. If the user exists an exception message is shown to the user.
                user = await userService.CreateAsync(email!, values, locked, HttpContext.RequestAborted);
            }

            if (user != null)
            {
                await userService.AddLoginAsync(user.Id, login, HttpContext.RequestAborted);

                // Login might fail if the user is locked out.
                (isLoggedIn, isLocked) = await LoginAsync(login);
            }
        }

        if (isLocked)
        {
            return View(nameof(LockedOut));
        }
        else if (!isLoggedIn)
        {
            return RedirectToAction(nameof(Login));
        }
        else if (user != null && !user.Claims.HasConsent() && !identityOptions.NoConsent)
        {
            // This should actually never happen, because user should not be null, when logged in.
            return RedirectToAction(nameof(Consent), new { returnUrl });
        }
        else
        {
            return RedirectToReturnUrl(returnUrl);
        }
    }

    private async Task<bool> HasLoginAsync(IUser user)
    {
        if (await userService.HasPasswordAsync(user, HttpContext.RequestAborted))
        {
            return true;
        }

        var logins = await userService.GetLoginsAsync(user, HttpContext.RequestAborted);

        return logins.Count > 0;
    }

    private async Task<(bool Success, bool Locked)> LoginAsync(UserLoginInfo externalLogin)
    {
        var result = await SignInManager.ExternalLoginSignInAsync(externalLogin.LoginProvider, externalLogin.ProviderKey, true);

        return (result.Succeeded, result.IsLockedOut);
    }
}
