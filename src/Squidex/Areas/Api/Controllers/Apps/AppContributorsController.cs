// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Net.Http.Headers;
using Squidex.Areas.Api.Controllers.Apps.Models;
using Squidex.Domain.Apps.Entities.Apps;
using Squidex.Domain.Apps.Entities.Apps.Commands;
using Squidex.Domain.Apps.Entities.Apps.Invitation;
using Squidex.Domain.Apps.Entities.Apps.Services;
using Squidex.Infrastructure.Commands;
using Squidex.Shared;
using Squidex.Web;

namespace Squidex.Areas.Api.Controllers.Apps
{
    /// <summary>
    /// Manages and configures apps.
    /// </summary>
    [ApiExplorerSettings(GroupName = nameof(Apps))]
    public sealed class AppContributorsController : ApiController
    {
        private readonly IAppPlansProvider appPlansProvider;

        public AppContributorsController(ICommandBus commandBus, IAppPlansProvider appPlansProvider)
            : base(commandBus)
        {
            this.appPlansProvider = appPlansProvider;
        }

        /// <summary>
        /// Get app contributors.
        /// </summary>
        /// <param name="app">The name of the app.</param>
        /// <returns>
        /// 200 => App contributors returned.
        /// 404 => App not found.
        /// </returns>
        [HttpGet]
        [Route("apps/{app}/contributors/")]
        [ProducesResponseType(typeof(ContributorsDto), 200)]
        [ApiPermission(Permissions.AppContributorsRead)]
        [ApiCosts(0)]
        public IActionResult GetContributors(string app)
        {
            var response = Deferred.Response(() =>
            {
                return ContributorsDto.FromApp(App, appPlansProvider, this, false);
            });

            Response.Headers[HeaderNames.ETag] = App.ToEtag();

            return Ok(response);
        }

        /// <summary>
        /// Assign contributor to app.
        /// </summary>
        /// <param name="app">The name of the app.</param>
        /// <param name="request">Contributor object that needs to be added to the app.</param>
        /// <returns>
        /// 201 => User assigned to app.
        /// 400 => User is already assigned to the app or not found.
        /// 404 => App not found.
        /// </returns>
        [HttpPost]
        [Route("apps/{app}/contributors/")]
        [ProducesResponseType(typeof(ContributorsDto), 200)]
        [ApiPermission(Permissions.AppContributorsAssign)]
        [ApiCosts(1)]
        public async Task<IActionResult> PostContributor(string app, [FromBody] AssignContributorDto request)
        {
            var command = request.ToCommand();

            var response = await InvokeCommandAsync(command);

            return CreatedAtAction(nameof(GetContributors), new { app }, response);
        }

        /// <summary>
        /// Remove contributor from app.
        /// </summary>
        /// <param name="app">The name of the app.</param>
        /// <param name="id">The id of the contributor.</param>
        /// <returns>
        /// 200 => User removed from app.
        /// 400 => User is not assigned to the app.
        /// 404 => Contributor or app not found.
        /// </returns>
        [HttpDelete]
        [Route("apps/{app}/contributors/{id}/")]
        [ProducesResponseType(typeof(ContributorsDto), 200)]
        [ApiPermission(Permissions.AppContributorsRevoke)]
        [ApiCosts(1)]
        public async Task<IActionResult> DeleteContributor(string app, string id)
        {
            var command = new RemoveContributor { ContributorId = id };

            var response = await InvokeCommandAsync(command);

            return Ok(response);
        }

        private async Task<ContributorsDto> InvokeCommandAsync(ICommand command)
        {
            var context = await CommandBus.PublishAsync(command);

            if (context.PlainResult is InvitedResult invited)
            {
                return ContributorsDto.FromApp(invited.App, appPlansProvider, this, true);
            }
            else
            {
                return ContributorsDto.FromApp(context.Result<IAppEntity>(), appPlansProvider, this, false);
            }
        }
    }
}
