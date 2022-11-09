// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
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
using Squidex.Web;

namespace Squidex.Areas.IdentityServer.Controllers.Profile;

[Authorize]
public sealed class ProfileController : IdentityServerController
{
    private readonly IUserPictureStore userPictureStore;
    private readonly IUserService userService;
    private readonly IAssetThumbnailGenerator assetThumbnailGenerator;
    private readonly MyIdentityOptions identityOptions;

    public ProfileController(
        IOptions<MyIdentityOptions> identityOptions,
        IUserPictureStore userPictureStore,
        IUserService userService,
        IAssetThumbnailGenerator assetThumbnailGenerator)
    {
        this.identityOptions = identityOptions.Value;
        this.userPictureStore = userPictureStore;
        this.userService = userService;
        this.assetThumbnailGenerator = assetThumbnailGenerator;
    }

    [HttpGet]
    [Route("account/profile/")]
    public async Task<IActionResult> Profile(string? successMessage = null)
    {
        var user = await userService.GetAsync(User, HttpContext.RequestAborted);

        return View(await GetVM<None>(user, successMessage: successMessage));
    }

    [HttpPost]
    [Route("account/profile/login-add/")]
    public async Task<IActionResult> AddLogin(string provider)
    {
        await HttpContext.SignOutAsync(IdentityConstants.ExternalScheme);

        var userId = userService.GetUserId(User, HttpContext.RequestAborted);

        var challengeRedirectUrl = Url.Action(nameof(AddLoginCallback));
        var challengeProperties = SignInManager.ConfigureExternalAuthenticationProperties(provider, challengeRedirectUrl, userId);

        return Challenge(challengeProperties, provider);
    }

    [HttpGet]
    [Route("account/profile/login-add-callback/")]
    public Task<IActionResult> AddLoginCallback()
    {
        return MakeChangeAsync((id, ct) => AddLoginAsync(id, ct),
            T.Get("users.profile.addLoginDone"), None.Value);
    }

    [HttpPost]
    [Route("account/profile/update/")]
    public Task<IActionResult> UpdateProfile(ChangeProfileModel model)
    {
        return MakeChangeAsync((id, ct) => userService.UpdateAsync(id, model.ToValues(), ct: ct),
            T.Get("users.profile.updateProfileDone"), model);
    }

    [HttpPost]
    [Route("account/profile/properties/")]
    public Task<IActionResult> UpdateProperties(ChangePropertiesModel model)
    {
        return MakeChangeAsync((id, ct) => userService.UpdateAsync(id, model.ToValues(), ct: ct),
            T.Get("users.profile.updatePropertiesDone"), model);
    }

    [HttpPost]
    [Route("account/profile/login-remove/")]
    public Task<IActionResult> RemoveLogin(RemoveLoginModel model)
    {
        return MakeChangeAsync((id, ct) => userService.RemoveLoginAsync(id, model.LoginProvider, model.ProviderKey, ct),
            T.Get("users.profile.removeLoginDone"), model);
    }

    [HttpPost]
    [Route("account/profile/password-set/")]
    public Task<IActionResult> SetPassword(SetPasswordModel model)
    {
        return MakeChangeAsync((id, ct) => userService.SetPasswordAsync(id, model.Password, ct: ct),
            T.Get("users.profile.setPasswordDone"), model);
    }

    [HttpPost]
    [Route("account/profile/password-change/")]
    public Task<IActionResult> ChangePassword(ChangePasswordModel model)
    {
        return MakeChangeAsync((id, ct) => userService.SetPasswordAsync(id, model.Password, model.OldPassword, ct),
            T.Get("users.profile.changePasswordDone"), model);
    }

    [HttpPost]
    [Route("account/profile/generate-client-secret/")]
    public Task<IActionResult> GenerateClientSecret()
    {
        return MakeChangeAsync((id, ct) => GenerateClientSecretAsync(id, ct),
            T.Get("users.profile.generateClientDone"), None.Value);
    }

    [HttpPost]
    [Route("account/profile/upload-picture/")]
    public Task<IActionResult> UploadPicture(List<IFormFile> file)
    {
        return MakeChangeAsync((id, ct) => UpdatePictureAsync(file, id, ct),
            T.Get("users.profile.uploadPictureDone"), None.Value);
    }

    private async Task GenerateClientSecretAsync(string id,
        CancellationToken ct)
    {
        var update = new UserValues { ClientSecret = RandomHash.New() };

        await userService.UpdateAsync(id, update, ct: ct);
    }

    private async Task AddLoginAsync(string id,
        CancellationToken ct)
    {
        var login = await SignInManager.GetExternalLoginInfoWithDisplayNameAsync(id);

        await userService.AddLoginAsync(id, login, ct);
    }

    private async Task UpdatePictureAsync(List<IFormFile> files, string id,
        CancellationToken ct)
    {
        if (files.Count != 1)
        {
            throw new ValidationException(T.Get("validation.onlyOneFile"));
        }

        await UploadResizedAsync(files[0], id, ct);

        var update = new UserValues
        {
            PictureUrl = SquidexClaimTypes.PictureUrlStore
        };

        await userService.UpdateAsync(id, update, ct: ct);
    }

    private async Task UploadResizedAsync(IFormFile file, string id,
        CancellationToken ct)
    {
        await using var assetResized = TempAssetFile.Create(file.ToAssetFile());

        var resizeOptions = new ResizeOptions
        {
            TargetWidth = 128,
            TargetHeight = 128
        };

        try
        {
            await using (var originalStream = file.OpenReadStream())
            {
                await using (var resizeStream = assetResized.OpenWrite())
                {
                    await assetThumbnailGenerator.CreateThumbnailAsync(originalStream, file.ContentType, resizeStream, resizeOptions, ct);
                }
            }
        }
        catch
        {
            throw new ValidationException(T.Get("validation.notAnImage"));
        }

        await using (var resizeStream = assetResized.OpenWrite())
        {
            await userPictureStore.UploadAsync(id, resizeStream, ct);
        }
    }

    private async Task<IActionResult> MakeChangeAsync<TModel>(Func<string, CancellationToken, Task> action, string successMessage, TModel? model = null) where TModel : class
    {
        var user = await userService.GetAsync(User, HttpContext.RequestAborted);

        if (user == null)
        {
            return NotFound();
        }

        if (!ModelState.IsValid)
        {
            return View(nameof(Profile), await GetVM(user, model));
        }

        string errorMessage;
        try
        {
            await action(user.Id, HttpContext.RequestAborted);

            await SignInManager.SignInAsync((IdentityUser)user.Identity, true);

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

        return View(nameof(Profile), await GetVM(user, model, errorMessage));
    }

    private async Task<ProfileVM> GetVM<TModel>(IUser? user, TModel? model = null, string? errorMessage = null, string? successMessage = null) where TModel : class
    {
        if (user == null)
        {
            throw new DomainException(T.Get("users.userNotFound"));
        }

        var (providers, hasPassword, logins) = await AsyncHelper.WhenAll(
            SignInManager.GetExternalProvidersAsync(),
            userService.HasPasswordAsync(user, HttpContext.RequestAborted),
            userService.GetLoginsAsync(user, HttpContext.RequestAborted));

        var vm = new ProfileVM
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
            SimpleMapper.Map(model, vm);
        }

        vm.Properties ??= user.Claims.GetCustomProperties().Select(UserProperty.FromTuple).ToList();

        return vm;
    }
}
