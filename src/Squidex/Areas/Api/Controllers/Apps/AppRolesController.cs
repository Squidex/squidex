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
using Squidex.Infrastructure;
using Squidex.Infrastructure.Commands;
using Squidex.Pipeline;
using Squidex.Shared;

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
        /// 200 => App roles returned.
        /// 404 => App not found.
        /// </returns>
        [HttpGet]
        [Route("apps/{app}/roles/")]
        [ProducesResponseType(typeof(RolesDto), 200)]
        [ApiPermission(Permissions.AppRolesRead)]
        [ApiCosts(0)]
        public IActionResult GetRoles(string app)
        {
            var response = RolesDto.FromApp(App);

            Response.Headers[HeaderNames.ETag] = App.Version.ToString();

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
        [ProducesResponseType(typeof(string[]), 200)]
        [ApiPermission(Permissions.AppRolesRead)]
        [ApiCosts(0)]
        public async Task<IActionResult> GetPermissions(string app)
        {
            var response = await permissionsProvider.GetPermissionsAsync(App);

            Response.Headers[HeaderNames.ETag] = string.Join(";", response).Sha256Base64();

            return Ok(response);
        }

        /// <summary>
        /// Add role to app.
        /// </summary>
        /// <param name="app">The name of the app.</param>
        /// <param name="request">Role object that needs to be added to the app.</param>
        /// <returns>
        /// 200 => User assigned to app.
        /// 400 => Role name already in use.
        /// 404 => App not found.
        /// </returns>
        [HttpPost]
        [Route("apps/{app}/roles/")]
        [ProducesResponseType(typeof(ErrorDto), 400)]
        [ApiPermission(Permissions.AppRolesCreate)]
        [ApiCosts(1)]
        public async Task<IActionResult> PostRole(string app, [FromBody] AddRoleDto request)
        {
            await CommandBus.PublishAsync(request.ToCommand());

            return NoContent();
        }

        /// <summary>
        /// Update an existing app role.
        /// </summary>
        /// <param name="app">The name of the app.</param>
        /// <param name="role">The name of the role to be updated.</param>
        /// <param name="request">Role to be updated for the app.</param>
        /// <returns>
        /// 204 => Role updated.
        /// 400 => Role request not valid.
        /// 404 => Role or app not found.
        /// </returns>
        [HttpPut]
        [Route("apps/{app}/roles/{role}/")]
        [ApiPermission(Permissions.AppRolesUpdate)]
        [ApiCosts(1)]
        public async Task<IActionResult> UpdateRole(string app, string role, [FromBody] UpdateRoleDto request)
        {
            await CommandBus.PublishAsync(request.ToCommand(role));

            return NoContent();
        }

        /// <summary>
        /// Remove role from app.
        /// </summary>
        /// <param name="app">The name of the app.</param>
        /// <param name="role">The name of the role.</param>
        /// <returns>
        /// 204 => Role deleted.
        /// 400 => Role is in use by contributor or client or default role.
        /// 404 => Role or app not found.
        /// </returns>
        [HttpDelete]
        [Route("apps/{app}/roles/{role}/")]
        [ProducesResponseType(typeof(ErrorDto), 400)]
        [ApiPermission(Permissions.AppRolesDelete)]
        [ApiCosts(1)]
        public async Task<IActionResult> DeleteRole(string app, string role)
        {
            await CommandBus.PublishAsync(new DeleteRole { Name = role });

            return NoContent();
        }
    }
}
