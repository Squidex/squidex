﻿// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Squidex.Areas.Api.Controllers.Statistics.Models;
using Squidex.Domain.Apps.Entities.Apps;
using Squidex.Domain.Apps.Entities.Apps.Services;
using Squidex.Domain.Apps.Entities.Assets;
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
        private readonly IUsageTracker usageTracker;
        private readonly IAppLogStore appLogStore;
        private readonly IAppPlansProvider appPlansProvider;
        private readonly IAssetUsageTracker assetStatsRepository;
        private readonly IDataProtector dataProtector;
        private readonly UrlsOptions urlsOptions;

        public UsagesController(
            ICommandBus commandBus,
            IUsageTracker usageTracker,
            IAppLogStore appLogStore,
            IAppPlansProvider appPlansProvider,
            IAssetUsageTracker assetStatsRepository,
            IDataProtectionProvider dataProtection,
            IOptions<UrlsOptions> urlsOptions)
            : base(commandBus)
        {
            this.usageTracker = usageTracker;

            this.appLogStore = appLogStore;
            this.appPlansProvider = appPlansProvider;
            this.assetStatsRepository = assetStatsRepository;
            this.urlsOptions = urlsOptions.Value;

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
        [ProducesResponseType(typeof(LogDownloadDto), 200)]
        [ApiPermission(Permissions.AppCommon)]
        [ApiCosts(0)]
        public IActionResult GetLog(string app)
        {
            var token = dataProtector.Protect(App.Id.ToString());

            var url = urlsOptions.BuildUrl($"/api/apps/log/{token}/");

            var response = new LogDownloadDto { DownloadUrl = url };

            return Ok(response);
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
        [Route("apps/{app}/usages/calls/month/")]
        [ProducesResponseType(typeof(CurrentCallsDto), 200)]
        [ApiPermission(Permissions.AppCommon)]
        [ApiCosts(0)]
        public async Task<IActionResult> GetMonthlyCalls(string app)
        {
            var count = await usageTracker.GetMonthlyCallsAsync(AppId.ToString(), DateTime.Today);

            var plan = appPlansProvider.GetPlanForApp(App);

            var response = new CurrentCallsDto { Count = count, MaxAllowed = plan.MaxApiCalls };

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
        [ProducesResponseType(typeof(Dictionary<string, CallsUsageDto[]>), 200)]
        [ApiPermission(Permissions.AppCommon)]
        [ApiCosts(0)]
        public async Task<IActionResult> GetUsages(string app, DateTime fromDate, DateTime toDate)
        {
            if (fromDate > toDate && (toDate - fromDate).TotalDays > 100)
            {
                return BadRequest();
            }

            var usages = await usageTracker.QueryAsync(AppId.ToString(), fromDate.Date, toDate.Date);

            var response = usages.ToDictionary(x => x.Key, x => x.Value.Select(CallsUsageDto.FromUsage).ToArray());

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
        [ProducesResponseType(typeof(CurrentStorageDto), 200)]
        [ApiPermission(Permissions.AppCommon)]
        [ApiCosts(0)]
        public async Task<IActionResult> GetCurrentStorageSize(string app)
        {
            var size = await assetStatsRepository.GetTotalSizeAsync(AppId);

            var plan = appPlansProvider.GetPlanForApp(App);

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
        [ProducesResponseType(typeof(StorageUsageDto[]), 200)]
        [ApiPermission(Permissions.AppCommon)]
        [ApiCosts(0)]
        public async Task<IActionResult> GetStorageSizes(string app, DateTime fromDate, DateTime toDate)
        {
            if (fromDate > toDate && (toDate - fromDate).TotalDays > 100)
            {
                return BadRequest();
            }

            var usages = await assetStatsRepository.QueryAsync(AppId, fromDate.Date, toDate.Date);

            var models = usages.Select(StorageUsageDto.FromStats).ToArray();

            return Ok(models);
        }

        [HttpGet]
        [Route("apps/log/{token}/")]
        [ApiExplorerSettings(IgnoreApi = true)]
        public IActionResult GetLogFile(string token)
        {
            var appId = dataProtector.Unprotect(token);

            var today = DateTime.Today;

            return new FileCallbackResult("text/csv", $"Usage-{today:yyy-MM-dd}.csv", false, stream =>
            {
                return appLogStore.ReadLogAsync(appId, today.AddDays(-30), today, stream);
            });
        }
    }
}
