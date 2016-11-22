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
using NSwag.Annotations;
using Squidex.Infrastructure.Reflection;
using Squidex.Modules.Api.Users.Models;
using Squidex.Pipeline;
using Squidex.Read.Users.Repositories;

namespace Squidex.Modules.Api.Users
{
    [Authorize]
    [ApiExceptionFilter]
    [SwaggerTag("Users", Description = "Readonly API to retrieve information about squidex users.")]
    public class UsersController : Controller
    {
        private readonly IUserRepository userRepository;

        public UsersController(IUserRepository userRepository)
        {
            this.userRepository = userRepository;
        }

        /// <summary>
        /// Get users by query.
        /// </summary>
        /// <param name="query">The query to search the user by email address. Case invariant.</param>
        /// <remarks>
        /// Search the user by query that contains the email address or the part of the email address.
        /// </remarks>
        [HttpGet]
        [Route("users")]
        [SwaggerTags("Users")]
        [DescribedResponseType(200, typeof(UserDto[]), "Users returned.")]
        public async Task<IActionResult> GetUsers(string query)
        {
            var entities = await userRepository.FindUsersByQuery(query ?? string.Empty);

            var model = entities.Select(x => SimpleMapper.Map(x, new UserDto())).ToList();

            return Ok(model);
        }

        /// <summary>
        /// Get user by id.
        /// </summary>
        /// <param name="id">The id of the user (GUID).</param>
        [HttpGet]
        [Route("users/{id}/")]
        [SwaggerTags("Users")]
        [DescribedResponseType(200, typeof(UserDto), "User found.")]
        [DescribedResponseType(404, typeof(void), "User not found.")]
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
