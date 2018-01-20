// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using NSwag.Annotations;
using Squidex.Areas.Api.Controllers.EventConsumers.Models;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Commands;
using Squidex.Infrastructure.EventSourcing.Grains.Messages;
using Squidex.Infrastructure.Reflection;
using Squidex.Pipeline;

namespace Squidex.Areas.Api.Controllers.EventConsumers
{
    [ApiAuthorize]
    [ApiExceptionFilter]
    [MustBeAdministrator]
    [SwaggerIgnore]
    public sealed class EventConsumersController : ApiController
    {
        private readonly IPubSub pubSub;

        public EventConsumersController(ICommandBus commandBus, IPubSub pubSub)
            : base(commandBus)
        {
            this.pubSub = pubSub;
        }

        [HttpGet]
        [Route("event-consumers/")]
        [ApiCosts(0)]
        public async Task<IActionResult> GetEventConsumers()
        {
            var entities = await pubSub.RequestAsync<GetStatesRequest, GetStatesResponse>(new GetStatesRequest(), TimeSpan.FromSeconds(2), true);

            var models = entities.States.Select(x => SimpleMapper.Map(x, new EventConsumerDto())).ToList();

            return Ok(models);
        }

        [HttpPut]
        [Route("event-consumers/{name}/start/")]
        [ApiCosts(0)]
        public IActionResult Start(string name)
        {
            pubSub.Publish(new StartConsumerMessage { ConsumerName = name }, true);

            return NoContent();
        }

        [HttpPut]
        [Route("event-consumers/{name}/stop/")]
        [ApiCosts(0)]
        public IActionResult Stop(string name)
        {
            pubSub.Publish(new StopConsumerMessage { ConsumerName = name }, true);

            return NoContent();
        }

        [HttpPut]
        [Route("event-consumers/{name}/reset/")]
        [ApiCosts(0)]
        public IActionResult Reset(string name)
        {
            pubSub.Publish(new ResetConsumerMessage { ConsumerName = name }, true);

            return NoContent();
        }
    }
}
