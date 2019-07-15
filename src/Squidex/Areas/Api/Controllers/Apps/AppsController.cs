// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Net.Http.Headers;
using Squidex.Areas.Api.Controllers.Apps.Models;
using Squidex.Domain.Apps.Entities;
using Squidex.Domain.Apps.Entities.Apps;
using Squidex.Domain.Apps.Entities.Apps.Commands;
using Squidex.Domain.Apps.Entities.Apps.Services;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Commands;
using Squidex.Infrastructure.Security;
using Squidex.Shared;
using Squidex.Shared.Identity;
using Squidex.Web;

namespace Squidex.Areas.Api.Controllers.Apps
{
    /// <summary>
    /// Manages and configures apps.
    /// </summary>
    [ApiExplorerSettings(GroupName = nameof(Apps))]
    public sealed class AppsController : ApiController
    {
        private readonly IAppProvider appProvider;
        private readonly IAppPlansProvider appPlansProvider;

        public AppsController(ICommandBus commandBus,
            IAppProvider appProvider,
            IAppPlansProvider appPlansProvider)
            : base(commandBus)
        {
            this.appProvider = appProvider;
            this.appPlansProvider = appPlansProvider;
        }

        /// <summary>
        /// Get your apps.
        /// </summary>
        /// <returns>
        /// 200 => Apps returned.
        /// </returns>
        /// <remarks>
        /// You can only retrieve the list of apps when you are authenticated as a user (OpenID implicit flow).
        /// You will retrieve all apps, where you are assigned as a contributor.
        /// </remarks>
        [HttpGet]
        [Route("apps/")]
        [ProducesResponseType(typeof(AppDto[]), 200)]
        [ApiPermission]
        [ApiCosts(0)]
        public async Task<IActionResult> GetApps()
        {
            var userOrClientId = HttpContext.User.UserOrClientId();
            var userPermissions = HttpContext.Permissions();

            var apps = await appProvider.GetUserApps(userOrClientId, userPermissions);

            var response = Deferred.Response(() =>
            {
                return apps.Select(a => AppDto.FromApp(a, userOrClientId, userPermissions, appPlansProvider, this)).ToArray();
            });

            Response.Headers[HeaderNames.ETag] = apps.ToEtag();

            return Ok(response);
        }

        /// <summary>
        /// Create a new app.
        /// </summary>
        /// <param name="request">The app object that needs to be added to squidex.</param>
        /// <returns>
        /// 201 => App created.
        /// 400 => App request not valid.
        /// 409 => App name is already in use.
        /// </returns>
        /// <remarks>
        /// You can only create an app when you are authenticated as a user (OpenID implicit flow).
        /// You will be assigned as owner of the new app automatically.
        /// </remarks>
        [HttpPost]
        [Route("apps/")]
        [ProducesResponseType(typeof(AppDto), 201)]
        [ApiPermission]
        [ApiCosts(1)]
        public async Task<IActionResult> PostApp([FromBody] CreateAppDto request)
        {
            var context = await CommandBus.PublishAsync(request.ToCommand());

            var userOrClientId = HttpContext.User.UserOrClientId();
            var userPermissions = HttpContext.Permissions();

            var result = context.Result<IAppEntity>();
            var response = AppDto.FromApp(result, userOrClientId, userPermissions, appPlansProvider, this);

            return CreatedAtAction(nameof(GetApps), response);
        }

        /// <summary>
        /// Archive the app.
        /// </summary>
        /// <param name="app">The name of the app to archive.</param>
        /// <returns>
        /// 204 => App archived.
        /// 404 => App not found.
        /// </returns>
        [HttpDelete]
        [Route("apps/{app}/")]
        [ApiPermission(Permissions.AppDelete)]
        [ApiCosts(1)]
        public async Task<IActionResult> DeleteApp(string app)
        {
            await CommandBus.PublishAsync(new ArchiveApp());

            return NoContent();
        }
    }
}
