// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Globalization;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Net.Http.Headers;
using Squidex.Areas.Api.Controllers.Comments.Models;
using Squidex.Domain.Apps.Entities.Comments;
using Squidex.Domain.Apps.Entities.Comments.Commands;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Commands;
using Squidex.Infrastructure.Security;
using Squidex.Infrastructure.Translations;
using Squidex.Web;

namespace Squidex.Areas.Api.Controllers.Comments.Notifications
{
    /// <summary>
    /// Manages user notifications.
    /// </summary>
    [ApiExplorerSettings(GroupName = nameof(Notifications))]
    public sealed class UserNotificationsController : ApiController
    {
        private static readonly NamedId<DomainId> NoApp = NamedId.Of(DomainId.Empty, "none");
        private readonly ICommentsLoader commentsLoader;

        public UserNotificationsController(ICommandBus commandBus, ICommentsLoader commentsLoader)
            : base(commandBus)
        {
            this.commentsLoader = commentsLoader;
        }

        /// <summary>
        /// Get all notifications.
        /// </summary>
        /// <param name="userId">The user id.</param>
        /// <param name="version">The current version.</param>
        /// <remarks>
        /// When passing in a version you can retrieve all updates since then.
        /// </remarks>
        /// <returns>
        /// 200 => All comments returned.
        /// </returns>
        [HttpGet]
        [Route("users/{userId}/notifications")]
        [ProducesResponseType(typeof(CommentsDto), StatusCodes.Status200OK)]
        [ApiPermission]
        public async Task<IActionResult> GetNotifications(DomainId userId, [FromQuery] long version = EtagVersion.Any)
        {
            CheckPermissions(userId);

            var result = await commentsLoader.GetCommentsAsync(userId, version);

            var response = Deferred.Response(() =>
            {
                return CommentsDto.FromResult(result);
            });

            Response.Headers[HeaderNames.ETag] = result.Version.ToString(CultureInfo.InvariantCulture);

            return Ok(response);
        }

        /// <summary>
        /// Delete a notification.
        /// </summary>
        /// <param name="userId">The user id.</param>
        /// <param name="commentId">The id of the comment.</param>
        /// <returns>
        /// 204 => Comment deleted.
        /// 404 => Comment not found.
        /// </returns>
        [HttpDelete]
        [Route("users/{userId}/notifications/{commentId}")]
        [ApiPermission]
        public async Task<IActionResult> DeleteComment(DomainId userId, DomainId commentId)
        {
            CheckPermissions(userId);

            var commmand = new DeleteComment
            {
                AppId = NoApp,
                CommentsId = userId,
                CommentId = commentId
            };

            await CommandBus.PublishAsync(commmand);

            return NoContent();
        }

        private void CheckPermissions(DomainId userId)
        {
            if (!string.Equals(userId.ToString(), User.OpenIdSubject(), StringComparison.Ordinal))
            {
                throw new DomainForbiddenException(T.Get("comments.noPermissions"));
            }
        }
    }
}
