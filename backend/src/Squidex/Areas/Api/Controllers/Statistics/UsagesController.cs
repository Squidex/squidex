// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Squidex.Areas.Api.Controllers.Statistics.Models;
using Squidex.Domain.Apps.Entities.Apps;
using Squidex.Domain.Apps.Entities.Apps.Plans;
using Squidex.Domain.Apps.Entities.Assets;
using Squidex.Hosting;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Commands;
using Squidex.Infrastructure.UsageTracking;
using Squidex.Shared;
using Squidex.Web;

namespace Squidex.Areas.Api.Controllers.Statistics
{
    /// <summary>
    /// Retrieves usage information for apps.
    /// </summary>
    [ApiExplorerSettings(GroupName = nameof(Statistics))]
    public sealed class UsagesController : ApiController
    {
        private readonly IApiUsageTracker usageTracker;
        private readonly IAppLogStore appLogStore;
        private readonly IAppPlansProvider appPlansProvider;
        private readonly IAssetUsageTracker assetStatsRepository;
        private readonly IDataProtector dataProtector;
        private readonly IUrlGenerator urlGenerator;

        public UsagesController(
            ICommandBus commandBus,
            IDataProtectionProvider dataProtection,
            IApiUsageTracker usageTracker,
            IAppLogStore appLogStore,
            IAppPlansProvider appPlansProvider,
            IAssetUsageTracker assetStatsRepository,
            IUrlGenerator urlGenerator)
            : base(commandBus)
        {
            this.usageTracker = usageTracker;

            this.appLogStore = appLogStore;
            this.appPlansProvider = appPlansProvider;
            this.assetStatsRepository = assetStatsRepository;
            this.urlGenerator = urlGenerator;

            dataProtector = dataProtection.CreateProtector("LogToken");
        }

        /// <summary>
        /// Get api calls as log file.
        /// </summary>
        /// <param name="app">The name of the app.</param>
        /// <returns>
        /// 200 => Usage tracking results returned.
        /// 404 => App not found.
        /// </returns>
        [HttpGet]
        [Route("apps/{app}/usages/log/")]
        [ProducesResponseType(typeof(LogDownloadDto), StatusCodes.Status200OK)]
        [ApiPermissionOrAnonymous(Permissions.AppUsage)]
        [ApiCosts(0)]
        public IActionResult GetLog(string app)
        {
            var token = dataProtector.Protect(App.Id.ToString());

            var url = urlGenerator.BuildUrl($"/api/apps/log/{token}/");

            var response = new LogDownloadDto { DownloadUrl = url };

            return Ok(response);
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
        [Route("apps/{app}/usages/calls/{fromDate}/{toDate}/")]
        [ProducesResponseType(typeof(CallsUsageDtoDto), StatusCodes.Status200OK)]
        [ApiPermissionOrAnonymous(Permissions.AppUsage)]
        [ApiCosts(0)]
        public async Task<IActionResult> GetUsages(string app, DateTime fromDate, DateTime toDate)
        {
            if (fromDate > toDate && (toDate - fromDate).TotalDays > 100)
            {
                return BadRequest();
            }

            var (summary, details) = await usageTracker.QueryAsync(AppId.ToString(), fromDate.Date, toDate.Date, HttpContext.RequestAborted);

            var (plan, _) = appPlansProvider.GetPlanForApp(App);

            var response = CallsUsageDtoDto.FromStats(plan, summary, details);

            return Ok(response);
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
        [Route("apps/{app}/usages/storage/today/")]
        [ProducesResponseType(typeof(CurrentStorageDto), StatusCodes.Status200OK)]
        [ApiPermissionOrAnonymous(Permissions.AppUsage)]
        [ApiCosts(0)]
        public async Task<IActionResult> GetCurrentStorageSize(string app)
        {
            var size = await assetStatsRepository.GetTotalSizeAsync(AppId);

            var (plan, _) = appPlansProvider.GetPlanForApp(App);

            var response = new CurrentStorageDto { Size = size, MaxAllowed = plan.MaxAssetSize };

            return Ok(response);
        }

        /// <summary>
        /// Get asset usage by date.
        /// </summary>
        /// <param name="app">The name of the app.</param>
        /// <param name="fromDate">The from date.</param>
        /// <param name="toDate">The to date.</param>
        /// <returns>
        /// 200 => Storage usage returned.
        /// 400 => Range between from date and to date is not valid or has more than 100 days.
        /// 404 => App not found.
        /// </returns>
        [HttpGet]
        [Route("apps/{app}/usages/storage/{fromDate}/{toDate}/")]
        [ProducesResponseType(typeof(StorageUsagePerDateDto[]), StatusCodes.Status200OK)]
        [ApiPermissionOrAnonymous(Permissions.AppUsage)]
        [ApiCosts(0)]
        public async Task<IActionResult> GetStorageSizes(string app, DateTime fromDate, DateTime toDate)
        {
            if (fromDate > toDate && (toDate - fromDate).TotalDays > 100)
            {
                return BadRequest();
            }

            var usages = await assetStatsRepository.QueryAsync(AppId, fromDate.Date, toDate.Date);

            var models = usages.Select(StorageUsagePerDateDto.FromStats).ToArray();

            return Ok(models);
        }

        [HttpGet]
        [Route("apps/log/{token}/")]
        [ApiExplorerSettings(IgnoreApi = true)]
        public IActionResult GetLogFile(string token)
        {
            var appId = DomainId.Create(dataProtector.Unprotect(token));

            var today = DateTime.UtcNow.Date;

            var fileName = $"Usage-{today:yyy-MM-dd}.csv";

            var callback = new FileCallback((body, range, ct) =>
            {
                return appLogStore.ReadLogAsync(appId, today.AddDays(-30), today, body, ct);
            });

            return new FileCallbackResult("text/csv", callback)
            {
                FileDownloadName = fileName
            };
        }
    }
}
