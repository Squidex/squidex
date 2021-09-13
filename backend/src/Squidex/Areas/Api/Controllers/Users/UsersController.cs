// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Net.Http.Headers;
using Squidex.Areas.Api.Controllers.Users.Models;
using Squidex.Domain.Users;
using Squidex.Infrastructure.Commands;
using Squidex.Log;
using Squidex.Shared.Identity;
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
        private readonly IHttpClientFactory httpClientFactory;
        private readonly IUserPictureStore userPictureStore;
        private readonly IUserResolver userResolver;
        private readonly ISemanticLog log;

        static UsersController()
        {
            var assembly = typeof(UsersController).Assembly;

            using (var resourceStream = assembly.GetManifestResourceStream("Squidex.Areas.Api.Controllers.Users.Assets.Avatar.png"))
            {
                AvatarBytes = new byte[resourceStream!.Length];

                _ = resourceStream.Read(AvatarBytes, 0, AvatarBytes.Length);
            }
        }

        public UsersController(
            ICommandBus commandBus,
            IHttpClientFactory httpClientFactory,
            IUserPictureStore userPictureStore,
            IUserResolver userResolver,
            ISemanticLog log)
            : base(commandBus)
        {
            this.httpClientFactory = httpClientFactory;
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
        [ProducesResponseType(typeof(ResourcesDto), StatusCodes.Status200OK)]
        [ApiPermission]
        public IActionResult GetUserResources()
        {
            var response = ResourcesDto.FromResources(Resources);

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
        [ProducesResponseType(typeof(UserDto[]), StatusCodes.Status200OK)]
        [ApiPermission]
        public async Task<IActionResult> GetUsers(string query)
        {
            try
            {
                var users = await userResolver.QueryByEmailAsync(query, HttpContext.RequestAborted);

                var response = users.Where(x => !x.Claims.IsHidden()).Select(x => UserDto.FromUser(x, Resources)).ToArray();

                return Ok(response);
            }
            catch (Exception ex)
            {
                log.LogError(ex, w => w
                    .WriteProperty("action", nameof(GetUsers))
                    .WriteProperty("status", "Failed"));
            }

            return Ok(Array.Empty<UserDto>());
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
        [ProducesResponseType(typeof(UserDto), StatusCodes.Status200OK)]
        [ApiPermission]
        public async Task<IActionResult> GetUser(string id)
        {
            try
            {
                var entity = await userResolver.FindByIdAsync(id, HttpContext.RequestAborted);

                if (entity != null)
                {
                    var response = UserDto.FromUser(entity, Resources);

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
        [ProducesResponseType(typeof(FileResult), StatusCodes.Status200OK)]
        [ResponseCache(Duration = 300)]
        public async Task<IActionResult> GetUserPicture(string id)
        {
            try
            {
                var entity = await userResolver.FindByIdAsync(id, HttpContext.RequestAborted);

                if (entity != null)
                {
                    if (entity.Claims.IsPictureUrlStored())
                    {
                        var callback = new FileCallback(async (body, range, ct) =>
                        {
                            try
                            {
                                await userPictureStore.DownloadAsync(entity.Id, body, ct);
                            }
                            catch
                            {
                                await body.WriteAsync(AvatarBytes, ct);
                            }
                        });

                        return new FileCallbackResult("image/png", callback);
                    }

                    using (var client = httpClientFactory.CreateClient())
                    {
                        var url = entity.Claims.PictureNormalizedUrl();

                        if (!string.IsNullOrWhiteSpace(url))
                        {
                            var response = await client.GetAsync(url, HttpCompletionOption.ResponseHeadersRead, HttpContext.RequestAborted);

                            if (response.IsSuccessStatusCode)
                            {
                                var contentType = response.Content.Headers.ContentType?.ToString();

                                var etag = response.Headers.ETag;

                                var result = new FileStreamResult(await response.Content.ReadAsStreamAsync(HttpContext.RequestAborted), contentType);

                                if (!string.IsNullOrWhiteSpace(etag?.Tag))
                                {
                                    result.EntityTag = new EntityTagHeaderValue(etag.Tag, etag.IsWeak);
                                }

                                return result;
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
