// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Orleans;
using Squidex.Areas.Api.Controllers.UI.Models;
using Squidex.Domain.Apps.Entities.Apps;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Commands;
using Squidex.Infrastructure.Orleans;
using Squidex.Infrastructure.Security;
using Squidex.Shared;
using Squidex.Web;

namespace Squidex.Areas.Api.Controllers.UI
{
    public sealed class UIController : ApiController
    {
        private static readonly Permission CreateAppPermission = new Permission(Permissions.AdminAppCreate);
        private readonly MyUIOptions uiOptions;
        private readonly IGrainFactory grainFactory;

        public UIController(ICommandBus commandBus, IOptions<MyUIOptions> uiOptions, IGrainFactory grainFactory)
            : base(commandBus)
        {
            this.uiOptions = uiOptions.Value;

            this.grainFactory = grainFactory;
        }

        /// <summary>
        /// Get ui settings.
        /// </summary>
        /// <returns>
        /// 200 => UI settings returned.
        /// </returns>
        [HttpGet]
        [Route("ui/settings/")]
        [ProducesResponseType(typeof(UISettingsDto), 200)]
        [ApiPermission]
        public IActionResult GetSettings()
        {
            var result = new UISettingsDto
            {
                CanCreateApps = !uiOptions.OnlyAdminsCanCreateApps || Context.Permissions.Includes(CreateAppPermission)
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
        [ProducesResponseType(typeof(Dictionary<string, string>), 200)]
        [ApiPermission]
        public async Task<IActionResult> GetSettings(string app)
        {
            var result = await GetSettingsGrain(AppKey()).GetAsync();

            return Ok(result.Value);
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
        [ProducesResponseType(typeof(Dictionary<string, string>), 200)]
        [ApiPermission]
        public async Task<IActionResult> GetUserSettings(string app)
        {
            var result = await GetSettingsGrain(UserKey()).GetAsync();

            return Ok(result.Value);
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
            await GetSettingsGrain(AppKey()).SetAsync(key, request.Value.AsJ());

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
            await GetSettingsGrain(UserKey()).SetAsync(key, request.Value.AsJ());

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
            await GetSettingsGrain(AppKey()).RemoveAsync(key);

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
            await GetSettingsGrain(UserKey()).RemoveAsync(key);

            return NoContent();
        }

        private IAppUISettingsGrain GetSettingsGrain(string key)
        {
            return grainFactory.GetGrain<IAppUISettingsGrain>(key);
        }

        private string AppKey()
        {
            return $"{AppId}";
        }

        private string UserKey()
        {
            var subject = User.OpenIdSubject();

            if (string.IsNullOrWhiteSpace(subject))
            {
                throw new DomainForbiddenException("Not allowed for clients.");
            }

            return $"{AppId}_{subject}";
        }
    }
}
