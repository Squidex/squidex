// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using NSwag.Annotations;
using Squidex.Areas.Api.Controllers.History.Models;
using Squidex.Domain.Apps.Entities.History.Repositories;
using Squidex.Infrastructure.Commands;
using Squidex.Pipeline;

namespace Squidex.Areas.Api.Controllers.History
{
    /// <summary>
    /// Readonly API to get an event stream.
    /// </summary>
    [ApiAuthorize]
    [ApiExceptionFilter]
    [AppApi]
    [MustBeAppEditor]
    [SwaggerTag(nameof(History))]
    public sealed class HistoryController : ApiController
    {
        private readonly IHistoryEventRepository historyEventRepository;

        public HistoryController(ICommandBus commandBus, IHistoryEventRepository historyEventRepository)
            : base(commandBus)
        {
            this.historyEventRepository = historyEventRepository;
        }

        /// <summary>
        /// Get the events from the history
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
        [ApiCosts(0.1)]
        public async Task<IActionResult> GetHistory(string app, string channel)
        {
            var entities = await historyEventRepository.QueryByChannelAsync(App.Id, channel, 100);

            var response = entities.Select(HistoryEventDto.FromHistoryEvent).ToList();

            return Ok(response);
        }
    }
}
