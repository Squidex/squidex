// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Squidex.Areas.Api.Controllers.Users.Models;
using Squidex.Domain.Users;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Commands;
using Squidex.Infrastructure.Security;
using Squidex.Pipeline;
using Squidex.Shared;

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
        [ApiPermission(Permissions.AdminUsersRead)]
        public async Task<IActionResult> GetUsers([FromQuery] string query = null, [FromQuery] int skip = 0, [FromQuery] int take = 10)
        {
            var taskForItems = userManager.QueryByEmailAsync(query, take, skip);
            var taskForCount = userManager.CountByEmailAsync(query);

            await Task.WhenAll(taskForItems, taskForCount);

            var response = new UsersDto
            {
                Total = taskForCount.Result,
                Items = taskForItems.Result.Select(UserDto.FromUser).ToArray()
            };

            return Ok(response);
        }

        [HttpGet]
        [Route("user-management/{id}/")]
        [ApiPermission(Permissions.AdminUsersRead)]
        public async Task<IActionResult> GetUser(string id)
        {
            var entity = await userManager.FindByIdWithClaimsAsync(id);

            if (entity == null)
            {
                return NotFound();
            }

            var response = UserDto.FromUser(entity);

            return Ok(response);
        }

        [HttpPost]
        [Route("user-management/")]
        [ApiPermission(Permissions.AdminUsersCreate)]
        public async Task<IActionResult> PostUser([FromBody] CreateUserDto request)
        {
            var user = await userManager.CreateAsync(userFactory, request.ToValues());

            var response = new UserCreatedDto { Id = user.Id };

            return Ok(response);
        }

        [HttpPut]
        [Route("user-management/{id}/")]
        [ApiPermission(Permissions.AdminUsersUpdate)]
        public async Task<IActionResult> PutUser(string id, [FromBody] UpdateUserDto request)
        {
            await userManager.UpdateAsync(id, request.ToValues());

            return NoContent();
        }

        [HttpPut]
        [Route("user-management/{id}/lock/")]
        [ApiPermission(Permissions.AdminUsersLock)]
        public async Task<IActionResult> LockUser(string id)
        {
            if (IsSelf(id))
            {
                throw new ValidationException("Locking user failed.", new ValidationError("You cannot lock yourself."));
            }

            await userManager.LockAsync(id);

            return NoContent();
        }

        [HttpPut]
        [Route("user-management/{id}/unlock/")]
        [ApiPermission(Permissions.AdminUsersUnlock)]
        public async Task<IActionResult> UnlockUser(string id)
        {
            if (IsSelf(id))
            {
                throw new ValidationException("Unlocking user failed.", new ValidationError("You cannot unlock yourself."));
            }

            await userManager.UnlockAsync(id);

            return NoContent();
        }

        private bool IsSelf(string id)
        {
            var subject = User.OpenIdSubject();

            return string.Equals(subject, id, StringComparison.OrdinalIgnoreCase);
        }
    }
}
