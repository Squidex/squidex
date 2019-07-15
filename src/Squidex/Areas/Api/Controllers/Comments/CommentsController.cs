// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Net.Http.Headers;
using Orleans;
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
    /// Manages comments for any kind of resource.
    /// </summary>
    [ApiExplorerSettings(GroupName = nameof(Comments))]
    public sealed class CommentsController : ApiController
    {
        private readonly IGrainFactory grainFactory;

        public CommentsController(ICommandBus commandBus, IGrainFactory grainFactory)
            : base(commandBus)
        {
            this.grainFactory = grainFactory;
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
        /// 200 => All comments returned.
        /// 404 => App not found.
        /// </returns>
        [HttpGet]
        [Route("apps/{app}/comments/{commentsId}")]
        [ProducesResponseType(typeof(CommentsDto), 200)]
        [ApiPermission(Permissions.AppCommon)]
        [ApiCosts(0)]
        public async Task<IActionResult> GetComments(string app, Guid commentsId, [FromQuery] long version = EtagVersion.Any)
        {
            var result = await grainFactory.GetGrain<ICommentGrain>(commentsId).GetCommentsAsync(version);

            var response = Deferred.Response(() =>
            {
                return CommentsDto.FromResult(result);
            });

            Response.Headers[HeaderNames.ETag] = result.Version.ToString();

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
        /// 400 => Comment is not valid.
        /// 404 => App not found.
        /// </returns>
        [HttpPost]
        [Route("apps/{app}/comments/{commentsId}")]
        [ProducesResponseType(typeof(EntityCreatedDto), 201)]
        [ApiPermission(Permissions.AppCommon)]
        [ApiCosts(0)]
        public async Task<IActionResult> PostComment(string app, Guid commentsId, [FromBody] UpsertCommentDto request)
        {
            var command = request.ToCreateCommand(commentsId);

            await CommandBus.PublishAsync(command);

            var response = CommentDto.FromCommand(command);

            return CreatedAtAction(nameof(GetComments), new { commentsId }, response);
        }

        /// <summary>
        /// Updates the comment.
        /// </summary>
        /// <param name="app">The name of the app.</param>
        /// <param name="commentsId">The id of the comments.</param>
        /// <param name="commentId">The id of the comment.</param>
        /// <param name="request">The comment object that needs to updated.</param>
        /// <returns>
        /// 204 => Comment updated.
        /// 400 => Comment text not valid.
        /// 404 => Comment or app not found.
        /// </returns>
        [HttpPut]
        [Route("apps/{app}/comments/{commentsId}/{commentId}")]
        [ApiPermission(Permissions.AppCommon)]
        [ApiCosts(0)]
        public async Task<IActionResult> PutComment(string app, Guid commentsId, Guid commentId, [FromBody] UpsertCommentDto request)
        {
            await CommandBus.PublishAsync(request.ToUpdateComment(commentsId, commentId));

            return NoContent();
        }

        /// <summary>
        /// Deletes the comment.
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
        [ApiPermission(Permissions.AppCommon)]
        [ApiCosts(0)]
        public async Task<IActionResult> DeleteComment(string app, Guid commentsId, Guid commentId)
        {
            await CommandBus.PublishAsync(new DeleteComment { CommentsId = commentsId, CommentId = commentId });

            return NoContent();
        }
    }
}
