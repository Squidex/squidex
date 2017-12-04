// ==========================================================================
//  AppPatternsController.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using NSwag.Annotations;
using Squidex.Areas.Api.Controllers.UI.Models;
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
    [MustBeAppOwner]
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
        [ProducesResponseType(typeof(UIRegexSuggestionDto[]), 200)]
        [ApiCosts(1)]
        public IActionResult GetPatterns(string app)
        {
            return Ok(App.Patterns.Values?
                        .Where(x =>
                            !string.IsNullOrWhiteSpace(x.Name) &&
                            !string.IsNullOrWhiteSpace(x.Pattern))
                        .Select(x => new UIRegexSuggestionDto { Name = x.Name, Pattern = x.Pattern, DefaultMessage = x.DefaultMessage })
                        .OrderBy(x => x.Name)
                        .ToList()
                    ?? new List<UIRegexSuggestionDto>());
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
        [ProducesResponseType(typeof(UIRegexSuggestionDto), 201)]
        [ApiCosts(1)]
        public async Task<IActionResult> PostPattern(string app, [FromBody] UIRegexSuggestionDto request)
        {
            var command = SimpleMapper.Map(request, new AddPattern());

            await CommandBus.PublishAsync(command);

            var response = SimpleMapper.Map(command, new UIRegexSuggestionDto());

            return CreatedAtAction(nameof(GetPatterns), new { app }, response);
        }

        /// <summary>
        /// Update an existing app patterm.
        /// </summary>
        /// <param name="app">The name of the app.</param>
        /// <param name="name">The name of the pattern to be updated.</param>
        /// <param name="request">Pattern to be updated for the app.</param>
        /// <returns>
        /// 204 => Pattern updated.
        /// 404 => App not found or pattern not found.
        /// </returns>
        [HttpPut]
        [Route("apps/{app}/patterns/{name}")]
        [ProducesResponseType(typeof(UIRegexSuggestionDto), 201)]
        [ApiCosts(1)]
        public async Task<IActionResult> UpdatePattern(string app, string name, [FromBody] UIRegexSuggestionDto request)
        {
            var command = SimpleMapper.Map(request, new UpdatePattern { OriginalName = name });

            await CommandBus.PublishAsync(command);
            return NoContent();
        }

        /// <summary>
        /// Revoke an app client
        /// </summary>
        /// <param name="app">The name of the app.</param>
        /// <param name="name">The name of the pattern to be deleted.</param>
        /// <returns>
        /// 204 => Pattern removed.
        /// 404 => App not found or pattern not found.
        /// </returns>
        /// <remarks>
        /// Schemas using this pattern will still function using the same Regular Expression
        /// </remarks>
        [HttpDelete]
        [Route("apps/{app}/patterns/{name}/")]
        [ApiCosts(1)]
        public async Task<IActionResult> DeletePattern(string app, string name)
        {
            await CommandBus.PublishAsync(new DeletePattern { Name = name });

            return NoContent();
        }
    }
}
