// ==========================================================================
//  UsagesController.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using NSwag.Annotations;
using Squidex.Controllers.Api.Statistics.Models;
using Squidex.Domain.Apps.Read.Apps.Services;
using Squidex.Domain.Apps.Read.Assets.Repositories;
using Squidex.Infrastructure.CQRS.Commands;
using Squidex.Infrastructure.UsageTracking;
using Squidex.Pipeline;

namespace Squidex.Controllers.Api.Statistics
{
    /// <summary>
    /// Retrieves usage information for apps.
    /// </summary>
    [MustBeAppEditor]
    [ApiExceptionFilter]
    [AppApi]
    [SwaggerTag("Statistics")]
    public class UsagesController : ControllerBase
    {
        private readonly IUsageTracker usageTracker;
        private readonly IAppPlansProvider appPlanProvider;
        private readonly IAssetStatsRepository assetStatsRepository;

        public UsagesController(
            ICommandBus commandBus,
            IUsageTracker usageTracker,
            IAppPlansProvider appPlanProvider,
            IAssetStatsRepository assetStatsRepository)
            : base(commandBus)
        {
            this.usageTracker = usageTracker;

            this.appPlanProvider = appPlanProvider;
            this.assetStatsRepository = assetStatsRepository;
        }

        /// <summary>
        /// Get api calls for this month.
        /// </summary>
        /// <param name="app">The name of the app.</param>
        /// <returns>
        /// 200 => Usage tracking results returned.
        /// 404 => App not found.
        /// </returns>
        [HttpGet]
        [Route("apps/{app}/usages/calls/month")]
        [ProducesResponseType(typeof(CurrentCallsDto), 200)]
        [ApiCosts(0)]
        public async Task<IActionResult> GetMonthlyCalls(string app)
        {
            var count = await usageTracker.GetMonthlyCalls(App.Id.ToString(), DateTime.Today);

            var plan = appPlanProvider.GetPlanForApp(App);

            return Ok(new CurrentCallsDto { Count = count, MaxAllowed = plan.MaxApiCalls });
        }

        /// <summary>
        /// Get api calls in date range.
        /// </summary>
        /// <param name="app">The name of the app.</param>
        /// <param name="fromDate">The from date.</param>
        /// <param name="toDate">The to date.</param>
        /// <returns>
        /// 200 => API call returned.
        /// 404 => App not found.
        /// 400 => Range between from date and to date is not valid or has more than 100 days.
        /// </returns>
        [HttpGet]
        [Route("apps/{app}/usages/calls/{fromDate}/{toDate}")]
        [ProducesResponseType(typeof(CallsUsageDto[]), 200)]
        [ApiCosts(0)]
        public async Task<IActionResult> GetUsages(string app, DateTime fromDate, DateTime toDate)
        {
            if (fromDate > toDate && (toDate - fromDate).TotalDays > 100)
            {
                return BadRequest();
            }

            var entities = await usageTracker.QueryAsync(App.Id.ToString(), fromDate.Date, toDate.Date);

            var models = entities.Select(x =>
            {
                var averageMs = x.TotalCount == 0 ? 0 : x.TotalElapsedMs / x.TotalCount;

                return new CallsUsageDto { Date = x.Date, Count = x.TotalCount, AverageMs = averageMs };
            }).ToList();

            return Ok(models);
        }

        /// <summary>
        /// Get total asset size.
        /// </summary>
        /// <param name="app">The name of the app.</param>
        /// <returns>
        /// 200 => Storage usage returned.
        /// 404 => App not found.
        /// </returns>
        [HttpGet]
        [Route("apps/{app}/usages/storage/today")]
        [ProducesResponseType(typeof(CurrentStorageDto), 200)]
        [ApiCosts(0)]
        public async Task<IActionResult> GetCurrentStorageSize(string app)
        {
            var size = await assetStatsRepository.GetTotalSizeAsync(App.Id);

            var plan = appPlanProvider.GetPlanForApp(App);

            return Ok(new CurrentStorageDto { Size = size, MaxAllowed = plan.MaxAssetSize });
        }

        /// <summary>
        /// Get storage usage in date range.
        /// </summary>
        /// <param name="app">The name of the app.</param>
        /// <param name="fromDate">The from date.</param>
        /// <param name="toDate">The to date.</param>
        /// <returns>
        /// 200 => Storage usage returned.
        /// 404 => App not found.
        /// 400 => Range between from date and to date is not valid or has more than 100 days.
        /// </returns>
        [HttpGet]
        [Route("apps/{app}/usages/storage/{fromDate}/{toDate}")]
        [ProducesResponseType(typeof(StorageUsageDto[]), 200)]
        [ApiCosts(0)]
        public async Task<IActionResult> GetStorageSizes(string app, DateTime fromDate, DateTime toDate)
        {
            if (fromDate > toDate && (toDate - fromDate).TotalDays > 100)
            {
                return BadRequest();
            }

            var entities = await assetStatsRepository.QueryAsync(App.Id, fromDate.Date, toDate.Date);

            var models = entities.Select(x => new StorageUsageDto { Date = x.Date, Count = x.TotalCount, Size = x.TotalSize }).ToList();

            return Ok(models);
        }
    }
}
