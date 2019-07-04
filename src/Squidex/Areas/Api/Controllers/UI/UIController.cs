// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Orleans;
using Squidex.Areas.Api.Controllers.UI.Models;
using Squidex.Domain.Apps.Entities.Apps;
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
            var result = await grainFactory.GetGrain<IAppUISettingsGrain>(AppId).GetAsync();

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
            await grainFactory.GetGrain<IAppUISettingsGrain>(AppId).SetAsync(key, request.Value.AsJ());

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
            await grainFactory.GetGrain<IAppUISettingsGrain>(AppId).RemoveAsync(key);

            return NoContent();
        }
    }
}
