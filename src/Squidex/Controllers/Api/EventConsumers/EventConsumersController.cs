// ==========================================================================
//  EventConsumersController.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using NSwag.Annotations;
using Squidex.Controllers.Api.EventConsumers.Models;
using Squidex.Infrastructure.Actors;
using Squidex.Infrastructure.CQRS.Events;
using Squidex.Infrastructure.CQRS.Events.Actors.Messages;
using Squidex.Infrastructure.Reflection;
using Squidex.Pipeline;

namespace Squidex.Controllers.Api.EventConsumers
{
    [MustBeAdministrator]
    [ApiExceptionFilter]
    [SwaggerIgnore]
    public sealed class EventConsumersController : Controller
    {
        private readonly IEventConsumerInfoRepository eventConsumerRepository;
        private readonly IActors actors;

        public EventConsumersController(IEventConsumerInfoRepository eventConsumerRepository, IActors actors)
        {
            this.eventConsumerRepository = eventConsumerRepository;

            this.actors = actors;
        }

        [HttpGet]
        [Route("event-consumers/")]
        [ApiCosts(0)]
        public async Task<IActionResult> GetEventConsumers()
        {
            var entities = await eventConsumerRepository.QueryAsync();

            var models = entities.Select(x => SimpleMapper.Map(x, new EventConsumerDto())).ToList();

            return Ok(models);
        }

        [HttpPut]
        [Route("event-consumers/{name}/start")]
        [ApiCosts(0)]
        public IActionResult Start(string name)
        {
            var actor = actors.Get(name);

            actor?.Tell(new StartConsumerMessage());

            return NoContent();
        }

        [HttpPut]
        [Route("event-consumers/{name}/stop")]
        [ApiCosts(0)]
        public IActionResult Stop(string name)
        {
            var actor = actors.Get(name);

            actor?.Tell(new StopConsumerMessage());

            return NoContent();
        }

        [HttpPut]
        [Route("event-consumers/{name}/reset")]
        [ApiCosts(0)]
        public IActionResult Reset(string name)
        {
            var actor = actors.Get(name);

            actor?.Tell(new ResetConsumerMessage());

            return NoContent();
        }
    }
}
