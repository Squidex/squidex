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
using Squidex.Shared;
using Squidex.Web;

namespace Squidex.Areas.Api.Controllers.Comments;

/// <summary>
/// Update and query comments for any kind of app resource.
/// </summary>
[ApiExplorerSettings(GroupName = nameof(Comments))]
public sealed class CommentsController : ApiController
{
    private readonly ICommentsLoader commentsLoader;
    private readonly IWatchingService watchingService;

    public CommentsController(ICommandBus commandBus, ICommentsLoader commentsLoader,
        IWatchingService watchingService)
        : base(commandBus)
    {
        this.commentsLoader = commentsLoader;

        this.watchingService = watchingService;
    }

    /// <summary>
    /// Get all watching users..
    /// </summary>
    /// <param name="app">The name of the app.</param>
    /// <param name="resource">The path to the resource.</param>
    /// <response code="200">Watching users returned.</response>.
    /// <response code="404">App not found.</response>.
    [HttpGet]
    [Route("apps/{app}/watching/{*resource}")]
    [ProducesResponseType(typeof(string[]), StatusCodes.Status200OK)]
    [ApiPermissionOrAnonymous]
    [ApiCosts(0)]
    public async Task<IActionResult> GetWatchingUsers(string app, string? resource = null)
    {
        var result = await watchingService.GetWatchingUsersAsync(App.Id, resource, UserId, HttpContext.RequestAborted);

        return Ok(result);
    }

    /// <summary>
    /// Get all comments.
    /// </summary>
    /// <param name="app">The name of the app.</param>
    /// <param name="commentsId">The ID of the comments.</param>
    /// <param name="version">The current version.</param>
    /// <remarks>
    /// When passing in a version you can retrieve all updates since then.
    /// </remarks>
    /// <response code="200">Comments returned.</response>.
    /// <response code="404">App not found.</response>.
    [HttpGet]
    [Route("apps/{app}/comments/{commentsId}")]
    [ProducesResponseType(typeof(CommentsDto), StatusCodes.Status200OK)]
    [ApiPermissionOrAnonymous(PermissionIds.AppCommentsRead)]
    [ApiCosts(0)]
    public async Task<IActionResult> GetComments(string app, DomainId commentsId, [FromQuery] long version = EtagVersion.Any)
    {
        var result = await commentsLoader.GetCommentsAsync(Id(commentsId), version, HttpContext.RequestAborted);

        var response = Deferred.Response(() =>
        {
            return CommentsDto.FromDomain(result);
        });

        Response.Headers[HeaderNames.ETag] = result.Version.ToString(CultureInfo.InvariantCulture);

        return Ok(response);
    }

    /// <summary>
    /// Create a new comment.
    /// </summary>
    /// <param name="app">The name of the app.</param>
    /// <param name="commentsId">The ID of the comments.</param>
    /// <param name="request">The comment object that needs to created.</param>
    /// <response code="201">Comment created.</response>.
    /// <response code="400">Comment request not valid.</response>.
    /// <response code="404">App not found.</response>.
    [HttpPost]
    [Route("apps/{app}/comments/{commentsId}")]
    [ProducesResponseType(typeof(CommentDto), 201)]
    [ApiPermissionOrAnonymous(PermissionIds.AppCommentsCreate)]
    [ApiCosts(0)]
    public async Task<IActionResult> PostComment(string app, DomainId commentsId, [FromBody] UpsertCommentDto request)
    {
        var command = request.ToCreateCommand(commentsId);

        await CommandBus.PublishAsync(command, HttpContext.RequestAborted);

        var response = CommentDto.FromDomain(command);

        return CreatedAtAction(nameof(GetComments), new { app, commentsId }, response);
    }

    /// <summary>
    /// Update a comment.
    /// </summary>
    /// <param name="app">The name of the app.</param>
    /// <param name="commentsId">The ID of the comments.</param>
    /// <param name="commentId">The ID of the comment.</param>
    /// <param name="request">The comment object that needs to updated.</param>
    /// <response code="204">Comment updated.</response>.
    /// <response code="400">Comment request not valid.</response>.
    /// <response code="404">Comment or app not found.</response>.
    [HttpPut]
    [Route("apps/{app}/comments/{commentsId}/{commentId}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ApiPermissionOrAnonymous(PermissionIds.AppCommentsUpdate)]
    [ApiCosts(0)]
    public async Task<IActionResult> PutComment(string app, DomainId commentsId, DomainId commentId, [FromBody] UpsertCommentDto request)
    {
        var command = request.ToUpdateComment(commentsId, commentId);

        await CommandBus.PublishAsync(command, HttpContext.RequestAborted);

        return NoContent();
    }

    /// <summary>
    /// Delete a comment.
    /// </summary>
    /// <param name="app">The name of the app.</param>
    /// <param name="commentsId">The ID of the comments.</param>
    /// <param name="commentId">The ID of the comment.</param>
    /// <response code="204">Comment deleted.</response>.
    /// <response code="404">Comment or app not found.</response>.
    [HttpDelete]
    [Route("apps/{app}/comments/{commentsId}/{commentId}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ApiPermissionOrAnonymous(PermissionIds.AppCommentsDelete)]
    [ApiCosts(0)]
    public async Task<IActionResult> DeleteComment(string app, DomainId commentsId, DomainId commentId)
    {
        var command = new DeleteComment
        {
            CommentsId = commentsId,
            CommentId = commentId
        };

        await CommandBus.PublishAsync(command, HttpContext.RequestAborted);

        return NoContent();
    }

    private DomainId Id(DomainId commentsId)
    {
        return DomainId.Combine(App.Id, commentsId);
    }
}
