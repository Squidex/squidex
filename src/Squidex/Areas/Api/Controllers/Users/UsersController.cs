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
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using NSwag.Annotations;
using Squidex.Areas.Api.Controllers.Users.Models;
using Squidex.Domain.Users;
using Squidex.Infrastructure.Commands;
using Squidex.Infrastructure.Log;
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
        private readonly IUserPictureStore userPictureStore;
        private readonly IUserResolver userResolver;
        private readonly ISemanticLog log;

        static UsersController()
        {
            var assembly = typeof(UsersController).GetTypeInfo().Assembly;

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
        [ProducesResponseType(typeof(PublicUserDto[]), 200)]
        public async Task<IActionResult> GetUsers(string query)
        {
            try
            {
                var entities = await userResolver.QueryByEmailAsync(query);

                var models = entities.Where(x => !x.IsHidden()).Select(UserDto.FromUser).ToArray();

                return Ok(models);
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
        [ApiAuthorize]
        [HttpGet]
        [Route("users/{id}/")]
        [ProducesResponseType(typeof(PublicUserDto), 200)]
        public async Task<IActionResult> GetUser(string id)
        {
            try
            {
                var entity = await userResolver.FindByIdOrEmailAsync(id);

                if (entity != null)
                {
                    var response = UserDto.FromUser(entity);

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
        [ProducesResponseType(200)]
        [ResponseCache(Duration = 3600)]
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
