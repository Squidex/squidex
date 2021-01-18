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
using Squidex.Infrastructure.Validation;
using Squidex.Shared.Identity;
using Squidex.Shared.Users;

namespace Squidex.Areas.IdentityServer.Controllers.Profile
{
    [Authorize]
    public sealed class ProfileController : IdentityServerController
    {
        private static readonly ResizeOptions ResizeOptions = new ResizeOptions { Width = 128, Height = 128, Mode = ResizeMode.Crop };
        private readonly SignInManager<IdentityUser> signInManager;
        private readonly IUserPictureStore userPictureStore;
        private readonly IUserService userService;
        private readonly IAssetThumbnailGenerator assetThumbnailGenerator;
        private readonly MyIdentityOptions identityOptions;

        public ProfileController(
            SignInManager<IdentityUser> signInManager,
            IUserPictureStore userPictureStore,
            IUserService userService,
            IAssetThumbnailGenerator assetThumbnailGenerator,
            IOptions<MyIdentityOptions> identityOptions)
        {
            this.signInManager = signInManager;
            this.identityOptions = identityOptions.Value;
            this.userPictureStore = userPictureStore;
            this.userService = userService;
            this.assetThumbnailGenerator = assetThumbnailGenerator;
        }

        [HttpGet]
        [Route("/account/profile/")]
        public async Task<IActionResult> Profile(string? successMessage = null)
        {
            var user = await userService.GetAsync(User);

            return View(await GetProfileVM<None>(user, successMessage: successMessage));
        }

        [HttpPost]
        [Route("/account/profile/login-add/")]
        public async Task<IActionResult> AddLogin(string provider)
        {
            await HttpContext.SignOutAsync(IdentityConstants.ExternalScheme);

            var properties =
                signInManager.ConfigureExternalAuthenticationProperties(provider,
                    Url.Action(nameof(AddLoginCallback)), userService.GetUserId(User));

            return Challenge(properties, provider);
        }

        [HttpGet]
        [Route("/account/profile/login-add-callback/")]
        public Task<IActionResult> AddLoginCallback()
        {
            return MakeChangeAsync<None>(u => AddLoginAsync(u),
                T.Get("users.profile.addLoginDone"), None.Value);
        }

        [HttpPost]
        [Route("/account/profile/update/")]
        public Task<IActionResult> UpdateProfile(ChangeProfileModel model)
        {
            return MakeChangeAsync(id => userService.UpdateAsync(id, model.ToValues()),
                T.Get("users.profile.updateProfileDone"), model);
        }

        [HttpPost]
        [Route("/account/profile/properties/")]
        public Task<IActionResult> UpdateProperties(ChangePropertiesModel model)
        {
            return MakeChangeAsync(id => userService.UpdateAsync(id, model.ToValues()),
                T.Get("users.profile.updatePropertiesDone"), model);
        }

        [HttpPost]
        [Route("/account/profile/login-remove/")]
        public Task<IActionResult> RemoveLogin(RemoveLoginModel model)
        {
            return MakeChangeAsync(id => userService.RemoveLoginAsync(id, model.LoginProvider, model.ProviderKey),
                T.Get("users.profile.removeLoginDone"), model);
        }

        [HttpPost]
        [Route("/account/profile/password-set/")]
        public Task<IActionResult> SetPassword(SetPasswordModel model)
        {
            return MakeChangeAsync(id => userService.SetPasswordAsync(id, model.Password),
                T.Get("users.profile.setPasswordDone"), model);
        }

        [HttpPost]
        [Route("/account/profile/password-change/")]
        public Task<IActionResult> ChangePassword(ChangePasswordModel model)
        {
            return MakeChangeAsync(id => userService.SetPasswordAsync(id, model.Password, model.OldPassword),
                T.Get("users.profile.changePasswordDone"), model);
        }

        [HttpPost]
        [Route("/account/profile/generate-client-secret/")]
        public Task<IActionResult> GenerateClientSecret()
        {
            return MakeChangeAsync(id => GenerateClientSecretAsync(id),
                T.Get("users.profile.generateClientDone"), None.Value);
        }

        [HttpPost]
        [Route("/account/profile/upload-picture/")]
        public Task<IActionResult> UploadPicture(List<IFormFile> file)
        {
            return MakeChangeAsync<None>(user => UpdatePictureAsync(file, user),
                T.Get("users.profile.uploadPictureDone"), None.Value);
        }

        private async Task GenerateClientSecretAsync(string id)
        {
            var update = new UserValues { ClientSecret = RandomHash.New() };

            await userService.UpdateAsync(id, update);
        }

        private async Task AddLoginAsync(string id)
        {
            var externalLogin = await signInManager.GetExternalLoginInfoWithDisplayNameAsync(id);

            await userService.AddLoginAsync(id, externalLogin);
        }

        private async Task UpdatePictureAsync(List<IFormFile> file, string id)
        {
            if (file.Count != 1)
            {
                throw new ValidationException(T.Get("validation.onlyOneFile"));
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
                    throw new ValidationException(T.Get("validation.notAnImage"));
                }

                await userPictureStore.UploadAsync(id, thumbnailStream);
            }

            var update = new UserValues { PictureUrl = SquidexClaimTypes.PictureUrlStore };

            await userService.UpdateAsync(id, update);
        }

        private async Task<IActionResult> MakeChangeAsync<TModel>(Func<string, Task> action, string successMessage, TModel? model = null) where TModel : class
        {
            var user = await userService.GetAsync(User);

            if (user == null)
            {
                return NotFound();
            }

            if (!ModelState.IsValid)
            {
                return View(nameof(Profile), await GetProfileVM(user, model));
            }

            string errorMessage;
            try
            {
                await action(user.Id);

                await signInManager.SignInAsync((IdentityUser)user.Identity, true);

                return RedirectToAction(nameof(Profile), new { successMessage });
            }
            catch (ValidationException ex)
            {
                errorMessage = ex.Message;
            }
            catch (Exception)
            {
                errorMessage = T.Get("users.errorHappened");
            }

            return View(nameof(Profile), await GetProfileVM(user, model, errorMessage));
        }

        private async Task<ProfileVM> GetProfileVM<TModel>(IUser? user, TModel? model = null, string? errorMessage = null, string? successMessage = null) where TModel : class
        {
            if (user == null)
            {
                throw new DomainException(T.Get("users.userNotFound"));
            }

            var (providers, hasPassword, logins) = await AsyncHelper.WhenAll(
                signInManager.GetExternalProvidersAsync(),
                userService.HasPasswordAsync(user),
                userService.GetLoginsAsync(user));

            var result = new ProfileVM
            {
                Id = user.Id,
                ClientSecret = user.Claims.ClientSecret()!,
                Email = user.Email,
                ErrorMessage = errorMessage,
                ExternalLogins = logins,
                ExternalProviders = providers,
                DisplayName = user.Claims.DisplayName()!,
                HasPassword = hasPassword,
                HasPasswordAuth = identityOptions.AllowPasswordAuth,
                IsHidden = user.Claims.IsHidden(),
                SuccessMessage = successMessage
            };

            if (model != null)
            {
                SimpleMapper.Map(model, result);
            }

            result.Properties ??= user.Claims.GetCustomProperties().Select(UserProperty.FromTuple).ToList();

            return result;
        }
    }
}
