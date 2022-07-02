// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Microsoft.AspNetCore.Mvc;
using Orleans;
using Squidex.Areas.Api.Controllers.EventConsumers.Models;
using Squidex.Infrastructure.Commands;
using Squidex.Infrastructure.EventSourcing.Grains;
using Squidex.Shared;
using Squidex.Web;

namespace Squidex.Areas.Api.Controllers.EventConsumers
{
    public sealed class EventConsumersController : ApiController
    {
        private readonly IEventConsumerManager eventConsumerManager;

        public EventConsumersController(ICommandBus commandBus, IEventConsumerManager eventConsumerManager)
            : base(commandBus)
        {
            this.eventConsumerManager = eventConsumerManager;
        }

        [HttpGet]
        [Route("event-consumers/")]
        [ProducesResponseType(typeof(EventConsumersDto), StatusCodes.Status200OK)]
        [ApiPermission(Permissions.AdminEventsRead)]
        public async Task<IActionResult> GetEventConsumers()
        {
            var eventConsumers = await eventConsumerManager.GetConsumersAsync(HttpContext.RequestAborted);

            var response = EventConsumersDto.FromDomain(eventConsumers, Resources);

            return Ok(response);
        }

        [HttpPut]
        [Route("event-consumers/{consumerName}/start/")]
        [ApiPermission(Permissions.AdminEventsManage)]
        public async Task StartEventConsumer(string consumerName)
        {
            await eventConsumerManager.StartAsync(consumerName);
        }

        [HttpPut]
        [Route("event-consumers/{consumerName}/stop/")]
        [ApiPermission(Permissions.AdminEventsManage)]
        public async Task StopEventConsumer(string consumerName)
        {
            await eventConsumerManager.StopAsync(consumerName);
        }

        [HttpPut]
        [Route("event-consumers/{consumerName}/reset/")]
        [ProducesResponseType(typeof(EventConsumerDto), StatusCodes.Status200OK)]
        [ApiPermission(Permissions.AdminEventsManage)]
        public async Task ResetEventConsumer(string consumerName)
        {
            await eventConsumerManager.ResetAsync(consumerName);
        }
    }
}
