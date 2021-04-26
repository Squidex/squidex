// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Net.Http.Headers;
using Squidex.Areas.Api.Controllers.Apps.Models;
using Squidex.Domain.Apps.Entities.Apps;
using Squidex.Domain.Apps.Entities.Apps.Commands;
using Squidex.Infrastructure.Commands;
using Squidex.Shared;
using Squidex.Web;

namespace Squidex.Areas.Api.Controllers.Apps
{
    /// <summary>
    /// Manages and configures apps.
    /// </summary>
    [ApiExplorerSettings(GroupName = nameof(Apps))]
    public sealed class AppClientsController : ApiController
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
        /// 200 => Clients returned.
        /// 404 => App not found.
        /// </returns>
        /// <remarks>
        /// Gets all configured clients for the app with the specified name.
        /// </remarks>
        [HttpGet]
        [Route("apps/{app}/clients/")]
        [ProducesResponseType(typeof(ClientsDto), StatusCodes.Status200OK)]
        [ApiPermissionOrAnonymous(Permissions.AppClientsRead)]
        [ApiCosts(0)]
        public IActionResult GetClients(string app)
        {
            var response = Deferred.Response(() =>
            {
                return GetResponse(App);
            });

            Response.Headers[HeaderNames.ETag] = App.ToEtag();

            return Ok(response);
        }

        /// <summary>
        /// Create a new app client.
        /// </summary>
        /// <param name="app">The name of the app.</param>
        /// <param name="request">Client object that needs to be added to the app.</param>
        /// <returns>
        /// 201 => Client created.
        /// 400 => Client request not valid.
        /// 404 => App not found.
        /// </returns>
        /// <remarks>
        /// Create a new client for the app with the specified name.
        /// The client secret is auto generated on the server and returned. The client does not exire, the access token is valid for 30 days.
        /// </remarks>
        [HttpPost]
        [Route("apps/{app}/clients/")]
        [ProducesResponseType(typeof(ClientsDto), 201)]
        [ApiPermissionOrAnonymous(Permissions.AppClientsCreate)]
        [ApiCosts(1)]
        public async Task<IActionResult> PostClient(string app, [FromBody] CreateClientDto request)
        {
            var command = request.ToCommand();

            var response = await InvokeCommandAsync(command);

            return CreatedAtAction(nameof(GetClients), new { app }, response);
        }

        /// <summary>
        /// Updates an app client.
        /// </summary>
        /// <param name="app">The name of the app.</param>
        /// <param name="id">The id of the client that must be updated.</param>
        /// <param name="request">Client object that needs to be updated.</param>
        /// <returns>
        /// 200 => Client updated.
        /// 400 => Client request not valid.
        /// 404 => Client or app not found.
        /// </returns>
        /// <remarks>
        /// Only the display name can be changed, create a new client if necessary.
        /// </remarks>
        [HttpPut]
        [Route("apps/{app}/clients/{id}/")]
        [ProducesResponseType(typeof(ClientsDto), StatusCodes.Status200OK)]
        [ApiPermissionOrAnonymous(Permissions.AppClientsUpdate)]
        [ApiCosts(1)]
        public async Task<IActionResult> PutClient(string app, string id, [FromBody] UpdateClientDto request)
        {
            var command = request.ToCommand(id);

            var response = await InvokeCommandAsync(command);

            return Ok(response);
        }

        /// <summary>
        /// Revoke an app client.
        /// </summary>
        /// <param name="app">The name of the app.</param>
        /// <param name="id">The id of the client that must be deleted.</param>
        /// <returns>
        /// 200 => Client deleted.
        /// 404 => Client or app not found.
        /// </returns>
        /// <remarks>
        /// The application that uses this client credentials cannot access the API after it has been revoked.
        /// </remarks>
        [HttpDelete]
        [Route("apps/{app}/clients/{id}/")]
        [ProducesResponseType(typeof(ClientsDto), StatusCodes.Status200OK)]
        [ApiPermissionOrAnonymous(Permissions.AppClientsDelete)]
        [ApiCosts(1)]
        public async Task<IActionResult> DeleteClient(string app, string id)
        {
            var command = new RevokeClient { Id = id };

            var response = await InvokeCommandAsync(command);

            return Ok(response);
        }

        private async Task<ClientsDto> InvokeCommandAsync(ICommand command)
        {
            var context = await CommandBus.PublishAsync(command);

            var result = context.Result<IAppEntity>();
            var response = GetResponse(result);

            return response;
        }

        private ClientsDto GetResponse(IAppEntity app)
        {
            return ClientsDto.FromApp(app, Resources);
        }
    }
}
