// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.IO;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using NSwag.Annotations;
using Squidex.Areas.Api.Controllers.Users.Models;
using Squidex.Domain.Users;
using Squidex.Infrastructure.Commands;
using Squidex.Infrastructure.Reflection;
using Squidex.Pipeline;
using Squidex.Shared.Users;

namespace Squidex.Areas.Api.Controllers.Users
{
    /// <summary>
    /// Readonly API to retrieve information about squidex users.
    /// </summary>
    [ApiExceptionFilter]
    [SwaggerTag(nameof(Users))]
    public sealed class UsersController : ApiController
    {
        private static readonly byte[] AvatarBytes;
        private readonly UserManager<IUser> userManager;
        private readonly IUserPictureStore userPictureStore;

        static UsersController()
        {
            var assembly = typeof(UsersController).GetTypeInfo().Assembly;

            using (var avatarStream = assembly.GetManifestResourceStream("Squidex.Areas.Api.Controllers.Users.Assets.Avatar.png"))
            {
                AvatarBytes = new byte[avatarStream.Length];

                avatarStream.Read(AvatarBytes, 0, AvatarBytes.Length);
            }
        }

        public UsersController(ICommandBus commandBus, UserManager<IUser> userManager, IUserPictureStore userPictureStore)
            : base(commandBus)
        {
            this.userManager = userManager;
            this.userPictureStore = userPictureStore;
        }

        /// <summary>
        /// Get users by query.
        /// </summary>
        /// <param name="query">The query to search the user by email address. Case invariant.</param>
        /// <remarks>
        /// Search the user by query that contains the email address or the part of the email address.
        /// </remarks>
        /// <returns>
        /// 200 => Users returned.
        /// </returns>
        [ApiAuthorize]
        [HttpGet]
        [Route("users/")]
        [ProducesResponseType(typeof(UserDto[]), 200)]
        public async Task<IActionResult> GetUsers(string query)
        {
            var entities = await userManager.QueryByEmailAsync(query ?? string.Empty);

            var models = entities.Select(x => SimpleMapper.Map(x, new UserDto { DisplayName = x.DisplayName(), PictureUrl = x.PictureUrl() })).ToArray();

            return Ok(models);
        }

        /// <summary>
        /// Get user by id.
        /// </summary>
        /// <param name="id">The id of the user (GUID).</param>
        /// <returns>
        /// 200 => User found.
        /// 404 => User not found.
        /// </returns>
        [ApiAuthorize]
        [HttpGet]
        [Route("users/{id}/")]
        [ProducesResponseType(typeof(UserDto), 200)]
        public async Task<IActionResult> GetUser(string id)
        {
            var entity = await userManager.FindByIdAsync(id);

            if (entity == null)
            {
                return NotFound();
            }

            var response = SimpleMapper.Map(entity, new UserDto { DisplayName = entity.DisplayName(), PictureUrl = entity.PictureUrl() });

            return Ok(response);
        }

        /// <summary>
        /// Get user picture by id.
        /// </summary>
        /// <param name="id">The id of the user (GUID).</param>
        /// <returns>
        /// 200 => User found and image or fallback returned.
        /// 404 => User not found.
        /// </returns>
        [HttpGet]
        [Route("users/{id}/picture/")]
        [ProducesResponseType(200)]
        public async Task<IActionResult> GetUserPicture(string id)
        {
            var entity = await userManager.FindByIdAsync(id);

            if (entity == null)
            {
                return NotFound();
            }

            try
            {
                if (entity.IsPictureUrlStored())
                {
                    return new FileStreamResult(await userPictureStore.DownloadAsync(entity.Id), "image/png");
                }
            }
            catch
            {
                return new FileStreamResult(new MemoryStream(AvatarBytes), "image/png");
            }

            using (var client = new HttpClient())
            {
                var url = entity.PictureNormalizedUrl();

                if (!string.IsNullOrWhiteSpace(url))
                {
                    var response = await client.GetAsync(url);

                    if (response.IsSuccessStatusCode)
                    {
                        var contentType = response.Content.Headers.ContentType.ToString();

                        return new FileStreamResult(await response.Content.ReadAsStreamAsync(), contentType);
                    }
                }
            }

            return new FileStreamResult(new MemoryStream(AvatarBytes), "image/png");
        }
    }
}
