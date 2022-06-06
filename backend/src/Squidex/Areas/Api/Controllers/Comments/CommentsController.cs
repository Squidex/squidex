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
using Squidex.Shared;
using Squidex.Web;

namespace Squidex.Areas.Api.Controllers.Comments
{
    /// <summary>
    /// Manages comments for any kind of app resource.
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
        /// <returns>
        /// 200 => Watching users returned.
        /// 404 => App not found.
        /// </returns>
        [HttpGet]
        [Route("apps/{app}/watching/{*resource}")]
        [ProducesResponseType(typeof(string[]), StatusCodes.Status200OK)]
        [ApiPermissionOrAnonymous]
        [ApiCosts(0)]
        public async Task<IActionResult> GetWatchingUsers(string app, string? resource = null)
        {
            var result = await watchingService.GetWatchingUsersAsync(App.Id, resource ?? "all", UserId());

            return Ok(result);
        }

        /// <summary>
        /// Get all comments.
        /// </summary>
        /// <param name="app">The name of the app.</param>
        /// <param name="commentsId">The id of the comments.</param>
        /// <param name="version">The current version.</param>
        /// <remarks>
        /// When passing in a version you can retrieve all updates since then.
        /// </remarks>
        /// <returns>
        /// 200 => Comments returned.
        /// 404 => App not found.
        /// </returns>
        [HttpGet]
        [Route("apps/{app}/comments/{commentsId}")]
        [ProducesResponseType(typeof(CommentsDto), StatusCodes.Status200OK)]
        [ApiPermissionOrAnonymous(Permissions.AppCommentsRead)]
        [ApiCosts(0)]
        public async Task<IActionResult> GetComments(string app, DomainId commentsId, [FromQuery] long version = EtagVersion.Any)
        {
            var result = await commentsLoader.GetCommentsAsync(commentsId, version);

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
        /// <param name="commentsId">The id of the comments.</param>
        /// <param name="request">The comment object that needs to created.</param>
        /// <returns>
        /// 201 => Comment created.
        /// 400 => Comment request not valid.
        /// 404 => App not found.
        /// </returns>
        [HttpPost]
        [Route("apps/{app}/comments/{commentsId}")]
        [ProducesResponseType(typeof(CommentDto), 201)]
        [ApiPermissionOrAnonymous(Permissions.AppCommentsCreate)]
        [ApiCosts(0)]
        public async Task<IActionResult> PostComment(string app, DomainId commentsId, [FromBody] UpsertCommentDto request)
        {
            var command = request.ToCreateCommand(commentsId);

            await CommandBus.PublishAsync(command);

            var response = CommentDto.FromDomain(command);

            return CreatedAtAction(nameof(GetComments), new { app, commentsId }, response);
        }

        /// <summary>
        /// Update a comment.
        /// </summary>
        /// <param name="app">The name of the app.</param>
        /// <param name="commentsId">The id of the comments.</param>
        /// <param name="commentId">The id of the comment.</param>
        /// <param name="request">The comment object that needs to updated.</param>
        /// <returns>
        /// 204 => Comment updated.
        /// 400 => Comment request not valid.
        /// 404 => Comment or app not found.
        /// </returns>
        [HttpPut]
        [Route("apps/{app}/comments/{commentsId}/{commentId}")]
        [ApiPermissionOrAnonymous(Permissions.AppCommentsUpdate)]
        [ApiCosts(0)]
        public async Task<IActionResult> PutComment(string app, DomainId commentsId, DomainId commentId, [FromBody] UpsertCommentDto request)
        {
            var command = request.ToUpdateComment(commentsId, commentId);

            await CommandBus.PublishAsync(command);

            return NoContent();
        }

        /// <summary>
        /// Delete a comment.
        /// </summary>
        /// <param name="app">The name of the app.</param>
        /// <param name="commentsId">The id of the comments.</param>
        /// <param name="commentId">The id of the comment.</param>
        /// <returns>
        /// 204 => Comment deleted.
        /// 404 => Comment or app not found.
        /// </returns>
        [HttpDelete]
        [Route("apps/{app}/comments/{commentsId}/{commentId}")]
        [ApiPermissionOrAnonymous(Permissions.AppCommentsDelete)]
        [ApiCosts(0)]
        public async Task<IActionResult> DeleteComment(string app, DomainId commentsId, DomainId commentId)
        {
            var command = new DeleteComment { CommentsId = commentsId, CommentId = commentId };

            await CommandBus.PublishAsync(command);

            return NoContent();
        }

        private string UserId()
        {
            var subject = User.OpenIdSubject();

            if (string.IsNullOrWhiteSpace(subject))
            {
                throw new DomainForbiddenException(T.Get("common.httpOnlyAsUser"));
            }

            return subject;
        }
    }
}
