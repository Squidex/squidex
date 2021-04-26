// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Squidex.Areas.Api.Controllers.UI.Models;
using Squidex.Domain.Apps.Entities.Apps;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Commands;
using Squidex.Infrastructure.Security;
using Squidex.Infrastructure.Translations;
using Squidex.Shared;
using Squidex.Web;

namespace Squidex.Areas.Api.Controllers.UI
{
    public sealed class UIController : ApiController
    {
        private static readonly Permission CreateAppPermission = new Permission(Permissions.AdminAppCreate);
        private readonly MyUIOptions uiOptions;
        private readonly IAppUISettings appUISettings;

        public UIController(ICommandBus commandBus, IOptions<MyUIOptions> uiOptions, IAppUISettings appUISettings)
            : base(commandBus)
        {
            this.uiOptions = uiOptions.Value;

            this.appUISettings = appUISettings;
        }

        /// <summary>
        /// Get ui settings.
        /// </summary>
        /// <returns>
        /// 200 => UI settings returned.
        /// </returns>
        [HttpGet]
        [Route("ui/settings/")]
        [ProducesResponseType(typeof(UISettingsDto), StatusCodes.Status200OK)]
        [ApiPermission]
        public IActionResult GetSettings()
        {
            var result = new UISettingsDto
            {
                CanCreateApps = !uiOptions.OnlyAdminsCanCreateApps || Context.UserPermissions.Includes(CreateAppPermission)
            };

            return Ok(result);
        }

        /// <summary>
        /// Get ui settings.
        /// </summary>
        /// <param name="app">The name of the app.</param>
        /// <returns>
        /// 200 => UI settings returned.
        /// 404 => App not found.
        /// </returns>
        [HttpGet]
        [Route("apps/{app}/ui/settings/")]
        [ProducesResponseType(typeof(Dictionary<string, string>), StatusCodes.Status200OK)]
        [ApiPermission]
        public async Task<IActionResult> GetSettings(string app)
        {
            var result = await appUISettings.GetAsync(AppId, null);

            return Ok(result);
        }

        /// <summary>
        /// Get my ui settings.
        /// </summary>
        /// <param name="app">The name of the app.</param>
        /// <returns>
        /// 200 => UI settings returned.
        /// 404 => App not found.
        /// </returns>
        [HttpGet]
        [Route("apps/{app}/ui/settings/me")]
        [ProducesResponseType(typeof(Dictionary<string, string>), StatusCodes.Status200OK)]
        [ApiPermission]
        public async Task<IActionResult> GetUserSettings(string app)
        {
            var result = await appUISettings.GetAsync(AppId, UserId());

            return Ok(result);
        }

        /// <summary>
        /// Set ui settings.
        /// </summary>
        /// <param name="app">The name of the app.</param>
        /// <param name="key">The name of the setting.</param>
        /// <param name="request">The request with the value to update.</param>
        /// <returns>
        /// 200 => UI setting set.
        /// 404 => App not found.
        /// </returns>
        [HttpPut]
        [Route("apps/{app}/ui/settings/{key}")]
        [ApiPermission]
        public async Task<IActionResult> PutSetting(string app, string key, [FromBody] UpdateSettingDto request)
        {
            await appUISettings.SetAsync(AppId, null, key, request.Value);

            return NoContent();
        }

        /// <summary>
        /// Set my ui settings.
        /// </summary>
        /// <param name="app">The name of the app.</param>
        /// <param name="key">The name of the setting.</param>
        /// <param name="request">The request with the value to update.</param>
        /// <returns>
        /// 200 => UI setting set.
        /// 404 => App not found.
        /// </returns>
        [HttpPut]
        [Route("apps/{app}/ui/settings/me/{key}")]
        [ApiPermission]
        public async Task<IActionResult> PutUserSetting(string app, string key, [FromBody] UpdateSettingDto request)
        {
            await appUISettings.SetAsync(AppId, UserId(), key, request.Value);

            return NoContent();
        }

        /// <summary>
        /// Remove ui settings.
        /// </summary>
        /// <param name="app">The name of the app.</param>
        /// <param name="key">The name of the setting.</param>
        /// <returns>
        /// 200 => UI setting removed.
        /// 404 => App not found.
        /// </returns>
        [HttpDelete]
        [Route("apps/{app}/ui/settings/{key}")]
        [ApiPermission]
        public async Task<IActionResult> DeleteSetting(string app, string key)
        {
            await appUISettings.RemoveAsync(AppId, null, key);

            return NoContent();
        }

        /// <summary>
        /// Remove my ui settings.
        /// </summary>
        /// <param name="app">The name of the app.</param>
        /// <param name="key">The name of the setting.</param>
        /// <returns>
        /// 200 => UI setting removed.
        /// 404 => App not found.
        /// </returns>
        [HttpDelete]
        [Route("apps/{app}/ui/settings/me/{key}")]
        [ApiPermission]
        public async Task<IActionResult> DeleteUserSetting(string app, string key)
        {
            await appUISettings.RemoveAsync(AppId, UserId(), key);

            return NoContent();
        }

        private string UserId()
        {
            var subject = User.OpenIdSubject();

            if (string.IsNullOrWhiteSpace(subject))
            {
                throw new DomainForbiddenException(T.Get("common.httpOnlyAsUser"));
            }

            return subject;
        }
    }
}
