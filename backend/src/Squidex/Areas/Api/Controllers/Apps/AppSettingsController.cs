// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Squidex.Areas.Api.Controllers.Apps.Models;
using Squidex.Domain.Apps.Entities.Apps;
using Squidex.Infrastructure.Commands;
using Squidex.Shared;
using Squidex.Web;

namespace Squidex.Areas.Api.Controllers.Apps
{
    /// <summary>
    /// Manages and configures apps.
    /// </summary>
    [ApiExplorerSettings(GroupName = nameof(Apps))]
    public sealed class AppSettingsController : ApiController
    {
        public AppSettingsController(ICommandBus commandBus)
            : base(commandBus)
        {
        }

        /// <summary>
        /// Get the app settings.
        /// </summary>
        /// <param name="app">The name of the app to get the settings for.</param>
        /// <returns>
        /// 200 => App settingsd returned.
        /// 404 => App not found.
        /// </returns>
        [HttpGet]
        [Route("apps/{app}/settings")]
        [ProducesResponseType(typeof(AppSettingsDto), StatusCodes.Status200OK)]
        [ApiPermissionOrAnonymous]
        [ApiCosts(0)]
        public IActionResult GetSettings(string app)
        {
            var response = Deferred.Response(() =>
            {
                return GetResponse(App);
            });

            return Ok(response);
        }

        /// <summary>
        /// Update the app settings.
        /// </summary>
        /// <param name="app">The name of the app to update.</param>
        /// <param name="request">The values to update.</param>
        /// <returns>
        /// 200 => App updated.
        /// 400 => App request not valid.
        /// 404 => App not found.
        /// </returns>
        [HttpPut]
        [Route("apps/{app}/settings")]
        [ProducesResponseType(typeof(AppSettingsDto), StatusCodes.Status200OK)]
        [ApiPermissionOrAnonymous(Permissions.AppUpdateSettings)]
        [ApiCosts(0)]
        public async Task<IActionResult> PutSettings(string app, [FromBody] UpdateAppSettingsDto request)
        {
            var response = await InvokeCommandAsync(request.ToCommand());

            return Ok(response);
        }

        private async Task<AppSettingsDto> InvokeCommandAsync(ICommand command)
        {
            var context = await CommandBus.PublishAsync(command);

            var result = context.Result<IAppEntity>();
            var response = GetResponse(result);

            return response;
        }

        private AppSettingsDto GetResponse(IAppEntity result)
        {
            return AppSettingsDto.FromApp(result, Resources);
        }
    }
}
