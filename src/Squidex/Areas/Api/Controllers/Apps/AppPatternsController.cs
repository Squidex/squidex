// ==========================================================================
//  AppPatternsController.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using NSwag.Annotations;
using Squidex.Areas.Api.Controllers.Apps.Models;
using Squidex.Config;
using Squidex.Domain.Apps.Write.Apps.Commands;
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
        private readonly List<AppPatternDto> defaultPatterns;

        public AppPatternsController(ICommandBus commandBus, IOptions<MyUIOptions> uiOptions)
            : base(commandBus)
        {
            this.defaultPatterns = uiOptions.Value.RegexSuggestions;
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
        [ApiCosts(1)]
        public async Task<IActionResult> GetPatterns(string app)
        {
            List<AppPatternDto> patterns;
            if (App.Patterns.Values.Count() == 0)
            {
                await CreateIntialPatterns();
                patterns = defaultPatterns;
            }
            else
            {
                patterns = App.Patterns.Values?.Select(x =>
                    new AppPatternDto
                    {
                        Id = x.Id,
                        Name = x.Name,
                        Pattern = x.Pattern,
                        DefaultMessage = x.DefaultMessage
                    }).ToList();
            }

            var orderedPatterns = patterns.OrderBy(x => x.Name);
            return Ok(orderedPatterns);
        }

        private async Task CreateIntialPatterns()
        {
            foreach (var pattern in defaultPatterns)
            {
                var command = SimpleMapper.Map(pattern, new AddPattern());
                await CommandBus.PublishAsync(command);
            }
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
        public async Task<IActionResult> PostPattern(string app, [FromBody] AppPatternDto request)
        {
            var command = SimpleMapper.Map(request, new AddPattern());

            await CommandBus.PublishAsync(command);

            return CreatedAtAction(nameof(GetPatterns), new { app }, request);
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
        [Route("apps/{app}/patterns/{name}")]
        [ProducesResponseType(typeof(AppPatternDto), 201)]
        [ApiCosts(1)]
        public async Task<IActionResult> UpdatePattern(string app, Guid id, [FromBody] AppPatternDto request)
        {
            var command = SimpleMapper.Map(request, new UpdatePattern { Id = id });

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
        /// 404 => App not found or pattern not found.
        /// </returns>
        /// <remarks>
        /// Schemas using this pattern will still function using the same Regular Expression
        /// </remarks>
        [HttpDelete]
        [Route("apps/{app}/patterns/{id}/")]
        [ApiCosts(1)]
        public async Task<IActionResult> DeletePattern(string app, Guid id)
        {
            await CommandBus.PublishAsync(new DeletePattern { Id = id });

            return NoContent();
        }
    }
}
