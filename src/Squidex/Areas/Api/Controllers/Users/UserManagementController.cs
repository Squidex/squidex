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
using NSwag.Annotations;
using Squidex.Areas.Api.Controllers.Users.Models;
using Squidex.Domain.Users;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Commands;
using Squidex.Infrastructure.Reflection;
using Squidex.Infrastructure.Security;
using Squidex.Pipeline;
using Squidex.Shared.Users;

namespace Squidex.Areas.Api.Controllers.Users
{
    [ApiAuthorize]
    [ApiExceptionFilter]
    [ApiModelValidation]
    [MustBeAdministrator]
    [SwaggerIgnore]
    public sealed class UserManagementController : ApiController
    {
        private readonly UserManager<IUser> userManager;
        private readonly IUserFactory userFactory;

        public UserManagementController(ICommandBus commandBus, UserManager<IUser> userManager, IUserFactory userFactory)
            : base(commandBus)
        {
            this.userManager = userManager;
            this.userFactory = userFactory;
        }

        [HttpGet]
        [Route("user-management/")]
        [ApiCosts(0)]
        public async Task<IActionResult> GetUsers([FromQuery] string query = null, [FromQuery] int skip = 0, [FromQuery] int take = 10)
        {
            var taskForItems = userManager.QueryByEmailAsync(query, take, skip);
            var taskForCount = userManager.CountByEmailAsync(query);

            await Task.WhenAll(taskForItems, taskForCount);

            var response = new UsersDto
            {
                Total = taskForCount.Result,
                Items = taskForItems.Result.Select(Map).ToArray()
            };

            return Ok(response);
        }

        [HttpGet]
        [Route("user-management/{id}/")]
        [ApiCosts(0)]
        public async Task<IActionResult> GetUser(string id)
        {
            var entity = await userManager.FindByIdAsync(id);

            if (entity == null)
            {
                return NotFound();
            }

            var response = Map(entity);

            return Ok(response);
        }

        [HttpPost]
        [Route("user-management/")]
        [ApiCosts(0)]
        public async Task<IActionResult> PostUser([FromBody] CreateUserDto request)
        {
            var user = await userManager.CreateAsync(userFactory, request.Email, request.DisplayName, request.Password);

            var response = new UserCreatedDto { Id = user.Id, PictureUrl = user.PictureUrl() };

            return Ok(response);
        }

        [HttpPut]
        [Route("user-management/{id}/")]
        [ApiCosts(0)]
        public async Task<IActionResult> PutUser(string id, [FromBody] UpdateUserDto request)
        {
            await userManager.UpdateAsync(id, request.Email, request.DisplayName, request.Password);

            return NoContent();
        }

        [HttpPut]
        [Route("user-management/{id}/lock/")]
        [ApiCosts(0)]
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
        [ApiCosts(0)]
        public async Task<IActionResult> UnlockUser(string id)
        {
            if (IsSelf(id))
            {
                throw new ValidationException("Unlocking user failed.", new ValidationError("You cannot unlock yourself."));
            }

            await userManager.UnlockAsync(id);

            return NoContent();
        }

        private static UserDto Map(IUser user)
        {
            return SimpleMapper.Map(user, new UserDto { DisplayName = user.DisplayName(), PictureUrl = user.PictureUrl() });
        }

        private bool IsSelf(string id)
        {
            var subject = User.OpenIdSubject();

            return string.Equals(subject, id, StringComparison.OrdinalIgnoreCase);
        }
    }
}
