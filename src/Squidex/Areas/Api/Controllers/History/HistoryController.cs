// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Squidex.Areas.Api.Controllers.History.Models;
using Squidex.Domain.Apps.Entities.History;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Commands;
using Squidex.Shared;
using Squidex.Web;

namespace Squidex.Areas.Api.Controllers.History
{
    /// <summary>
    /// Readonly API to get an event stream.
    /// </summary>
    [ApiExplorerSettings(GroupName = nameof(History))]
    public sealed class HistoryController : ApiController
    {
        private readonly IHistoryService historyService;

        public HistoryController(ICommandBus commandBus, IHistoryService historyService)
            : base(commandBus)
        {
            this.historyService = historyService;
        }

        /// <summary>
        /// Get the events from the history.
        /// </summary>
        /// <param name="app">The name of the app.</param>
        /// <param name="channel">The name of the channel.</param>
        /// <returns>
        /// 200 => Events returned.
        /// 404 => App not found.
        /// </returns>
        [HttpGet]
        [Route("apps/{app}/history/")]
        [ProducesResponseType(typeof(HistoryEventDto), 200)]
        [ApiPermission(Permissions.AppCommon)]
        [ApiCosts(0.1)]
        public async Task<IActionResult> GetHistory(string app, string channel)
        {
            var events = await historyService.QueryByChannelAsync(AppId, channel, 100);

            var response = events.Select(HistoryEventDto.FromHistoryEvent).Where(x => x.Message != null).ToArray();

            return Ok(response);
        }
    }
}
