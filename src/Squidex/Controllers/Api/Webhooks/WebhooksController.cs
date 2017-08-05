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
using Microsoft.Extensions.Primitives;
using NodaTime;
using NSwag.Annotations;
using Squidex.Controllers.Api.Webhooks.Models;
using Squidex.Domain.Apps.Read.Schemas.Repositories;
using Squidex.Domain.Apps.Write.Schemas.Commands;
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
    [SwaggerTag("Webhooks")]
    [MustBeAppDeveloper]
    public class WebhooksController : ControllerBase
    {
        private readonly ISchemaWebhookRepository webhooksRepository;
        private readonly IWebhookEventRepository webhookEventsRepository;

        public WebhooksController(ICommandBus commandBus,
            ISchemaWebhookRepository webhooksRepository,
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

            Response.Headers["ETag"] = new StringValues(App.Version.ToString());

            var response = webhooks.Select(w =>
            {
                var count = w.TotalTimedout + w.TotalSucceeded + w.TotalFailed;
                var average = count == 0 ? 0 : w.TotalRequestTime / count;

                return SimpleMapper.Map(w, new WebhookDto { AverageRequestTimeMs = average });
            });

            return Ok(response);
        }

        /// <summary>
        /// Create a new webhook.
        /// </summary>
        /// <param name="app">The name of the app.</param>
        /// <param name="name">The name of the schema.</param>
        /// <param name="request">The webhook object that needs to be added to the app.</param>
        /// <returns>
        /// 201 => Webhook created.
        /// 400 => Webhook name or properties are not valid.
        /// 409 => Webhook name already in use.
        /// 404 => App or schema not found.
        /// </returns>
        /// <remarks>
        /// All events for the specified app will be sent to the url. The timeout is 2 seconds.
        /// </remarks>
        [HttpPost]
        [Route("apps/{app}/schemas/{name}/webhooks/")]
        [ProducesResponseType(typeof(WebhookCreatedDto), 201)]
        [ProducesResponseType(typeof(ErrorDto), 400)]
        [ProducesResponseType(typeof(ErrorDto), 409)]
        [ApiCosts(1)]
        public async Task<IActionResult> PostWebhook(string app, string name, [FromBody] CreateWebhookDto request)
        {
            var command = new AddWebhook { Url = request.Url };

            await CommandBus.PublishAsync(command);

            var response = SimpleMapper.Map(command, new WebhookCreatedDto { SchemaId = command.SchemaId.Id.ToString() });

            return CreatedAtAction(nameof(GetWebhooks), new { app }, response);
        }

        /// <summary>
        /// Delete a webhook.
        /// </summary>
        /// <param name="app">The name of the app.</param>
        /// <param name="name">The name of the schema.</param>
        /// <param name="id">The id of the webhook to delete.</param>
        /// <returns>
        /// 204 => Webhook has been deleted.
        /// 404 => Webhook or shema or app not found.
        /// </returns>
        [HttpDelete]
        [Route("apps/{app}/schemas/{name}/webhooks/{id}")]
        [ApiCosts(1)]
        public async Task<IActionResult> DeleteWebhook(string app, string name, Guid id)
        {
            await CommandBus.PublishAsync(new DeleteWebhook { Id = id });

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
