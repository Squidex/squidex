// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Squidex.Areas.Api.Controllers.News.Models;
using Squidex.Areas.Api.Controllers.News.Service;
using Squidex.Infrastructure.Commands;
using Squidex.Pipeline;

namespace Squidex.Areas.Api.Controllers.News
{
    /// <summary>
    /// Readonly API for news items.
    /// </summary>
    [ApiExplorerSettings(GroupName = nameof(Languages))]
    public sealed class NewsController : ApiController
    {
        private readonly FeaturesService featuresService = new FeaturesService();

        public NewsController(ICommandBus commandBus)
            : base(commandBus)
        {
        }

        /// <summary>
        /// Get all features since latest version.
        /// </summary>
        /// <param name="version">The latest received version.</param>
        /// <returns>
        /// 200 => Latest features returned.
        /// </returns>
        [HttpGet]
        [Route("news/features/")]
        [ProducesResponseType(typeof(FeaturesDto), 200)]
        [ApiPermission]
        public async Task<IActionResult> GetLanguages([FromQuery] int version = 0)
        {
            var features = await featuresService.GetFeaturesAsync(version);

            return Ok(features);
        }
    }
}
