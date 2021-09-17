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
    public sealed class AppAssetsController : ApiController
    {
        public AppAssetsController(ICommandBus commandBus)
            : base(commandBus)
        {
        }

        /// <summary>
        /// Get the app asset scripts.
        /// </summary>
        /// <param name="app">The name of the app to get the asset scripts for.</param>
        /// <returns>
        /// 200 => App asset scripts returned.
        /// 404 => App not found.
        /// </returns>
        [HttpGet]
        [Route("apps/{app}/assets/scripts")]
        [ProducesResponseType(typeof(AssetScriptsDto), StatusCodes.Status200OK)]
        [ApiPermissionOrAnonymous(Permissions.AppAssetSScriptsRead)]
        [ApiCosts(0)]
        public IActionResult GetScripts(string app)
        {
            var response = Deferred.Response(() =>
            {
                return GetResponse(App);
            });

            return Ok(response);
        }

        /// <summary>
        /// Update the app asset scripts.
        /// </summary>
        /// <param name="app">The name of the app to update.</param>
        /// <param name="request">The values to update.</param>
        /// <returns>
        /// 200 => App updated.
        /// 400 => App request not valid.
        /// 404 => App not found.
        /// </returns>
        [HttpPut]
        [Route("apps/{app}/assets/scripts")]
        [ProducesResponseType(typeof(AssetScriptsDto), StatusCodes.Status200OK)]
        [ApiPermissionOrAnonymous(Permissions.AppAssetsScriptsUpdate)]
        [ApiCosts(0)]
        public async Task<IActionResult> PutScripts(string app, [FromBody] UpdateAssetScriptsDto request)
        {
            var response = await InvokeCommandAsync(request.ToCommand());

            return Ok(response);
        }

        private async Task<AssetScriptsDto> InvokeCommandAsync(ICommand command)
        {
            var context = await CommandBus.PublishAsync(command);

            var result = context.Result<IAppEntity>();
            var response = GetResponse(result);

            return response;
        }

        private AssetScriptsDto GetResponse(IAppEntity result)
        {
            return AssetScriptsDto.FromApp(result, Resources);
        }
    }
}
