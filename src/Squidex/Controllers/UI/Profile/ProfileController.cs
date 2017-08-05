// ==========================================================================
//  ProfileController.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using NSwag.Annotations;
using Squidex.Config.Identity;
using Squidex.Domain.Users;
using Squidex.Infrastructure.Assets;
using Squidex.Infrastructure.Reflection;
using Squidex.Shared.Users;

namespace Squidex.Controllers.UI.Profile
{
    [Authorize]
    [SwaggerIgnore]
    public class ProfileController : Controller
    {
        private readonly SignInManager<IUser> signInManager;
        private readonly UserManager<IUser> userManager;
        private readonly IUserPictureStore userPictureStore;
        private readonly IAssetThumbnailGenerator assetThumbnailGenerator;
        private readonly IOptions<MyIdentityOptions> identityOptions;
        private readonly IOptions<IdentityCookieOptions> identityCookieOptions;

        public ProfileController(
            SignInManager<IUser> signInManager,
            UserManager<IUser> userManager,
            IUserPictureStore userPictureStore,
            IAssetThumbnailGenerator assetThumbnailGenerator,
            IOptions<MyIdentityOptions> identityOptions,
            IOptions<IdentityCookieOptions> identityCookieOptions)
        {
            this.signInManager = signInManager;
            this.identityOptions = identityOptions;
            this.identityCookieOptions = identityCookieOptions;
            this.userManager = userManager;
            this.userPictureStore = userPictureStore;
            this.assetThumbnailGenerator = assetThumbnailGenerator;
        }

        [HttpGet]
        [Route("/account/profile")]
        public async Task<IActionResult> Profile(string successMessage = null)
        {
            var user = await userManager.GetUserAsync(User);

            return View(await GetProfileVM(user, successMessage: successMessage));
        }

        [HttpPost]
        [Route("/account/profile/login-add/")]
        public async Task<IActionResult> AddLogin(string provider)
        {
            await HttpContext.Authentication.SignOutAsync(identityCookieOptions.Value.ExternalCookieAuthenticationScheme);

            var properties =
                signInManager.ConfigureExternalAuthenticationProperties(provider,
                    Url.Action(nameof(AddLoginCallback)), userManager.GetUserId(User));

            return Challenge(properties, provider);
        }

        [HttpGet]
        [Route("/account/profile/login-add-callback/")]
        public Task<IActionResult> AddLoginCallback(string remoteError = null)
        {
            return MakeChangeAsync(async user =>
            {
                var externalLogin = await signInManager.GetExternalLoginInfoWithDisplayNameAsync(userManager.GetUserId(User));

                return await userManager.AddLoginAsync(user, externalLogin);
            }, "Login added successfully.");
        }

        [HttpPost]
        [Route("/account/profile/update")]
        public Task<IActionResult> Profile(ChangeProfileModel model)
        {
            return MakeChangeAsync(user => userManager.UpdateAsync(user, model.Email, model.DisplayName),
                "Account updated successfully.");
        }

        [HttpPost]
        [Route("/account/profile/login-remove")]
        public Task<IActionResult> RemoveLogin(RemoveLoginModel model)
        {
            return MakeChangeAsync(user => userManager.RemoveLoginAsync(user, model.LoginProvider, model.ProviderKey),
                "Login provider removed successfully.");
        }

        [HttpPost]
        [Route("/account/profile/password-set")]
        public Task<IActionResult> SetPassword(SetPasswordModel model)
        {
            return MakeChangeAsync(user => userManager.AddPasswordAsync(user, model.Password),
                "Password set successfully.");
        }

        [HttpPost]
        [Route("/account/profile/password-change")]
        public Task<IActionResult> ChangePassword(ChangePasswordModel model)
        {
            return MakeChangeAsync(user =>  userManager.ChangePasswordAsync(user, model.OldPassword, model.Password),
                "Password changed successfully.");
        }

        [HttpPost]
        [Route("/account/profile/upload-picture")]
        public Task<IActionResult> UploadPicture(List<IFormFile> file)
        {
            return MakeChangeAsync(async user =>
            {
                if (file.Count != 1)
                {
                    return IdentityResult.Failed(new IdentityError { Description = "Please upload a single file." });
                }

                var thumbnailStream = new MemoryStream();
                try
                {
                    await assetThumbnailGenerator.CreateThumbnailAsync(file[0].OpenReadStream(), thumbnailStream, 128, 128, "Crop");

                    thumbnailStream.Position = 0;
                }
                catch
                {
                    return IdentityResult.Failed(new IdentityError { Description = "Picture is not a valid image." });
                }

                await userPictureStore.UploadAsync(user.Id, thumbnailStream);

                user.SetPictureUrlToStore();

                return await userManager.UpdateAsync(user);
            }, "Picture uploaded successfully.");
        }

        private async Task<IActionResult> MakeChangeAsync(Func<IUser, Task<IdentityResult>> action, string successMessage, ChangeProfileModel model = null)
        {
            var user = await userManager.GetUserAsync(User);

            if (!ModelState.IsValid)
            {
                return View("Profile", await GetProfileVM(user, model));
            }

            string errorMessage;
            try
            {
                var result = await action(user);

                if (result.Succeeded)
                {
                    await signInManager.SignInAsync(user, true);

                    return RedirectToAction(nameof(Profile), new { successMessage });
                }

                errorMessage = string.Join(". ", result.Errors.Select(x => x.Description));
            }
            catch
            {
                errorMessage = "An unexpected exception occurred.";
            }

            return View("Profile", await GetProfileVM(user, model, errorMessage));
        }

        private async Task<ProfileVM> GetProfileVM(IUser user, ChangeProfileModel model = null, string errorMessage = null, string successMessage = null)
        {
            var providers =
                signInManager.GetExternalAuthenticationSchemes()
                    .Select(x => new ExternalProvider(x.AuthenticationScheme, x.DisplayName)).ToList();

            var result = new ProfileVM
            {
                Id = user.Id,
                Email = user.Email,
                ErrorMessage = errorMessage,
                ExternalLogins = user.Logins,
                ExternalProviders = providers,
                DisplayName = user.DisplayName(),
                HasPassword = await userManager.HasPasswordAsync(user),
                HasPasswordAuth = identityOptions.Value.AllowPasswordAuth,
                SuccessMessage = successMessage
            };

            if (model != null)
            {
                SimpleMapper.Map(model, result);
            }

            return result;
        }
    }
}
