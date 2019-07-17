// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Squidex.Config;
using Squidex.Domain.Users;
using Squidex.Infrastructure.Assets;
using Squidex.Infrastructure.Reflection;
using Squidex.Shared.Identity;
using Squidex.Shared.Users;

namespace Squidex.Areas.IdentityServer.Controllers.Profile
{
    [Authorize]
    public sealed class ProfileController : IdentityServerController
    {
        private readonly SignInManager<IdentityUser> signInManager;
        private readonly UserManager<IdentityUser> userManager;
        private readonly IUserPictureStore userPictureStore;
        private readonly IAssetThumbnailGenerator assetThumbnailGenerator;
        private readonly MyIdentityOptions identityOptions;

        public ProfileController(
            SignInManager<IdentityUser> signInManager,
            UserManager<IdentityUser> userManager,
            IUserPictureStore userPictureStore,
            IAssetThumbnailGenerator assetThumbnailGenerator,
            IOptions<MyIdentityOptions> identityOptions)
        {
            this.signInManager = signInManager;
            this.identityOptions = identityOptions.Value;
            this.userManager = userManager;
            this.userPictureStore = userPictureStore;
            this.assetThumbnailGenerator = assetThumbnailGenerator;
        }

        [HttpGet]
        [Route("/account/profile/")]
        public async Task<IActionResult> Profile(string successMessage = null)
        {
            var user = await userManager.GetUserWithClaimsAsync(User);

            return View(await GetProfileVM(user, successMessage: successMessage));
        }

        [HttpPost]
        [Route("/account/profile/login-add/")]
        public async Task<IActionResult> AddLogin(string provider)
        {
            await HttpContext.SignOutAsync(IdentityConstants.ExternalScheme);

            var properties =
                signInManager.ConfigureExternalAuthenticationProperties(provider,
                    Url.Action(nameof(AddLoginCallback)), userManager.GetUserId(User));

            return Challenge(properties, provider);
        }

        [HttpGet]
        [Route("/account/profile/login-add-callback/")]
        public Task<IActionResult> AddLoginCallback()
        {
            return MakeChangeAsync(user => AddLoginAsync(user),
                "Login added successfully.");
        }

        [HttpPost]
        [Route("/account/profile/update/")]
        public Task<IActionResult> UpdateProfile(ChangeProfileModel model)
        {
            return MakeChangeAsync(user => userManager.UpdateSafeAsync(user.Identity, model.ToValues()),
                "Account updated successfully.");
        }

        [HttpPost]
        [Route("/account/profile/login-remove/")]
        public Task<IActionResult> RemoveLogin(RemoveLoginModel model)
        {
            return MakeChangeAsync(user => userManager.RemoveLoginAsync(user.Identity, model.LoginProvider, model.ProviderKey),
                "Login provider removed successfully.");
        }

        [HttpPost]
        [Route("/account/profile/password-set/")]
        public Task<IActionResult> SetPassword(SetPasswordModel model)
        {
            return MakeChangeAsync(user => userManager.AddPasswordAsync(user.Identity, model.Password),
                "Password set successfully.");
        }

        [HttpPost]
        [Route("/account/profile/password-change/")]
        public Task<IActionResult> ChangePassword(ChangePasswordModel model)
        {
            return MakeChangeAsync(user => userManager.ChangePasswordAsync(user.Identity, model.OldPassword, model.Password),
                "Password changed successfully.");
        }

        [HttpPost]
        [Route("/account/profile/upload-picture/")]
        public Task<IActionResult> UploadPicture(List<IFormFile> file)
        {
            return MakeChangeAsync(user => UpdatePictureAsync(file, user),
                "Picture uploaded successfully.");
        }

        private async Task<IdentityResult> AddLoginAsync(UserWithClaims user)
        {
            var externalLogin = await signInManager.GetExternalLoginInfoWithDisplayNameAsync(userManager.GetUserId(User));

            return await userManager.AddLoginAsync(user.Identity, externalLogin);
        }

        private async Task<IdentityResult> UpdatePictureAsync(List<IFormFile> file, UserWithClaims user)
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

            return await userManager.UpdateSafeAsync(user.Identity, new UserValues { PictureUrl = SquidexClaimTypes.PictureUrlStore });
        }

        private async Task<IActionResult> MakeChangeAsync(Func<UserWithClaims, Task<IdentityResult>> action, string successMessage, ChangeProfileModel model = null)
        {
            var user = await userManager.GetUserWithClaimsAsync(User);

            if (!ModelState.IsValid)
            {
                return View(nameof(Profile), await GetProfileVM(user, model));
            }

            string errorMessage;
            try
            {
                var result = await action(user);

                if (result.Succeeded)
                {
                    await signInManager.SignInAsync(user.Identity, true);

                    return RedirectToAction(nameof(Profile), new { successMessage });
                }

                errorMessage = string.Join(". ", result.Errors.Select(x => x.Description));
            }
            catch
            {
                errorMessage = "An unexpected exception occurred.";
            }

            return View(nameof(Profile), await GetProfileVM(user, model, errorMessage));
        }

        private async Task<ProfileVM> GetProfileVM(UserWithClaims user, ChangeProfileModel model = null, string errorMessage = null, string successMessage = null)
        {
            var taskForProviders = signInManager.GetExternalProvidersAsync();
            var taskForPassword = userManager.HasPasswordAsync(user.Identity);
            var taskForLogins = userManager.GetLoginsAsync(user.Identity);

            await Task.WhenAll(taskForProviders, taskForPassword, taskForLogins);

            var result = new ProfileVM
            {
                Id = user.Id,
                Email = user.Email,
                ErrorMessage = errorMessage,
                ExternalLogins = taskForLogins.Result,
                ExternalProviders = taskForProviders.Result,
                DisplayName = user.DisplayName(),
                IsHidden = user.IsHidden(),
                HasPassword = taskForPassword.Result,
                HasPasswordAuth = identityOptions.AllowPasswordAuth,
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
