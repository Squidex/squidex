// ==========================================================================
//  ProfileController.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using NSwag.Annotations;
using Squidex.Config.Identity;
using Squidex.Infrastructure.Reflection;
using Squidex.Read.Users;

namespace Squidex.Controllers.UI.Profile
{
    [Authorize]
    [SwaggerIgnore]
    public class ProfileController : Controller
    {
        private readonly UserManager<IUser> userManager;
        private readonly IOptions<MyIdentityOptions> identityOptions;

        public ProfileController(UserManager<IUser> userManager, IOptions<MyIdentityOptions> identityOptions)
        {
            this.userManager = userManager;
            this.identityOptions = identityOptions;
        }

        [HttpGet]
        [Route("/account/profile")]
        public async Task<IActionResult> Profile(string successMessage = null)
        {
            var user = await userManager.GetUserAsync(User);

            ViewBag.SuccessMessage = successMessage;

            return View(await GetProfileVM(user));
        }

        [HttpPost]
        [Route("/account/profile")]
        public Task<IActionResult> Profile(ChangeProfileModel model)
        {
            return MakeChangeAsync(async user =>
            {
                user.UpdateEmail(model.Email);
                user.SetDisplayName(model.DisplayName);

                return await userManager.UpdateAsync(user);
            }, "Account updated successfully. Please logout and login again to see the changes.");
        }

        [HttpPost]
        [Route("/account/setpassword")]
        public Task<IActionResult> SetPassword(SetPasswordModel model)
        {
            return MakeChangeAsync(user => userManager.AddPasswordAsync(user, model.Password), 
                "Password set successfully.");
        }

        [HttpPost]
        [Route("/account/changepassword")]
        public Task<IActionResult> ChangePassword(ChangePasswordModel model)
        {
            return MakeChangeAsync(user =>  userManager.ChangePasswordAsync(user, model.OldPassword, model.Password), 
                "Password changed successfully.");
        }

        private async Task<IActionResult> MakeChangeAsync(Func<IUser, Task<IdentityResult>> action, string successMessage, ChangeProfileModel model = null)
        {
            var user = await userManager.GetUserAsync(User);

            if (!ModelState.IsValid)
            {
                return View("Profile", await GetProfileVM(user, model));
            }

            try
            {
                var result = await action(user);

                if (result.Succeeded)
                {
                    return RedirectToAction(nameof(Profile), new { successMessage });
                }

                ViewBag.ErrorMessage = string.Join(". ", result.Errors.Select(x => x.Description));
            }
            catch
            {
                ViewBag.ErrorMessage = "An unexpected exception occurred.";
            }

            return View("Profile", await GetProfileVM(user, model));
        }

        private async Task<ProfileVM> GetProfileVM(IUser user, ChangeProfileModel model = null)
        {
            var result = new ProfileVM
            {
                Email = user.Email,
                DisplayName = user.DisplayName(),
                PictureUrl = user.PictureUrl(),
                HasPassword = await userManager.HasPasswordAsync(user),
                HasPasswordAuth = identityOptions.Value.AllowPasswordAuth
            };

            if (model != null)
            {
                SimpleMapper.Map(model, result);
            }

            return result;
        }
    }
}
