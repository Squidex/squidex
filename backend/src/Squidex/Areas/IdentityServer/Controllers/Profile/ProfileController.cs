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
using Squidex.Assets;
using Squidex.Config;
using Squidex.Domain.Users;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Reflection;
using Squidex.Infrastructure.Tasks;
using Squidex.Infrastructure.Translations;
using Squidex.Shared.Identity;
using Squidex.Shared.Users;

namespace Squidex.Areas.IdentityServer.Controllers.Profile
{
    [Authorize]
    public sealed class ProfileController : IdentityServerController
    {
        private static readonly ResizeOptions ResizeOptions = new ResizeOptions { Width = 128, Height = 128, Mode = ResizeMode.Crop };
        private readonly SignInManager<IdentityUser> signInManager;
        private readonly UserManager<IdentityUser> userManager;
        private readonly IUserPictureStore userPictureStore;
        private readonly IUserEvents userEvents;
        private readonly IAssetThumbnailGenerator assetThumbnailGenerator;
        private readonly MyIdentityOptions identityOptions;

        public ProfileController(
            SignInManager<IdentityUser> signInManager,
            UserManager<IdentityUser> userManager,
            IUserPictureStore userPictureStore,
            IUserEvents userEvents,
            IAssetThumbnailGenerator assetThumbnailGenerator,
            IOptions<MyIdentityOptions> identityOptions)
        {
            this.signInManager = signInManager;
            this.identityOptions = identityOptions.Value;
            this.userManager = userManager;
            this.userPictureStore = userPictureStore;
            this.userEvents = userEvents;
            this.assetThumbnailGenerator = assetThumbnailGenerator;
        }

        [HttpGet]
        [Route("/account/profile/")]
        public async Task<IActionResult> Profile(string? successMessage = null)
        {
            var user = await userManager.GetUserWithClaimsAsync(User);

            return View(await GetProfileVM<None>(user, successMessage: successMessage));
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
            return MakeChangeAsync<None>(u => AddLoginAsync(u),
                T.Get("users.profile.addLoginDone"));
        }

        [HttpPost]
        [Route("/account/profile/update/")]
        public Task<IActionResult> UpdateProfile(ChangeProfileModel model)
        {
            return MakeChangeAsync(u => UpdateAsync(u, model.ToValues()),
                T.Get("users.profile.updateProfileDone"), model);
        }

        [HttpPost]
        [Route("/account/profile/properties/")]
        public Task<IActionResult> UpdateProperties(ChangePropertiesModel model)
        {
            return MakeChangeAsync(u => UpdateAsync(u, model.ToValues()),
                T.Get("users.profile.updatePropertiesDone"), model);
        }

        [HttpPost]
        [Route("/account/profile/login-remove/")]
        public Task<IActionResult> RemoveLogin(RemoveLoginModel model)
        {
            return MakeChangeAsync(u => userManager.RemoveLoginAsync(u, model.LoginProvider, model.ProviderKey),
                T.Get("users.profile.removeLoginDone"), model);
        }

        [HttpPost]
        [Route("/account/profile/password-set/")]
        public Task<IActionResult> SetPassword(SetPasswordModel model)
        {
            return MakeChangeAsync(u => userManager.AddPasswordAsync(u, model.Password),
                T.Get("users.profile.setPasswordDone"), model);
        }

        [HttpPost]
        [Route("/account/profile/password-change/")]
        public Task<IActionResult> ChangePassword(ChangePasswordModel model)
        {
            return MakeChangeAsync(u => userManager.ChangePasswordAsync(u, model.OldPassword, model.Password),
                T.Get("users.profile.changePasswordDone"), model);
        }

        [HttpPost]
        [Route("/account/profile/generate-client-secret/")]
        public Task<IActionResult> GenerateClientSecret()
        {
            return MakeChangeAsync<None>(user => userManager.GenerateClientSecretAsync(user),
                T.Get("users.profile.generateClientDone"));
        }

        [HttpPost]
        [Route("/account/profile/upload-picture/")]
        public Task<IActionResult> UploadPicture(List<IFormFile> file)
        {
            return MakeChangeAsync<None>(user => UpdatePictureAsync(file, user),
                T.Get("users.profile.uploadPictureDone"));
        }

        private async Task<IdentityResult> AddLoginAsync(IdentityUser user)
        {
            var externalLogin = await signInManager.GetExternalLoginInfoWithDisplayNameAsync(userManager.GetUserId(User));

            return await userManager.AddLoginAsync(user, externalLogin);
        }

        private async Task<IdentityResult> UpdateAsync(IdentityUser user, UserValues values)
        {
            var result = await userManager.UpdateSafeAsync(user, values);

            if (result.Succeeded)
            {
                var resolved = await userManager.ResolveUserAsync(user);

                if (resolved != null)
                {
                    userEvents.OnUserUpdated(resolved);
                }
            }

            return result;
        }

        private async Task<IdentityResult> UpdatePictureAsync(List<IFormFile> file, IdentityUser user)
        {
            if (file.Count != 1)
            {
                var description = T.Get("validation.onlyOneFile");

                return IdentityResult.Failed(new IdentityError { Code = "PictureNotOneFile", Description = description });
            }

            using (var thumbnailStream = new MemoryStream())
            {
                try
                {
                    await assetThumbnailGenerator.CreateThumbnailAsync(file[0].OpenReadStream(), thumbnailStream, ResizeOptions);

                    thumbnailStream.Position = 0;
                }
                catch
                {
                    var description = T.Get("validation.notAnImage");

                    return IdentityResult.Failed(new IdentityError { Code = "PictureNotAnImage", Description = description });
                }

                await userPictureStore.UploadAsync(user.Id, thumbnailStream);
            }

            return await userManager.UpdateSafeAsync(user, new UserValues { PictureUrl = SquidexClaimTypes.PictureUrlStore });
        }

        private async Task<IActionResult> MakeChangeAsync<TModel>(Func<IdentityUser, Task<IdentityResult>> action, string successMessage, TModel? model = null) where TModel : class
        {
            var user = await userManager.GetUserWithClaimsAsync(User);

            if (user == null)
            {
                throw new DomainException(T.Get("users.userNotFound"));
            }

            if (!ModelState.IsValid)
            {
                return View(nameof(Profile), await GetProfileVM(user, model));
            }

            string errorMessage;
            try
            {
                var result = await action(user.Identity);

                if (result.Succeeded)
                {
                    await signInManager.SignInAsync(user.Identity, true);

                    return RedirectToAction(nameof(Profile), new { successMessage });
                }

                errorMessage = result.Localize();
            }
            catch
            {
                errorMessage = T.Get("users.errorHappened");
            }

            return View(nameof(Profile), await GetProfileVM(user, model, errorMessage));
        }

        private async Task<ProfileVM> GetProfileVM<TModel>(UserWithClaims? user, TModel? model = null, string? errorMessage = null, string? successMessage = null) where TModel : class
        {
            if (user == null)
            {
                throw new DomainException(T.Get("users.userNotFound"));
            }

            var (providers, hasPassword, logins) = await AsyncHelper.WhenAll(
                signInManager.GetExternalProvidersAsync(),
                userManager.HasPasswordAsync(user.Identity),
                userManager.GetLoginsAsync(user.Identity));

            var result = new ProfileVM
            {
                Id = user.Id,
                ClientSecret = user.ClientSecret()!,
                Email = user.Email,
                ErrorMessage = errorMessage,
                ExternalLogins = logins,
                ExternalProviders = providers,
                DisplayName = user.DisplayName()!,
                HasPassword = hasPassword,
                HasPasswordAuth = identityOptions.AllowPasswordAuth,
                IsHidden = user.IsHidden(),
                SuccessMessage = successMessage
            };

            if (model != null)
            {
                SimpleMapper.Map(model, result);
            }

            result.Properties ??= user.GetCustomProperties().Select(UserProperty.FromTuple).ToList();

            return result;
        }
    }
}
