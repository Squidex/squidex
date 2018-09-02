// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using NSwag.Annotations;
using Orleans;
using Squidex.Areas.Api.Controllers.UI.Models;
using Squidex.Config;
using Squidex.Domain.Apps.Entities.Apps;
using Squidex.Extensions.Actions.Twitter;
using Squidex.Infrastructure.Commands;
using Squidex.Pipeline;

namespace Squidex.Areas.Api.Controllers.UI
{
    /// <summary>
    /// Manages ui settings and configs.
    /// </summary>
    [ApiAuthorize]
    [ApiExceptionFilter]
    [AppApi]
    [SwaggerTag(nameof(UI))]
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
        [ApiCosts(0)]
        public async Task<IActionResult> GetSettings(string app)
        {
            var result = await grainFactory.GetGrain<IAppUISettingsGrain>(App.Id).GetAsync();

            result.Value["mapType"] = uiOptions.Map?.Type ?? "OSM";
            result.Value["mapKey"] = uiOptions.Map?.GoogleMaps?.Key;
            result.Value["supportTwitterAction"] = twitterOptions.IsConfigured();

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
        [ApiCosts(0)]
        public async Task<IActionResult> PutSetting(string app, string key, [FromBody] UpdateSettingDto request)
        {
            await grainFactory.GetGrain<IAppUISettingsGrain>(App.Id).SetAsync(key, request.Value);

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
        [ApiCosts(0)]
        public async Task<IActionResult> DeleteSetting(string app, string key)
        {
            await grainFactory.GetGrain<IAppUISettingsGrain>(App.Id).RemoveAsync(key);

            return NoContent();
        }
    }
}
