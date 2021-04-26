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
using Squidex.Domain.Apps.Entities.Apps.Invitation;
using Squidex.Domain.Apps.Entities.Apps.Plans;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Commands;
using Squidex.Infrastructure.Security;
using Squidex.Infrastructure.Translations;
using Squidex.Shared;
using Squidex.Shared.Users;
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
        private readonly IUserResolver userResolver;

        public AppContributorsController(ICommandBus commandBus, IAppPlansProvider appPlansProvider, IUserResolver userResolver)
            : base(commandBus)
        {
            this.appPlansProvider = appPlansProvider;

            this.userResolver = userResolver;
        }

        /// <summary>
        /// Get app contributors.
        /// </summary>
        /// <param name="app">The name of the app.</param>
        /// <returns>
        /// 200 => Contributors returned.
        /// 404 => App not found.
        /// </returns>
        [HttpGet]
        [Route("apps/{app}/contributors/")]
        [ProducesResponseType(typeof(ContributorsDto), StatusCodes.Status200OK)]
        [ApiPermissionOrAnonymous(Permissions.AppContributorsRead)]
        [ApiCosts(0)]
        public IActionResult GetContributors(string app)
        {
            var response = Deferred.AsyncResponse(() =>
            {
                return GetResponseAsync(App, false);
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
        /// 201 => Contributor assigned to app.
        /// 400 => Contributor request not valid.
        /// 404 => App not found.
        /// </returns>
        [HttpPost]
        [Route("apps/{app}/contributors/")]
        [ProducesResponseType(typeof(ContributorsDto), 201)]
        [ApiPermissionOrAnonymous(Permissions.AppContributorsAssign)]
        [ApiCosts(1)]
        public async Task<IActionResult> PostContributor(string app, [FromBody] AssignContributorDto request)
        {
            var command = request.ToCommand();

            var response = await InvokeCommandAsync(command);

            return CreatedAtAction(nameof(GetContributors), new { app }, response);
        }

        /// <summary>
        /// Remove yourself.
        /// </summary>
        /// <param name="app">The name of the app.</param>
        /// <returns>
        /// 200 => Contributor removed.
        /// 404 => Contributor or app not found.
        /// </returns>
        [HttpDelete]
        [Route("apps/{app}/contributors/me/")]
        [ProducesResponseType(typeof(ContributorsDto), StatusCodes.Status200OK)]
        [ApiPermission]
        [ApiCosts(1)]
        public async Task<IActionResult> DeleteMyself(string app)
        {
            var command = new RemoveContributor { ContributorId = UserId() };

            var response = await InvokeCommandAsync(command);

            return Ok(response);
        }

        /// <summary>
        /// Remove contributor.
        /// </summary>
        /// <param name="app">The name of the app.</param>
        /// <param name="id">The id of the contributor.</param>
        /// <returns>
        /// 200 => Contributor removed.
        /// 404 => Contributor or app not found.
        /// </returns>
        [HttpDelete]
        [Route("apps/{app}/contributors/{id}/")]
        [ProducesResponseType(typeof(ContributorsDto), StatusCodes.Status200OK)]
        [ApiPermissionOrAnonymous(Permissions.AppContributorsRevoke)]
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
                return await GetResponseAsync(invited.App, true);
            }
            else
            {
                return await GetResponseAsync(context.Result<IAppEntity>(), false);
            }
        }

        private string UserId()
        {
            var subject = User.OpenIdSubject();

            if (string.IsNullOrWhiteSpace(subject))
            {
                throw new DomainForbiddenException(T.Get("common.httpOnlyAsUser"));
            }

            return subject;
        }

        private Task<ContributorsDto> GetResponseAsync(IAppEntity app, bool invited)
        {
            return ContributorsDto.FromAppAsync(app, Resources, userResolver, appPlansProvider, invited);
        }
    }
}
