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
using Squidex.Infrastructure.Orleans;
using Squidex.Shared;
using Squidex.Web;

namespace Squidex.Areas.Api.Controllers.EventConsumers
{
    public sealed class EventConsumersController : ApiController
    {
        private readonly IGrainFactory grainFactory;

        public EventConsumersController(ICommandBus commandBus, IGrainFactory grainFactory)
            : base(commandBus)
        {
            this.grainFactory = grainFactory;
        }

        [HttpGet]
        [Route("event-consumers/")]
        [ProducesResponseType(typeof(EventConsumersDto), StatusCodes.Status200OK)]
        [ApiPermission(Permissions.AdminEventsRead)]
        public async Task<IActionResult> GetEventConsumers()
        {
            var eventConsumers = await GetGrain().GetConsumersAsync();

            var response = EventConsumersDto.FromDomain(eventConsumers.Value, Resources);

            return Ok(response);
        }

        [HttpPut]
        [Route("event-consumers/{consumerName}/start/")]
        [ProducesResponseType(typeof(EventConsumerDto), StatusCodes.Status200OK)]
        [ApiPermission(Permissions.AdminEventsManage)]
        public async Task<IActionResult> StartEventConsumer(string consumerName)
        {
            var eventConsumer = await GetGrain().StartAsync(consumerName);

            var response = EventConsumerDto.FromDomain(eventConsumer, Resources);

            return Ok(response);
        }

        [HttpPut]
        [Route("event-consumers/{consumerName}/stop/")]
        [ProducesResponseType(typeof(EventConsumerDto), StatusCodes.Status200OK)]
        [ApiPermission(Permissions.AdminEventsManage)]
        public async Task<IActionResult> StopEventConsumer(string consumerName)
        {
            var eventConsumer = await GetGrain().StopAsync(consumerName);

            var response = EventConsumerDto.FromDomain(eventConsumer, Resources);

            return Ok(response);
        }

        [HttpPut]
        [Route("event-consumers/{consumerName}/reset/")]
        [ProducesResponseType(typeof(EventConsumerDto), StatusCodes.Status200OK)]
        [ApiPermission(Permissions.AdminEventsManage)]
        public async Task<IActionResult> ResetEventConsumer(string consumerName)
        {
            var eventConsumer = await GetGrain().ResetAsync(consumerName);

            var response = EventConsumerDto.FromDomain(eventConsumer, Resources);

            return Ok(response);
        }

        private IEventConsumerManagerGrain GetGrain()
        {
            return grainFactory.GetGrain<IEventConsumerManagerGrain>(SingleGrain.Id);
        }
    }
}
