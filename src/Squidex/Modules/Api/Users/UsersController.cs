// =========================================================================
//  UsersController.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Squidex.Infrastructure.Reflection;
using Squidex.Modules.Api.Users.Models;
using Squidex.Pipeline;
using Squidex.Read.Users.Repositories;

namespace Squidex.Modules.Api.Users
{
    [Authorize]
    [ApiExceptionFilter]
    public class UsersController : Controller
    {
        private readonly IUserRepository userRepository;

        public UsersController(IUserRepository userRepository)
        {
            this.userRepository = userRepository;
        }

        [HttpGet]
        [Route("users")]
        public async Task<IActionResult> GetUsers(string email)
        {
            var entities = await userRepository.FindUsersByEmail(email);

            var model = entities.Select(x => SimpleMapper.Map(x, new UserDto())).ToList();

            return Ok(model);
        }

        [HttpGet]
        [Route("users/{id}/")]
        public async Task<IActionResult> GetUser(string id)
        {
            var entity = await userRepository.FindUserByIdAsync(id);

            if (entity == null)
            {
                return NotFound();
            }

            var model = SimpleMapper.Map(entity, new UserDto());

            return Ok(model);
        } 
    }
}
