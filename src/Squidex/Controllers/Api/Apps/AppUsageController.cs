// ==========================================================================
//  AppUsageController.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NSwag.Annotations;
using Squidex.Controllers.Api.Apps.Models;
using Squidex.Core.Identity;
using Squidex.Infrastructure.CQRS.Commands;
using Squidex.Infrastructure.UsageTracking;
using Squidex.Pipeline;

namespace Squidex.Controllers.Api.Apps
{
    /// <summary>
    /// Retrieves usage information for apps.
    /// </summary>
    [ApiExceptionFilter]
    [ServiceFilter(typeof(AppFilterAttribute))]
    [SwaggerTag("Apps")]
    public class AppUsageController : ControllerBase
    {
        private readonly IUsageTracker usageTracker;

        public AppUsageController(ICommandBus commandBus, IUsageTracker usageTracker)
            : base(commandBus)
        {
            this.usageTracker = usageTracker;
        }

        /// <summary>
        /// Get app usages.
        /// </summary>
        /// <param name="app">The name of the app.</param>
        /// <param name="fromDate">The from date.</param>
        /// <param name="toDate">The to date.</param>
        /// <returns>
        /// 200 => Usage tracking results returned.
        /// 404 => App not found.
        /// 400 => Range between from date and to date is not valid or has more than 100 days.
        /// </returns>
        [Authorize(Roles = SquidexRoles.AppEditor)]
        [HttpGet]
        [Route("apps/{app}/usages/{fromDate}/{toDate}")]
        [ProducesResponseType(typeof(UsageDto[]), 200)]
        public async Task<IActionResult> GetUsages(string app, DateTime fromDate, DateTime toDate)
        {
            if (fromDate > toDate && (toDate - fromDate).TotalDays > 100)
            {
                return BadRequest();
            }

            var entities = await usageTracker.FindAsync(App.Id.ToString(), fromDate.Date, toDate.Date);

            var models = entities.Select(x =>
            {
                var averageMs = x.TotalCount == 0 ? 0 : x.TotalElapsedMs / x.TotalCount;

                return new UsageDto { Date = x.Date, Count = x.TotalCount, AverageMs = averageMs };
            }).ToList();

            return Ok(models);
        }
    }
}
