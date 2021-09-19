// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

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

        public CommentsController(ICommandBus commandBus, ICommentsLoader commentsLoader)
            : base(commandBus)
        {
            this.commentsLoader = commentsLoader;
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
                return CommentsDto.FromResult(result);
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

            var response = CommentDto.FromCommand(command);

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
    }
}
