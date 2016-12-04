// ==========================================================================
//  AppClientsController.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NSwag.Annotations;
using Squidex.Controllers.Api.Apps.Models;
using Squidex.Infrastructure.CQRS.Commands;
using Squidex.Infrastructure.Reflection;
using Squidex.Pipeline;
using Squidex.Read.Apps.Services;
using Squidex.Write.Apps;
using Squidex.Write.Apps.Commands;

namespace Squidex.Controllers.Api.Apps
{
    /// <summary>
    /// Manages and configures apps.
    /// </summary>
    [Authorize(Roles = "app-owner")]
    [ApiExceptionFilter]
    [ServiceFilter(typeof(AppFilterAttribute))]
    [SwaggerTag("Apps")]
    public class AppClientsController : ControllerBase
    {
        private readonly IAppProvider appProvider;

        public AppClientsController(ICommandBus commandBus, IAppProvider appProvider)
            : base(commandBus)
        {
            this.appProvider = appProvider;
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
        /// Gets all configured client keys for the app with the specified name.
        /// </remarks>
        [HttpGet]
        [Route("apps/{app}/clients/")]
        [ProducesResponseType(typeof(ClientDto[]), 200)]
        public async Task<IActionResult> GetClients(string app)
        {
            var entity = await appProvider.FindAppByNameAsync(app);

            if (entity == null)
            {
                return NotFound();
            }

            var response = entity.Clients.Select(x => SimpleMapper.Map(x, new ClientDto())).ToList();

            return Ok(response);
        }

        /// <summary>
        /// Create a new app client.
        /// </summary>
        /// <param name="app">The name of the app.</param>
        /// <param name="request">Client object that needs to be added to the app.</param>
        /// <returns>
        /// 201 => Client key generated.
        /// 404 => App not found.
        /// </returns>
        /// <remarks>
        /// Create a new key for the app with the specified name.
        /// The client secret is auto generated on the server and returned. The client is valid for one year.
        /// </remarks>
        [HttpPost]
        [Route("apps/{app}/clients/")]
        [ProducesResponseType(typeof(ClientDto[]), 201)]
        public async Task<IActionResult> PostClient(string app, [FromBody] AttachClientDto request)
        {
            var context = await CommandBus.PublishAsync(SimpleMapper.Map(request, new AttachClient()));
            var result = context.Result<AppClient>();

            var response = SimpleMapper.Map(result, new ClientDto());

            return StatusCode(201, response);
        }

        /// <summary>
        /// Updates an app client.
        /// </summary>
        /// <param name="app">The name of the app.</param>
        /// <param name="clientId">The id of the client that must be updated.</param>
        /// <param name="request">Client object that needs to be added to the app.</param>
        /// <returns>
        /// 201 => Client key generated.
        /// 404 => App not found or client not found.
        /// </returns>
        [HttpPut]
        [Route("apps/{app}/clients/{clientId}/")]
        [ProducesResponseType(typeof(ClientDto[]), 201)]
        public async Task<IActionResult> PutClient(string app, string clientId, [FromBody] RenameClientDto request)
        {
            await CommandBus.PublishAsync(SimpleMapper.Map(request, new RenameClient { Id = clientId }));

            return NoContent();
        }

        /// <summary>
        /// Revoke an app client
        /// </summary>
        /// <param name="app">The name of the app.</param>
        /// <param name="clientId">The id of the client that must be deleted.</param>
        /// <returns>
        /// 404 => App not found or client not found.
        /// 204 => Client revoked.
        /// </returns>
        [HttpDelete]
        [Route("apps/{app}/clients/{clientId}/")]
        public async Task<IActionResult> DeleteClient(string app, string clientId)
        {
            await CommandBus.PublishAsync(new RevokeClient { Id = clientId });

            return NoContent();
        }
    }
}
