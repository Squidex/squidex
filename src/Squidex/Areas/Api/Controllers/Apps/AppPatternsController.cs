﻿// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using NSwag.Annotations;
using Squidex.Areas.Api.Controllers.Apps.Models;
using Squidex.Domain.Apps.Entities.Apps.Commands;
using Squidex.Infrastructure.Commands;
using Squidex.Infrastructure.Reflection;
using Squidex.Pipeline;

namespace Squidex.Areas.Api.Controllers.Apps
{
    /// <summary>
    /// Manages and configures app patterns.
    /// </summary>
    [ApiAuthorize]
    [MustBeAppDeveloper]
    [ApiExceptionFilter]
    [AppApi]
    [SwaggerTag(nameof(Apps))]
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
        [ApiCosts(0)]
        public IActionResult GetPatterns(string app)
        {
            var response =
                App.Patterns.Select(x => SimpleMapper.Map(x.Value, new AppPatternDto { PatternId = x.Key }))
                    .OrderBy(x => x.Name).ToList();

            return Ok(response);
        }

        /// <summary>
        /// Create a new app patterm.
        /// </summary>
        /// <param name="app">The name of the app.</param>
        /// <param name="request">Pattern to be added to the app.</param>
        /// <returns>
        /// 201 => Pattern generated.
        /// 404 => App not found.
        /// </returns>
        [HttpPost]
        [Route("apps/{app}/patterns/")]
        [ProducesResponseType(typeof(AppPatternDto), 201)]
        [ApiCosts(1)]
        public async Task<IActionResult> PostPattern(string app, [FromBody] UpdatePatternDto request)
        {
            var command = SimpleMapper.Map(request, new AddPattern());

            await CommandBus.PublishAsync(command);

            var response = SimpleMapper.Map(request, new AppPatternDto { PatternId = command.PatternId });

            return CreatedAtAction(nameof(GetPatterns), new { app }, response);
        }

        /// <summary>
        /// Update an existing app patterm.
        /// </summary>
        /// <param name="app">The name of the app.</param>
        /// <param name="id">The id of the pattern to be updated.</param>
        /// <param name="request">Pattern to be updated for the app.</param>
        /// <returns>
        /// 204 => Pattern updated.
        /// 404 => App not found or pattern not found.
        /// </returns>
        [HttpPut]
        [Route("apps/{app}/patterns/{id}/")]
        [ProducesResponseType(typeof(AppPatternDto), 201)]
        [ApiCosts(1)]
        public async Task<IActionResult> UpdatePattern(string app, Guid id, [FromBody] UpdatePatternDto request)
        {
            var command = SimpleMapper.Map(request, new UpdatePattern { PatternId = id });

            await CommandBus.PublishAsync(command);

            return NoContent();
        }

        /// <summary>
        /// Revoke an app client
        /// </summary>
        /// <param name="app">The name of the app.</param>
        /// <param name="id">The id of the pattern to be deleted.</param>
        /// <returns>
        /// 204 => Pattern removed.
        /// 404 => App or pattern not found.
        /// </returns>
        /// <remarks>
        /// Schemas using this pattern will still function using the same Regular Expression
        /// </remarks>
        [HttpDelete]
        [Route("apps/{app}/patterns/{id}/")]
        [ApiCosts(1)]
        public async Task<IActionResult> DeletePattern(string app, Guid id)
        {
            await CommandBus.PublishAsync(new DeletePattern { PatternId = id });

            return NoContent();
        }
    }
}
