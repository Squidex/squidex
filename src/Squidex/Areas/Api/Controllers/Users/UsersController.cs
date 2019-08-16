// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Squidex.Areas.Api.Controllers.Users.Models;
using Squidex.Domain.Users;
using Squidex.Infrastructure.Commands;
using Squidex.Infrastructure.Log;
using Squidex.Shared.Users;
using Squidex.Web;

namespace Squidex.Areas.Api.Controllers.Users
{
    /// <summary>
    /// Readonly API to retrieve information about squidex users.
    /// </summary>
    [ApiExplorerSettings(GroupName = nameof(Users))]
    public sealed class UsersController : ApiController
    {
        private static readonly byte[] AvatarBytes;
        private readonly IUserPictureStore userPictureStore;
        private readonly IUserResolver userResolver;
        private readonly ISemanticLog log;

        static UsersController()
        {
            var assembly = typeof(UsersController).Assembly;

            using (var avatarStream = assembly.GetManifestResourceStream("Squidex.Areas.Api.Controllers.Users.Assets.Avatar.png"))
            {
                AvatarBytes = new byte[avatarStream.Length];

                avatarStream.Read(AvatarBytes, 0, AvatarBytes.Length);
            }
        }

        public UsersController(
            ICommandBus commandBus,
            IUserPictureStore userPictureStore,
            IUserResolver userResolver,
            ISemanticLog log)
            : base(commandBus)
        {
            this.userPictureStore = userPictureStore;
            this.userResolver = userResolver;

            this.log = log;
        }

        /// <summary>
        /// Get the user resources.
        /// </summary>
        /// <returns>
        /// 200 => User resources returned.
        /// </returns>
        [HttpGet]
        [Route("/")]
        [ProducesResponseType(typeof(ResourcesDto), 200)]
        [ApiPermission]
        public IActionResult GetUserResources()
        {
            var response = ResourcesDto.FromController(this);

            return Ok(response);
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
        [HttpGet]
        [Route("users/")]
        [ProducesResponseType(typeof(UserDto[]), 200)]
        [ApiPermission]
        public async Task<IActionResult> GetUsers(string query)
        {
            try
            {
                var users = await userResolver.QueryByEmailAsync(query);

                var response = users.Where(x => !x.IsHidden()).Select(x => UserDto.FromUser(x, this)).ToArray();

                return Ok(response);
            }
            catch (Exception ex)
            {
                log.LogError(ex, w => w
                    .WriteProperty("action", nameof(GetUsers))
                    .WriteProperty("status", "Failed"));
            }

            return Ok(new UserDto[0]);
        }

        /// <summary>
        /// Get user by id.
        /// </summary>
        /// <param name="id">The id of the user (GUID).</param>
        /// <returns>
        /// 200 => User found.
        /// 404 => User not found.
        /// </returns>
        [HttpGet]
        [Route("users/{id}/")]
        [ProducesResponseType(typeof(UserDto), 200)]
        [ApiPermission]
        public async Task<IActionResult> GetUser(string id)
        {
            try
            {
                var entity = await userResolver.FindByIdOrEmailAsync(id);

                if (entity != null)
                {
                    var response = UserDto.FromUser(entity, this);

                    return Ok(response);
                }
            }
            catch (Exception ex)
            {
                log.LogError(ex, w => w
                    .WriteProperty("action", nameof(GetUser))
                    .WriteProperty("status", "Failed"));
            }

            return NotFound();
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
        [ProducesResponseType(typeof(FileResult), 200)]
        [ResponseCache(Duration = 300)]
        public async Task<IActionResult> GetUserPicture(string id)
        {
            try
            {
                var entity = await userResolver.FindByIdOrEmailAsync(id);

                if (entity != null)
                {
                    if (entity.IsPictureUrlStored())
                    {
                        return new FileStreamResult(await userPictureStore.DownloadAsync(entity.Id), "image/png");
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
                }
            }
            catch (Exception ex)
            {
                log.LogError(ex, w => w
                    .WriteProperty("action", nameof(GetUser))
                    .WriteProperty("status", "Failed"));
            }

            return new FileStreamResult(new MemoryStream(AvatarBytes), "image/png");
        }
    }
}
