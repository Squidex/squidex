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
using Squidex.Infrastructure.Tasks;
using Squidex.Infrastructure.Translations;
using Squidex.Shared;
using Squidex.Web;

namespace Squidex.Areas.Api.Controllers.Users
{
    [ApiModelValidation(true)]
    public sealed class UserManagementController : ApiController
    {
        private readonly UserManager<IdentityUser> userManager;
        private readonly IUserFactory userFactory;
        private readonly IUserEvents userEvents;

        public UserManagementController(ICommandBus commandBus, UserManager<IdentityUser> userManager, IUserFactory userFactory, IUserEvents userEvents)
            : base(commandBus)
        {
            this.userManager = userManager;
            this.userFactory = userFactory;
            this.userEvents = userEvents;
        }

        [HttpGet]
        [Route("user-management/")]
        [ProducesResponseType(typeof(UsersDto), 200)]
        [ApiPermission(Permissions.AdminUsersRead)]
        public async Task<IActionResult> GetUsers([FromQuery] string? query = null, [FromQuery] int skip = 0, [FromQuery] int take = 10)
        {
            var (items, total) = await AsyncHelper.WhenAll(
                userManager.QueryByEmailAsync(query, take, skip),
                userManager.CountByEmailAsync(query));

            var response = UsersDto.FromResults(items, total, Resources);

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

            var response = UserDto.FromUser(user, Resources);

            return Ok(response);
        }

        [HttpPost]
        [Route("user-management/")]
        [ProducesResponseType(typeof(UserDto), 201)]
        [ApiPermission(Permissions.AdminUsersCreate)]
        public async Task<IActionResult> PostUser([FromBody] CreateUserDto request)
        {
            var user = await userManager.CreateAsync(userFactory, request.ToValues());

            userEvents.OnUserRegistered(user);

            var response = UserDto.FromUser(user, Resources);

            return Ok(response);
        }

        [HttpPut]
        [Route("user-management/{id}/")]
        [ProducesResponseType(typeof(UserDto), 201)]
        [ApiPermission(Permissions.AdminUsersUpdate)]
        public async Task<IActionResult> PutUser(string id, [FromBody] UpdateUserDto request)
        {
            var user = await userManager.UpdateAsync(id, request.ToValues());

            userEvents.OnUserUpdated(user);

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

            var user = await userManager.LockAsync(id);

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

            var user = await userManager.UnlockAsync(id);

            var response = UserDto.FromUser(user, Resources);

            return Ok(response);
        }
    }
}
