// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Net.Http.Headers;
using Squidex.Areas.Api.Controllers.Apps.Models;
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
        [ProducesResponseType(typeof(AppPatternDto[]), 200)]
        [ApiPermission(Permissions.AppPatternsRead)]
        [ApiCosts(0)]
        public IActionResult GetPatterns(string app)
        {
            var response = App.Patterns.Select(AppPatternDto.FromKvp).OrderBy(x => x.Name).ToArray();

            Response.Headers[HeaderNames.ETag] = App.Version.ToString();

            return Ok(response);
        }

        /// <summary>
        /// Updates all app pattern.
        /// </summary>
        /// <param name="app">The name of the app.</param>
        /// <param name="id">The id of the pattern to be updated.</param>
        /// <param name="request">Patterns to be updated for the app.</param>
        /// <returns>
        /// 204 => Patterns updated.
        /// 400 => Patterns request not valid.
        /// 404 => App not found.
        /// </returns>
        [HttpPut]
        [Route("apps/{app}/patterns/")]
        [ProducesResponseType(typeof(AppPatternDto), 201)]
        [ApiPermission(Permissions.AppPatternsUpdate)]
        [ApiCosts(1)]
        public async Task<IActionResult> UpdatePatterns(string app, Guid id, [FromBody] ConfigurePatternsDto request)
        {
            await CommandBus.PublishAsync(request.ToConfigureCommand());

            return NoContent();
        }
    }
}
