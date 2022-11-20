// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Globalization;
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

namespace Squidex.Areas.Api.Controllers.Comments.Notifications;

/// <summary>
/// Update and query user notifications.
/// </summary>
[ApiExplorerSettings(GroupName = nameof(Notifications))]
public sealed class UserNotificationsController : ApiController
{
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
    /// <response code="200">All comments returned.</response>.
    [HttpGet]
    [Route("users/{userId}/notifications")]
    [ProducesResponseType(typeof(CommentsDto), StatusCodes.Status200OK)]
    [ApiPermission]
    public async Task<IActionResult> GetNotifications(DomainId userId, [FromQuery] long version = EtagVersion.Any)
    {
        CheckPermissions(userId);

        var result = await commentsLoader.GetCommentsAsync(userId, version, HttpContext.RequestAborted);

        var response = Deferred.Response(() =>
        {
            return CommentsDto.FromDomain(result);
        });

        Response.Headers[HeaderNames.ETag] = result.Version.ToString(CultureInfo.InvariantCulture);

        return Ok(response);
    }

    /// <summary>
    /// Delete a notification.
    /// </summary>
    /// <param name="userId">The user id.</param>
    /// <param name="commentId">The ID of the comment.</param>
    /// <response code="204">Comment deleted.</response>.
    /// <response code="404">Comment not found.</response>.
    [HttpDelete]
    [Route("users/{userId}/notifications/{commentId}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ApiPermission]
    public async Task<IActionResult> DeleteComment(DomainId userId, DomainId commentId)
    {
        CheckPermissions(userId);

        var commmand = new DeleteComment
        {
            AppId = CommentsCommand.NoApp,
            CommentsId = userId,
            CommentId = commentId
        };

        await CommandBus.PublishAsync(commmand, HttpContext.RequestAborted);

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
