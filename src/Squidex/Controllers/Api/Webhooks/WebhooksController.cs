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

        public WebhooksController(ICommandBus commandBus, ISchemaWebhookRepository webhooksRepository) 
            : base(commandBus)
        {
            this.webhooksRepository = webhooksRepository;
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

                return SimpleMapper.Map(w, new WebhookDto { AverageRequestTimeMs = average, LastDumps = w.LastDumps.ToList() });
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

            return CreatedAtAction(nameof(GetWebhooks), new { app }, SimpleMapper.Map(command, new WebhookCreatedDto { SchemaId = command.SchemaId.Id.ToString() }));
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
        public async Task<IActionResult> DeleteSchema(string app, string name, Guid id)
        {
            await CommandBus.PublishAsync(new DeleteWebhook { Id = id });

            return NoContent();
        }
    }
}
