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
using Orleans;
using Squidex.Areas.Api.Controllers.EventConsumers.Models;
using Squidex.Infrastructure.Commands;
using Squidex.Infrastructure.EventSourcing.Grains;
using Squidex.Pipeline;

namespace Squidex.Areas.Api.Controllers.EventConsumers
{
    [ApiAuthorize]
    [ApiExceptionFilter]
    [MustBeAdministrator]
    [SwaggerIgnore]
    public sealed class EventConsumersController : ApiController
    {
        private readonly IEventConsumerManagerGrain eventConsumerManagerGrain;

        public EventConsumersController(ICommandBus commandBus, IGrainFactory grainFactory)
            : base(commandBus)
        {
            eventConsumerManagerGrain = grainFactory.GetGrain<IEventConsumerManagerGrain>("Default");
        }

        [HttpGet]
        [Route("event-consumers/")]
        [ApiCosts(0)]
        public async Task<IActionResult> GetEventConsumers()
        {
            var entities = await eventConsumerManagerGrain.GetConsumersAsync();

            var response = entities.Value.Select(EventConsumerDto.FromEventConsumerInfo).ToList();

            return Ok(response);
        }

        [HttpPut]
        [Route("event-consumers/{name}/start/")]
        [ApiCosts(0)]
        public async Task<IActionResult> Start(string name)
        {
            await eventConsumerManagerGrain.StartAsync(name);

            return NoContent();
        }

        [HttpPut]
        [Route("event-consumers/{name}/stop/")]
        [ApiCosts(0)]
        public async Task<IActionResult> Stop(string name)
        {
            await eventConsumerManagerGrain.StopAsync(name);

            return NoContent();
        }

        [HttpPut]
        [Route("event-consumers/{name}/reset/")]
        [ApiCosts(0)]
        public async Task<IActionResult> Reset(string name)
        {
            await eventConsumerManagerGrain.ResetAsync(name);

            return NoContent();
        }
    }
}
