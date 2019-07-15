// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Squidex.Areas.Api.Controllers.Users.Models;
using Squidex.Domain.Users;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Commands;
using Squidex.Shared;
using Squidex.Web;

namespace Squidex.Areas.Api.Controllers.Users
{
    [ApiModelValidation(true)]
    public sealed class UserManagementController : ApiController
    {
        private readonly UserManager<IdentityUser> userManager;
        private readonly IUserFactory userFactory;

        public UserManagementController(ICommandBus commandBus, UserManager<IdentityUser> userManager, IUserFactory userFactory)
            : base(commandBus)
        {
            this.userManager = userManager;
            this.userFactory = userFactory;
        }

        [HttpGet]
        [Route("user-management/")]
        [ProducesResponseType(typeof(UsersDto), 200)]
        [ApiPermission(Permissions.AdminUsersRead)]
        public async Task<IActionResult> GetUsers([FromQuery] string query = null, [FromQuery] int skip = 0, [FromQuery] int take = 10)
        {
            var taskForItems = userManager.QueryByEmailAsync(query, take, skip);
            var taskForCount = userManager.CountByEmailAsync(query);

            await Task.WhenAll(taskForItems, taskForCount);

            var response = UsersDto.FromResults(taskForItems.Result, taskForCount.Result, this);

            return Ok(response);
        }

        [HttpGet]
        [Route("user-management/{id}/")]
        [ProducesResponseType(typeof(UserDto), 201)]
        [ApiPermission(Permissions.AdminUsersRead)]
        public async Task<IActionResult> GetUser(string id)
        {
            var user = await userManager.FindByIdWithClaimsAsync(id);

            if (user == null)
            {
                return NotFound();
            }

            var response = UserDto.FromUser(user, this);

            return Ok(response);
        }

        [HttpPost]
        [Route("user-management/")]
        [ProducesResponseType(typeof(UserDto), 201)]
        [ApiPermission(Permissions.AdminUsersCreate)]
        public async Task<IActionResult> PostUser([FromBody] CreateUserDto request)
        {
            var user = await userManager.CreateAsync(userFactory, request.ToValues());

            var response = UserDto.FromUser(user, this);

            return Ok(response);
        }

        [HttpPut]
        [Route("user-management/{id}/")]
        [ProducesResponseType(typeof(UserDto), 201)]
        [ApiPermission(Permissions.AdminUsersUpdate)]
        public async Task<IActionResult> PutUser(string id, [FromBody] UpdateUserDto request)
        {
            var user = await userManager.UpdateAsync(id, request.ToValues());

            var response = UserDto.FromUser(user, this);

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
                throw new ValidationException("Locking user failed.", new ValidationError("You cannot lock yourself."));
            }

            var user = await userManager.LockAsync(id);

            var response = UserDto.FromUser(user, this);

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
                throw new ValidationException("Unlocking user failed.", new ValidationError("You cannot unlock yourself."));
            }

            var user = await userManager.UnlockAsync(id);

            var response = UserDto.FromUser(user, this);

            return Ok(response);
        }
    }
}
