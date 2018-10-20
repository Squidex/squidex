// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Orleans;
using Squidex.Areas.Api.Controllers.Comments.Models;
using Squidex.Domain.Apps.Entities.Comments;
using Squidex.Domain.Apps.Entities.Comments.Commands;
using Squidex.Infrastructure.Commands;
using Squidex.Pipeline;

namespace Squidex.Areas.Api.Controllers.Comments
{
    /// <summary>
    /// Manages comments for any kind of resource.
    /// </summary>
    [ApiExceptionFilter]
    [ApiExplorerSettings(GroupName = nameof(Languages))]
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
        /// <param name="commentsId">The id of the comments.</param>
        /// <returns>
        /// 200 => All comments returned.
        /// </returns>
        [HttpGet]
        [Route("comments/{commentsId}")]
        [ProducesResponseType(typeof(CommentsDto), 200)]
        [ApiCosts(0)]
        public async Task<IActionResult> GetComments(Guid commentsId)
        {
            if (!int.TryParse(Request.Headers["X-Since"], out var version))
            {
                version = -1;
            }

            var result = await grainFactory.GetGrain<ICommentGrain>(commentsId).GetCommentsAsync(version);
            var response = CommentsDto.FromResult(result);

            Response.Headers["ETag"] = response.Version.ToString();

            return Ok(response);
        }

        /// <summary>
        /// Create a new comment.
        /// </summary>
        /// <param name="commentsId">The id of the comments.</param>
        /// <param name="request">The comment object that needs to created.</param>
        /// <returns>
        /// 201 => Comment created.
        /// 400 => Comment is not valid.
        /// </returns>
        [HttpPost]
        [Route("comments/{commentdsId}")]
        [ProducesResponseType(typeof(EntityCreatedDto), 201)]
        [ProducesResponseType(typeof(ErrorDto), 400)]
        [ApiCosts(0)]
        public async Task<IActionResult> PostComment(Guid commentsId, [FromBody] UpsertCommentDto request)
        {
            var command = request.ToCreateCommand(commentsId);
            var context = await CommandBus.PublishAsync(command);

            var response = CommentDto.FromCommand(command);

            return CreatedAtAction(nameof(GetComments), new { commentsId }, response);
        }

        /// <summary>
        /// Updates the comment.
        /// </summary>
        /// <param name="commentsId">The id of the comments.</param>
        /// <param name="commentId">The id of the comment.</param>
        /// <param name="request">The comment object that needs to updated.</param>
        /// <returns>
        /// 204 => Comment updated.
        /// 400 => Comment text not valid.
        /// 404 => Comment not found.
        /// </returns>
        [MustBeAppReader]
        [HttpPut]
        [Route("comments/{commentdsId}/{commentId}")]
        [ProducesResponseType(typeof(ErrorDto), 400)]
        [ApiCosts(0)]
        public async Task<IActionResult> PutComment(Guid commentsId, Guid commentId, [FromBody] UpsertCommentDto request)
        {
            await CommandBus.PublishAsync(request.ToUpdateComment(commentsId, commentId));

            return NoContent();
        }

        /// <summary>
        /// Deletes the comment.
        /// </summary>
        /// <param name="commentsId">The id of the comments.</param>
        /// <param name="commentId">The id of the comment.</param>
        /// <returns>
        /// 204 => Comment deleted.
        /// 404 => Comment not found.
        /// </returns>
        [HttpDelete]
        [Route("comments/{commentdsId}/{commentId}")]
        [ProducesResponseType(typeof(ErrorDto), 400)]
        [ApiCosts(0)]
        public async Task<IActionResult> DeleteComment(Guid commentsId, Guid commentId)
        {
            await CommandBus.PublishAsync(new DeleteComment { CommentsId = commentsId, CommentId = commentId });

            return NoContent();
        }
    }
}
