﻿// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Net.Http.Headers;
using Squidex.Areas.Api.Controllers.Comments.Models;
using Squidex.Domain.Apps.Entities.Comments;
using Squidex.Domain.Apps.Entities.Comments.Commands;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Commands;
using Squidex.Infrastructure.Security;
using Squidex.Web;

namespace Squidex.Areas.Api.Controllers.Comments.Notifications
{
    /// <summary>
    /// Manages user notifications.
    /// </summary>
    [ApiExplorerSettings(GroupName = nameof(Notifications))]
    public sealed class UserNotificationsController : ApiController
    {
        private static readonly NamedId<Guid> NoApp = NamedId.Of(Guid.Empty, "none");
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
        [ProducesResponseType(typeof(CommentsDto), 200)]
        [ApiPermission]
        public async Task<IActionResult> GetNotifications(string userId, [FromQuery] long version = EtagVersion.Any)
        {
            CheckPermissions(userId);

            var result = await commentsLoader.GetCommentsAsync(userId, version);

            var response = Deferred.Response(() =>
            {
                return CommentsDto.FromResult(result);
            });

            Response.Headers[HeaderNames.ETag] = result.Version.ToString();

            return Ok(response);
        }

        /// <summary>
        /// Deletes the notification.
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
        public async Task<IActionResult> DeleteComment(string userId, Guid commentId)
        {
            CheckPermissions(userId);

            await CommandBus.PublishAsync(new DeleteComment
            {
                AppId = NoApp,
                CommentsId = userId,
                CommentId = commentId
            });

            return NoContent();
        }

        private void CheckPermissions(string userId)
        {
            if (!string.Equals(userId, User.OpenIdSubject()))
            {
                throw new DomainForbiddenException("You can only access your notifications.");
            }
        }
    }
}
