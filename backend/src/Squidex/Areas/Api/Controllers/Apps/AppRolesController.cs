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
using Squidex.Infrastructure;
using Squidex.Infrastructure.Commands;
using Squidex.Shared;
using Squidex.Web;

namespace Squidex.Areas.Api.Controllers.Apps
{
    /// <summary>
    /// Manages and configures apps.
    /// </summary>
    [ApiExplorerSettings(GroupName = nameof(Apps))]
    public sealed class AppRolesController : ApiController
    {
        private readonly RolePermissionsProvider permissionsProvider;

        public AppRolesController(ICommandBus commandBus, RolePermissionsProvider permissionsProvider)
            : base(commandBus)
        {
            this.permissionsProvider = permissionsProvider;
        }

        /// <summary>
        /// Get app roles.
        /// </summary>
        /// <param name="app">The name of the app.</param>
        /// <returns>
        /// 200 => Roles returned.
        /// 404 => App not found.
        /// </returns>
        [HttpGet]
        [Route("apps/{app}/roles/")]
        [ProducesResponseType(typeof(RolesDto), StatusCodes.Status200OK)]
        [ApiPermissionOrAnonymous(Permissions.AppRolesRead)]
        [ApiCosts(0)]
        public IActionResult GetRoles(string app)
        {
            var response = Deferred.Response(() =>
            {
                return GetResponse(App);
            });

            Response.Headers[HeaderNames.ETag] = App.ToEtag();

            return Ok(response);
        }

        /// <summary>
        /// Get app permissions.
        /// </summary>
        /// <param name="app">The name of the app.</param>
        /// <returns>
        /// 200 => App permissions returned.
        /// 404 => App not found.
        /// </returns>
        [HttpGet]
        [Route("apps/{app}/roles/permissions")]
        [ProducesResponseType(typeof(string[]), StatusCodes.Status200OK)]
        [ApiPermissionOrAnonymous(Permissions.AppRolesRead)]
        [ApiCosts(0)]
        public IActionResult GetPermissions(string app)
        {
            var response = Deferred.AsyncResponse(() =>
            {
                return permissionsProvider.GetPermissionsAsync(App);
            });

            Response.Headers[HeaderNames.ETag] = string.Concat(response).ToSha256Base64();

            return Ok(response);
        }

        /// <summary>
        /// Add role to app.
        /// </summary>
        /// <param name="app">The name of the app.</param>
        /// <param name="request">Role object that needs to be added to the app.</param>
        /// <returns>
        /// 201 => Role created.
        /// 400 => Role request not valid.
        /// 404 => App not found.
        /// </returns>
        [HttpPost]
        [Route("apps/{app}/roles/")]
        [ProducesResponseType(typeof(RolesDto), 201)]
        [ApiPermissionOrAnonymous(Permissions.AppRolesCreate)]
        [ApiCosts(1)]
        public async Task<IActionResult> PostRole(string app, [FromBody] AddRoleDto request)
        {
            var command = request.ToCommand();

            var response = await InvokeCommandAsync(command);

            return CreatedAtAction(nameof(GetRoles), new { app }, response);
        }

        /// <summary>
        /// Update an app role.
        /// </summary>
        /// <param name="app">The name of the app.</param>
        /// <param name="roleName">The name of the role to be updated.</param>
        /// <param name="request">Role to be updated for the app.</param>
        /// <returns>
        /// 200 => Role updated.
        /// 400 => Role request not valid.
        /// 404 => Role or app not found.
        /// </returns>
        [HttpPut]
        [Route("apps/{app}/roles/{roleName}/")]
        [ProducesResponseType(typeof(RolesDto), StatusCodes.Status200OK)]
        [ApiPermissionOrAnonymous(Permissions.AppRolesUpdate)]
        [ApiCosts(1)]
        [UrlDecodeRouteParams]
        public async Task<IActionResult> PutRole(string app, string roleName, [FromBody] UpdateRoleDto request)
        {
            var command = request.ToCommand(roleName);

            var response = await InvokeCommandAsync(command);

            return Ok(response);
        }

        /// <summary>
        /// Remove role from app.
        /// </summary>
        /// <param name="app">The name of the app.</param>
        /// <param name="roleName">The name of the role.</param>
        /// <returns>
        /// 200 => Role deleted.
        /// 400 => Role is in use by contributor or client or a default role.
        /// 404 => Role or app not found.
        /// </returns>
        [HttpDelete]
        [Route("apps/{app}/roles/{roleName}/")]
        [ProducesResponseType(typeof(RolesDto), StatusCodes.Status200OK)]
        [ApiPermissionOrAnonymous(Permissions.AppRolesDelete)]
        [ApiCosts(1)]
        [UrlDecodeRouteParams]
        public async Task<IActionResult> DeleteRole(string app, string roleName)
        {
            var command = new DeleteRole { Name = roleName };

            var response = await InvokeCommandAsync(command);

            return Ok(response);
        }

        private async Task<RolesDto> InvokeCommandAsync(ICommand command)
        {
            var context = await CommandBus.PublishAsync(command);

            var result = context.Result<IAppEntity>();
            var response = GetResponse(result);

            return response;
        }

        private RolesDto GetResponse(IAppEntity result)
        {
            return RolesDto.FromApp(result, Resources);
        }
    }
}
