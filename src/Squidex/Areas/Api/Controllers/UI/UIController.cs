// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Orleans;
using Squidex.Areas.Api.Controllers.UI.Models;
using Squidex.Config;
using Squidex.Domain.Apps.Entities.Apps;
using Squidex.Extensions.Actions.Twitter;
using Squidex.Infrastructure.Commands;
using Squidex.Infrastructure.Orleans;
using Squidex.Pipeline;

namespace Squidex.Areas.Api.Controllers.UI
{
    public sealed class UIController : ApiController
    {
        private readonly MyUIOptions uiOptions;
        private readonly TwitterOptions twitterOptions;
        private readonly IGrainFactory grainFactory;

        public UIController(ICommandBus commandBus,
            IOptions<MyUIOptions> uiOptions,
            IOptions<TwitterOptions> twitterOptions,
            IGrainFactory grainFactory)
            : base(commandBus)
        {
            this.uiOptions = uiOptions.Value;
            this.grainFactory = grainFactory;
            this.twitterOptions = twitterOptions.Value;
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
        [ProducesResponseType(typeof(UISettingsDto), 200)]
        [ApiPermission]
        public async Task<IActionResult> GetSettings(string app)
        {
            var result = await grainFactory.GetGrain<IAppUISettingsGrain>(AppId).GetAsync();

            result.Value.Add("mapType", uiOptions.Map?.Type ?? "OSM");
            result.Value.Add("mapKey", uiOptions.Map?.GoogleMaps?.Key);

            result.Value.Add("supportTwitterAction", twitterOptions.IsConfigured());

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
