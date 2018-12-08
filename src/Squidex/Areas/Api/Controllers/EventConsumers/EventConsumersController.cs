// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Orleans;
using Squidex.Areas.Api.Controllers.EventConsumers.Models;
using Squidex.Infrastructure.Commands;
using Squidex.Infrastructure.EventSourcing.Grains;
using Squidex.Infrastructure.Orleans;
using Squidex.Pipeline;
using Squidex.Shared;

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
        [ApiPermission(Permissions.AdminEventsRead)]
        public async Task<IActionResult> GetEventConsumers()
        {
            var entities = await GetGrain().GetConsumersAsync();

            var response = entities.Value.OrderBy(x => x.Name).Select(EventConsumerDto.FromEventConsumerInfo).ToArray();

            return Ok(response);
        }

        [HttpPut]
        [Route("event-consumers/{name}/start/")]
        [ApiPermission(Permissions.AdminEventsManage)]
        public async Task<IActionResult> Start(string name)
        {
            await GetGrain().StartAsync(name);

            return NoContent();
        }

        [HttpPut]
        [Route("event-consumers/{name}/stop/")]
        [ApiPermission(Permissions.AdminEventsManage)]
        public async Task<IActionResult> Stop(string name)
        {
            await GetGrain().StopAsync(name);

            return NoContent();
        }

        [HttpPut]
        [Route("event-consumers/{name}/reset/")]
        [ApiPermission(Permissions.AdminEventsManage)]
        public async Task<IActionResult> Reset(string name)
        {
            await GetGrain().ResetAsync(name);

            return NoContent();
        }

        private IEventConsumerManagerGrain GetGrain()
        {
            return grainFactory.GetGrain<IEventConsumerManagerGrain>(SingleGrain.Id);
        }
    }
}
