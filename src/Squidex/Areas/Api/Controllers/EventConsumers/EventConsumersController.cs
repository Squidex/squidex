// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Threading.Tasks;
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
        [ProducesResponseType(typeof(EventConsumersDto), 200)]
        [ApiPermission(Permissions.AdminEventsRead)]
        public async Task<IActionResult> GetEventConsumers()
        {
            var eventConsumers = await GetGrain().GetConsumersAsync();

            var response = EventConsumersDto.FromResults(eventConsumers.Value, this);

            return Ok(response);
        }

        [HttpPut]
        [Route("event-consumers/{name}/start/")]
        [ProducesResponseType(typeof(EventConsumerDto), 200)]
        [ApiPermission(Permissions.AdminEventsManage)]
        public async Task<IActionResult> StartEventConsumer(string name)
        {
            var eventConsumer = await GetGrain().StartAsync(name);

            var response = EventConsumerDto.FromEventConsumerInfo(eventConsumer.Value, this);

            return Ok(response);
        }

        [HttpPut]
        [Route("event-consumers/{name}/stop/")]
        [ProducesResponseType(typeof(EventConsumerDto), 200)]
        [ApiPermission(Permissions.AdminEventsManage)]
        public async Task<IActionResult> StopEventConsumer(string name)
        {
            var eventConsumer = await GetGrain().StopAsync(name);

            var response = EventConsumerDto.FromEventConsumerInfo(eventConsumer.Value, this);

            return Ok(response);
        }

        [HttpPut]
        [Route("event-consumers/{name}/reset/")]
        [ProducesResponseType(typeof(EventConsumerDto), 200)]
        [ApiPermission(Permissions.AdminEventsManage)]
        public async Task<IActionResult> ResetEventConsumer(string name)
        {
            var eventConsumer = await GetGrain().ResetAsync(name);

            var response = EventConsumerDto.FromEventConsumerInfo(eventConsumer.Value, this);

            return Ok(response);
        }

        private IEventConsumerManagerGrain GetGrain()
        {
            return grainFactory.GetGrain<IEventConsumerManagerGrain>(SingleGrain.Id);
        }
    }
}
