// ==========================================================================
//  UIController.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using NSwag.Annotations;
using Squidex.Areas.Api.Controllers.UI.Models;
using Squidex.Config;
using Squidex.Infrastructure.Commands;
using Squidex.Pipeline;

namespace Squidex.Areas.Api.Controllers.UI
{
    /// <summary>
    /// Manages ui settings and configs.
    /// </summary>
    [ApiExceptionFilter]
    [SwaggerTag(nameof(UI))]
    public sealed class UIController : ApiController
    {
        private readonly MyUIOptions uiOptions;

        public UIController(ICommandBus commandBus, IOptions<MyUIOptions> uiOptions)
            : base(commandBus)
        {
            this.uiOptions = uiOptions.Value;
        }

        /// <summary>
        /// Get ui settings.
        /// </summary>
        [HttpGet]
        [Route("ui/settings/")]
        [ProducesResponseType(typeof(UISettingsDto), 200)]
        [ApiCosts(0)]
        public IActionResult GetSettings()
        {
            var dto = new UISettingsDto
            {
                RegexSuggestions =
                    uiOptions.RegexSuggestions?
                        .Where(x =>
                            !string.IsNullOrWhiteSpace(x.Key) &&
                            !string.IsNullOrWhiteSpace(x.Value))
                        .Select(x => new UIRegexSuggestionDto { Name = x.Key, Pattern = x.Value }).ToList()
                    ?? new List<UIRegexSuggestionDto>(),
                MapType = uiOptions.Map?.Type ?? "OSM",
                MapKey = uiOptions.Map?.GoogleMaps?.Key
            };

            return Ok(dto);
        }
    }
}
