// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Threading.Tasks;
using Orleans;
using Squidex.Domain.Apps.Entities.Comments.Commands;
using Squidex.Infrastructure.Commands;

namespace Squidex.Domain.Apps.Entities.Comments
{
    public sealed class CommentsCommandHandler : GrainCommandMiddleware<CommentsCommand, ICommentsGrain>
    {
        public CommentsCommandHandler(IGrainFactory grainFactory)
            : base(grainFactory)
        {
        }

        public async override Task HandleAsync(CommandContext context, Func<Task> next)
        {
            if (context.Command is CreateComment createComment && !createComment.NoMention)
            {
                await MentionUsersAsync(createComment);
            }

            await base.HandleAsync(context, next);
        }

        private Task MentionUsersAsync(CreateComment createComment)
        {
            throw new NotImplementedException();
        }
    }
}
