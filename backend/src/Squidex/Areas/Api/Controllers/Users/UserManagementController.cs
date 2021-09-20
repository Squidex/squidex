// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Squidex.Areas.Api.Controllers.Users.Models;
using Squidex.Domain.Users;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Commands;
using Squidex.Infrastructure.Translations;
using Squidex.Shared;
using Squidex.Web;

namespace Squidex.Areas.Api.Controllers.Users
{
    [ApiModelValidation(true)]
    public sealed class UserManagementController : ApiController
    {
        private readonly IUserService userService;

        public UserManagementController(ICommandBus commandBus, IUserService userService)
            : base(commandBus)
        {
            this.userService = userService;
        }

        [HttpGet]
        [Route("user-management/")]
        [ProducesResponseType(typeof(UsersDto), StatusCodes.Status200OK)]
        [ApiPermission(Permissions.AdminUsersRead)]
        public async Task<IActionResult> GetUsers([FromQuery] string? query = null, [FromQuery] int skip = 0, [FromQuery] int take = 10)
        {
            var users = await userService.QueryAsync(query, take, skip, HttpContext.RequestAborted);

            var response = UsersDto.FromResults(users, users.Total, Resources);

            return Ok(response);
        }

        [HttpGet]
        [Route("user-management/{id}/")]
        [ProducesResponseType(typeof(UserDto), 201)]
        [ApiPermission(Permissions.AdminUsersRead)]
        public async Task<IActionResult> GetUser(string id)
        {
            var user = await userService.FindByIdAsync(id, HttpContext.RequestAborted);

            if (user == null)
            {
                return NotFound();
            }

            var response = UserDto.FromUser(user, Resources);

            return Ok(response);
        }

        [HttpPost]
        [Route("user-management/")]
        [ProducesResponseType(typeof(UserDto), 201)]
        [ApiPermission(Permissions.AdminUsersCreate)]
        public async Task<IActionResult> PostUser([FromBody] CreateUserDto request)
        {
            var user = await userService.CreateAsync(request.Email, request.ToValues(), ct: HttpContext.RequestAborted);

            var response = UserDto.FromUser(user, Resources);

            return Ok(response);
        }

        [HttpPut]
        [Route("user-management/{id}/")]
        [ProducesResponseType(typeof(UserDto), 201)]
        [ApiPermission(Permissions.AdminUsersUpdate)]
        public async Task<IActionResult> PutUser(string id, [FromBody] UpdateUserDto request)
        {
            var user = await userService.UpdateAsync(id, request.ToValues(), ct: HttpContext.RequestAborted);

            var response = UserDto.FromUser(user, Resources);

            return Ok(response);
        }

        [HttpPut]
        [Route("user-management/{id}/lock/")]
        [ProducesResponseType(typeof(UserDto), 201)]
        [ApiPermission(Permissions.AdminUsersLock)]
        public async Task<IActionResult> LockUser(string id)
        {
            if (this.IsUser(id))
            {
                throw new DomainForbiddenException(T.Get("users.lockYourselfError"));
            }

            var user = await userService.LockAsync(id, HttpContext.RequestAborted);

            var response = UserDto.FromUser(user, Resources);

            return Ok(response);
        }

        [HttpPut]
        [Route("user-management/{id}/unlock/")]
        [ProducesResponseType(typeof(UserDto), 201)]
        [ApiPermission(Permissions.AdminUsersUnlock)]
        public async Task<IActionResult> UnlockUser(string id)
        {
            if (this.IsUser(id))
            {
                throw new DomainForbiddenException(T.Get("users.unlockYourselfError"));
            }

            var user = await userService.UnlockAsync(id, HttpContext.RequestAborted);

            var response = UserDto.FromUser(user, Resources);

            return Ok(response);
        }

        [HttpDelete]
        [Route("user-management/{id}/")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ApiPermission(Permissions.AdminUsersUnlock)]
        public async Task<IActionResult> DeleteUser(string id)
        {
            if (this.IsUser(id))
            {
                throw new DomainForbiddenException(T.Get("users.deleteYourselfError"));
            }

            await userService.DeleteAsync(id, HttpContext.RequestAborted);

            return NoContent();
        }
    }
}
