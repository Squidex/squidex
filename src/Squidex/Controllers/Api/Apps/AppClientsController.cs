// ==========================================================================
//  AppClientsController.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Primitives;
using NSwag.Annotations;
using Squidex.Controllers.Api.Apps.Models;
using Squidex.Domain.Apps.Write.Apps.Commands;
using Squidex.Infrastructure.CQRS.Commands;
using Squidex.Infrastructure.Reflection;
using Squidex.Pipeline;

namespace Squidex.Controllers.Api.Apps
{
    /// <summary>
    /// Manages and configures apps.
    /// </summary>
    [MustBeAppOwner]
    [ApiExceptionFilter]
    [AppApi]
    [SwaggerTag(nameof(Apps))]
    public sealed class AppClientsController : ControllerBase
    {
        public AppClientsController(ICommandBus commandBus)
            : base(commandBus)
        {
        }

        /// <summary>
        /// Get app clients.
        /// </summary>
        /// <param name="app">The name of the app.</param>
        /// <returns>
        /// 200 => Client keys returned.
        /// 404 => App not found.
        /// </returns>
        /// <remarks>
        /// Gets all configured clients for the app with the specified name.
        /// </remarks>
        [HttpGet]
        [Route("apps/{app}/clients/")]
        [ProducesResponseType(typeof(ClientDto[]), 200)]
        [ApiCosts(1)]
        public IActionResult GetClients(string app)
        {
            var response = App.Clients.Select(x => SimpleMapper.Map(x.Value, new ClientDto { Id = x.Key })).ToList();

            Response.Headers["ETag"] = new StringValues(App.Version.ToString());

            return Ok(response);
        }

        /// <summary>
        /// Create a new app client.
        /// </summary>
        /// <param name="app">The name of the app.</param>
        /// <param name="request">Client object that needs to be added to the app.</param>
        /// <returns>
        /// 201 => Client generated.
        /// 404 => App not found.
        /// </returns>
        /// <remarks>
        /// Create a new client for the app with the specified name.
        /// The client secret is auto generated on the server and returned. The client does not exire, the access token is valid for 30 days.
        /// </remarks>
        [HttpPost]
        [Route("apps/{app}/clients/")]
        [ProducesResponseType(typeof(ClientDto), 201)]
        [ApiCosts(1)]
        public async Task<IActionResult> PostClient(string app, [FromBody] CreateAppClientDto request)
        {
            var command = SimpleMapper.Map(request, new AttachClient());

            await CommandBus.PublishAsync(command);

            var response = SimpleMapper.Map(command, new ClientDto { Name = command.Id });

            return CreatedAtAction(nameof(GetClients), new { app }, response);
        }

        /// <summary>
        /// Updates an app client.
        /// </summary>
        /// <param name="app">The name of the app.</param>
        /// <param name="clientId">The id of the client that must be updated.</param>
        /// <param name="request">Client object that needs to be updated.</param>
        /// <returns>
        /// 204 => Client updated.
        /// 404 => App not found or client not found.
        /// </returns>
        /// <remarks>
        /// Only the display name can be changed, create a new client if necessary.
        /// </remarks>
        [HttpPut]
        [Route("apps/{app}/clients/{clientId}/")]
        [ApiCosts(1)]
        public async Task<IActionResult> PutClient(string app, string clientId, [FromBody] UpdateAppClientDto request)
        {
            await CommandBus.PublishAsync(SimpleMapper.Map(request, new UpdateClient { Id = clientId }));

            return NoContent();
        }

        /// <summary>
        /// Revoke an app client
        /// </summary>
        /// <param name="app">The name of the app.</param>
        /// <param name="clientId">The id of the client that must be deleted.</param>
        /// <returns>
        /// 204 => Client revoked.
        /// 404 => App not found or client not found.
        /// </returns>
        /// <remarks>
        /// The application that uses this client credentials cannot access the API after it has been revoked.
        /// </remarks>
        [HttpDelete]
        [Route("apps/{app}/clients/{clientId}/")]
        [ApiCosts(1)]
        public async Task<IActionResult> DeleteClient(string app, string clientId)
        {
            await CommandBus.PublishAsync(new RevokeClient { Id = clientId });

            return NoContent();
        }
    }
}
