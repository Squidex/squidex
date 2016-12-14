// ==========================================================================
//  HistoryController.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NSwag.Annotations;
using Squidex.Controllers.Api.History.Models;
using Squidex.Infrastructure.CQRS.Commands;
using Squidex.Infrastructure.Reflection;
using Squidex.Pipeline;
using Squidex.Read.Apps.Services;
using Squidex.Read.History.Repositories;

namespace Squidex.Controllers.Api.History
{
    /// <summary>
    /// Readonly API to get an event stream.
    /// </summary>
    [Authorize]
    [ApiExceptionFilter]
    [ServiceFilter(typeof(AppFilterAttribute))]
    [SwaggerTag("History")]
    public class HistoryController : ControllerBase
    {
        private readonly IAppProvider appProvider;
        private readonly IHistoryEventRepository historyEventRepository;

        public HistoryController(ICommandBus commandBus, IAppProvider appProvider, IHistoryEventRepository historyEventRepository) 
            : base(commandBus)
        {
            this.appProvider = appProvider;

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
        public async Task<IActionResult> GetHistory(string app, string channel)
        {
            var entity = await appProvider.FindAppByNameAsync(app);

            if (entity == null)
            {
                return NotFound();
            }

            var schemas = await historyEventRepository.QueryEventsByChannel(entity.Id, channel, 100);

            var response = schemas.Select(x => SimpleMapper.Map(x, new HistoryEventDto())).ToList();

            return Ok(response);
        }
    }
}
