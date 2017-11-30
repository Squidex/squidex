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
using Squidex.Domain.Apps.Core.Apps;
using Squidex.Infrastructure.CQRS.Commands;
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
        private readonly List<AppPattern> patterns;

        public UIController(ICommandBus commandBus, IOptions<List<AppPattern>> patterns)
            : base(commandBus)
        {
            this.patterns = patterns.Value;
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
                    patterns?
                        .Where(x =>
                            !string.IsNullOrWhiteSpace(x.Name) &&
                            !string.IsNullOrWhiteSpace(x.Pattern))
                        .Select(x => new UIRegexSuggestionDto { Name = x.Name, Pattern = x.Pattern, DefaultMessage = x.DefaultMessage }).ToList()
                    ?? new List<UIRegexSuggestionDto>()
            };

            return Ok(dto);
        }
    }
}
