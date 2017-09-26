// ==========================================================================
//  WebhooksController.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using NodaTime;
using NSwag.Annotations;
using Squidex.Controllers.Api.Webhooks.Models;
using Squidex.Domain.Apps.Core.Webhooks;
using Squidex.Domain.Apps.Read.Webhooks.Repositories;
using Squidex.Domain.Apps.Write.Webhooks.Commands;
using Squidex.Infrastructure.CQRS.Commands;
using Squidex.Infrastructure.Reflection;
using Squidex.Pipeline;

namespace Squidex.Controllers.Api.Webhooks
{
    /// <summary>
    /// Manages and retrieves information about schemas.
    /// </summary>
    [ApiExceptionFilter]
    [AppApi]
    [SwaggerTag(nameof(Webhooks))]
    [MustBeAppDeveloper]
    public sealed class WebhooksController : ControllerBase
    {
        private readonly IWebhookRepository webhooksRepository;
        private readonly IWebhookEventRepository webhookEventsRepository;

        public WebhooksController(ICommandBus commandBus,
            IWebhookRepository webhooksRepository,
            IWebhookEventRepository webhookEventsRepository)
            : base(commandBus)
        {
            this.webhooksRepository = webhooksRepository;
            this.webhookEventsRepository = webhookEventsRepository;
        }

        /// <summary>
        /// Get webhooks.
        /// </summary>
        /// <param name="app">The name of the app.</param>
        /// <returns>
        /// 200 => Webhooks returned.
        /// 404 => App not found.
        /// </returns>
        [HttpGet]
        [Route("apps/{app}/webhooks/")]
        [ProducesResponseType(typeof(WebhookDto[]), 200)]
        [ApiCosts(1)]
        public async Task<IActionResult> GetWebhooks(string app)
        {
            var webhooks = await webhooksRepository.QueryByAppAsync(App.Id);

            var response = webhooks.Select(w =>
            {
                var totalCount = w.TotalTimedout + w.TotalSucceeded + w.TotalFailed;
                var totalAverage = totalCount == 0 ? 0 : w.TotalRequestTime / totalCount;

                var schemas = w.Schemas.Select(s => SimpleMapper.Map(s, new WebhookSchemaDto())).ToList();

                return SimpleMapper.Map(w, new WebhookDto { AverageRequestTimeMs = totalAverage, Schemas = schemas });
            });

            return Ok(response);
        }

        /// <summary>
        /// Create a new webhook.
        /// </summary>
        /// <param name="app">The name of the app.</param>
        /// <param name="request">The webhook object that needs to be added to the app.</param>
        /// <returns>
        /// 201 => Webhook created.
        /// 400 => Webhook is not valid.
        /// 404 => App not found.
        /// </returns>
        /// <remarks>
        /// All events for the specified schemas will be sent to the url. The timeout is 2 seconds.
        /// </remarks>
        [HttpPost]
        [Route("apps/{app}/webhooks/")]
        [ProducesResponseType(typeof(EntityCreatedDto), 201)]
        [ProducesResponseType(typeof(ErrorDto), 400)]
        [ApiCosts(1)]
        public async Task<IActionResult> PostWebhook(string app, [FromBody] CreateWebhookDto request)
        {
            var schemas = request.Schemas.Select(s => SimpleMapper.Map(s, new WebhookSchema())).ToList();

            var command = new CreateWebhook { Url = request.Url, Schemas = schemas };

            var context = await CommandBus.PublishAsync(command);

            var result = context.Result<EntityCreatedResult<Guid>>();
            var response = new WebhookCreatedDto { Id = result.IdOrValue, SharedSecret = command.SharedSecret, Version = result.Version };

            return CreatedAtAction(nameof(GetWebhooks), new { app }, response);
        }

        /// <summary>
        /// Update a webhook.
        /// </summary>
        /// <param name="app">The name of the app.</param>
        /// <param name="id">The id of the webhook to update.</param>
        /// <param name="request">The webhook object that needs to be added to the app.</param>
        /// <returns>
        /// 203 => Webhook updated.
        /// 400 => Webhook is not valid.
        /// 404 => Webhook or app not found.
        /// </returns>
        /// <remarks>
        /// All events for the specified schemas will be sent to the url. The timeout is 2 seconds.
        /// </remarks>
        [HttpPut]
        [Route("apps/{app}/webhooks/{id}")]
        [ProducesResponseType(typeof(ErrorDto), 400)]
        [ApiCosts(1)]
        public async Task<IActionResult> PutWebhook(string app, Guid id, [FromBody] CreateWebhookDto request)
        {
            var schemas = request.Schemas.Select(s => SimpleMapper.Map(s, new WebhookSchema())).ToList();

            var command = new UpdateWebhook { WebhookId = id, Url = request.Url, Schemas = schemas };

            await CommandBus.PublishAsync(command);

            return NoContent();
        }

        /// <summary>
        /// Delete a webhook.
        /// </summary>
        /// <param name="app">The name of the app.</param>
        /// <param name="id">The id of the webhook to delete.</param>
        /// <returns>
        /// 204 => Webhook has been deleted.
        /// 404 => Webhook or app not found.
        /// </returns>
        [HttpDelete]
        [Route("apps/{app}/webhooks/{id}")]
        [ApiCosts(1)]
        public async Task<IActionResult> DeleteWebhook(string app, Guid id)
        {
            await CommandBus.PublishAsync(new DeleteWebhook { WebhookId = id });

            return NoContent();
        }

        /// <summary>
        /// Get webhook events.
        /// </summary>
        /// <param name="app">The name of the app.</param>
        /// <param name="skip">The number of events to skip.</param>
        /// <param name="take">The number of events to take.</param>
        /// <returns>
        /// 200 => Webhook events returned.
        /// 404 => App not found.
        /// </returns>
        [HttpGet]
        [Route("apps/{app}/webhooks/events")]
        [ProducesResponseType(typeof(WebhookEventsDto), 200)]
        [ApiCosts(0)]
        public async Task<IActionResult> GetEvents(string app, [FromQuery] int skip = 0, [FromQuery] int take = 20)
        {
            var taskForItems = webhookEventsRepository.QueryByAppAsync(App.Id, skip, take);
            var taskForCount = webhookEventsRepository.CountByAppAsync(App.Id);

            await Task.WhenAll(taskForItems, taskForCount);

            var response = new WebhookEventsDto
            {
                Total = taskForCount.Result,
                Items = taskForItems.Result.Select(x =>
                {
                    var itemModel = new WebhookEventDto();

                    SimpleMapper.Map(x, itemModel);
                    SimpleMapper.Map(x.Job, itemModel);

                    return itemModel;
                }).ToArray()
            };

            return Ok(response);
        }

        /// <summary>
        /// Enqueue the event to be send.
        /// </summary>
        /// <param name="app">The name of the app.</param>
        /// <param name="id">The event to enqueue.</param>
        /// <returns>
        /// 200 => Webhook enqueued.
        /// 404 => App or webhook event not found.
        /// </returns>
        [HttpPut]
        [Route("apps/{app}/webhooks/events/{id}")]
        [ApiCosts(0)]
        public async Task<IActionResult> PutEvent(string app, Guid id)
        {
            var entity = await webhookEventsRepository.FindAsync(id);

            if (entity == null)
            {
                return NotFound();
            }

            await webhookEventsRepository.EnqueueAsync(id, SystemClock.Instance.GetCurrentInstant());

            return Ok();
        }
    }
}
