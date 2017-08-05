// ==========================================================================
//  HistoryController.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using NSwag.Annotations;
using Squidex.Controllers.Api.History.Models;
using Squidex.Domain.Apps.Read.History.Repositories;
using Squidex.Infrastructure.CQRS.Commands;
using Squidex.Infrastructure.Reflection;
using Squidex.Pipeline;

namespace Squidex.Controllers.Api.History
{
    /// <summary>
    /// Readonly API to get an event stream.
    /// </summary>
    [MustBeAppEditor]
    [ApiExceptionFilter]
    [AppApi]
    [SwaggerTag("History")]
    public class HistoryController : ControllerBase
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

            var response = entities.Select(x => SimpleMapper.Map(x, new HistoryEventDto())).ToList();

            return Ok(response);
        }
    }
}
