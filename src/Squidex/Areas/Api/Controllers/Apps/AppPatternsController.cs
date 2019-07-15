// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Net.Http.Headers;
using Squidex.Areas.Api.Controllers.Apps.Models;
using Squidex.Domain.Apps.Entities.Apps;
using Squidex.Domain.Apps.Entities.Apps.Commands;
using Squidex.Infrastructure.Commands;
using Squidex.Shared;
using Squidex.Web;

namespace Squidex.Areas.Api.Controllers.Apps
{
    /// <summary>
    /// Manages and configures app patterns.
    /// </summary>
    [ApiExplorerSettings(GroupName = nameof(Apps))]
    public sealed class AppPatternsController : ApiController
    {
        public AppPatternsController(ICommandBus commandBus)
            : base(commandBus)
        {
        }

        /// <summary>
        /// Get app patterns.
        /// </summary>
        /// <param name="app">The name of the app.</param>
        /// <returns>
        /// 200 => Patterns returned.
        /// 404 => App not found.
        /// </returns>
        /// <remarks>
        /// Gets all configured regex patterns for the app with the specified name.
        /// </remarks>
        [HttpGet]
        [Route("apps/{app}/patterns/")]
        [ProducesResponseType(typeof(PatternsDto), 200)]
        [ApiPermission(Permissions.AppCommon)]
        [ApiCosts(0)]
        public IActionResult GetPatterns(string app)
        {
            var response = Deferred.Response(() =>
            {
                return PatternsDto.FromApp(App, this);
            });

            Response.Headers[HeaderNames.ETag] = App.ToEtag();

            return Ok(response);
        }

        /// <summary>
        /// Create a new app pattern.
        /// </summary>
        /// <param name="app">The name of the app.</param>
        /// <param name="request">Pattern to be added to the app.</param>
        /// <returns>
        /// 201 => Pattern generated.
        /// 400 => Pattern request not valid.
        /// 404 => App not found.
        /// </returns>
        [HttpPost]
        [Route("apps/{app}/patterns/")]
        [ProducesResponseType(typeof(PatternsDto), 200)]
        [ApiPermission(Permissions.AppPatternsCreate)]
        [ApiCosts(1)]
        public async Task<IActionResult> PostPattern(string app, [FromBody] UpdatePatternDto request)
        {
            var command = request.ToAddCommand();

            var response = await InvokeCommandAsync(command);

            return CreatedAtAction(nameof(GetPatterns), new { app }, response);
        }

        /// <summary>
        /// Update an existing app pattern.
        /// </summary>
        /// <param name="app">The name of the app.</param>
        /// <param name="id">The id of the pattern to be updated.</param>
        /// <param name="request">Pattern to be updated for the app.</param>
        /// <returns>
        /// 200 => Pattern updated.
        /// 400 => Pattern request not valid.
        /// 404 => Pattern or app not found.
        /// </returns>
        [HttpPut]
        [Route("apps/{app}/patterns/{id}/")]
        [ProducesResponseType(typeof(PatternsDto), 200)]
        [ApiPermission(Permissions.AppPatternsUpdate)]
        [ApiCosts(1)]
        public async Task<IActionResult> UpdatePattern(string app, Guid id, [FromBody] UpdatePatternDto request)
        {
            var command = request.ToUpdateCommand(id);

            var response = await InvokeCommandAsync(command);

            return Ok(response);
        }

        /// <summary>
        /// Delete an existing app pattern.
        /// </summary>
        /// <param name="app">The name of the app.</param>
        /// <param name="id">The id of the pattern to be deleted.</param>
        /// <returns>
        /// 200 => Pattern removed.
        /// 404 => Pattern or app not found.
        /// </returns>
        /// <remarks>
        /// Schemas using this pattern will still function using the same Regular Expression.
        /// </remarks>
        [HttpDelete]
        [Route("apps/{app}/patterns/{id}/")]
        [ProducesResponseType(typeof(PatternsDto), 200)]
        [ApiPermission(Permissions.AppPatternsDelete)]
        [ApiCosts(1)]
        public async Task<IActionResult> DeletePattern(string app, Guid id)
        {
            var command = new DeletePattern { PatternId = id };

            var response = await InvokeCommandAsync(command);

            return Ok(response);
        }

        private async Task<PatternsDto> InvokeCommandAsync(ICommand command)
        {
            var context = await CommandBus.PublishAsync(command);

            var result = context.Result<IAppEntity>();
            var response = PatternsDto.FromApp(result, this);

            return response;
        }
    }
}
