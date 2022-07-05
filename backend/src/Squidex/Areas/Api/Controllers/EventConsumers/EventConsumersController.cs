// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Microsoft.AspNetCore.Mvc;
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
        [ProducesResponseType(typeof(EventConsumerDto), StatusCodes.Status200OK)]
        [ApiPermission(Permissions.AdminEventsManage)]
        public async Task<IActionResult> StartEventConsumer(string consumerName)
        {
            var eventConsumer = await eventConsumerManager.StartAsync(consumerName, HttpContext.RequestAborted);

            var response = EventConsumerDto.FromDomain(eventConsumer, Resources);

            return Ok(response);
        }

        [HttpPut]
        [Route("event-consumers/{consumerName}/stop/")]
        [ProducesResponseType(typeof(EventConsumerDto), StatusCodes.Status200OK)]
        [ApiPermission(Permissions.AdminEventsManage)]
        public async Task<IActionResult> StopEventConsumer(string consumerName)
        {
            var eventConsumer = await eventConsumerManager.StopAsync(consumerName, HttpContext.RequestAborted);

            var response = EventConsumerDto.FromDomain(eventConsumer, Resources);

            return Ok(response);
        }

        [HttpPut]
        [Route("event-consumers/{consumerName}/reset/")]
        [ProducesResponseType(typeof(EventConsumerDto), StatusCodes.Status200OK)]
        [ApiPermission(Permissions.AdminEventsManage)]
        public async Task<IActionResult> ResetEventConsumer(string consumerName)
        {
            var eventConsumer = await eventConsumerManager.ResetAsync(consumerName, HttpContext.RequestAborted);

            var response = EventConsumerDto.FromDomain(eventConsumer, Resources);

            return Ok(response);
        }
    }
}
