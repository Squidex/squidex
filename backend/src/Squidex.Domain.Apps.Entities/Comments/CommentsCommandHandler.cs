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
using Squidex.Infrastructure;
using Squidex.Infrastructure.Commands;
using Squidex.Infrastructure.Orleans;

namespace Squidex.Domain.Apps.Entities.Comments
{
    public sealed class CommentsCommandHandler : ICommandMiddleware
    {
        private readonly IGrainFactory grainFactory;

        public CommentsCommandHandler(IGrainFactory grainFactory)
        {
            Guard.NotNull(grainFactory);

            this.grainFactory = grainFactory;
        }

        public async Task HandleAsync(CommandContext context, Func<Task> next)
        {
            if (context.Command is CommentsCommand commentsCommand)
            {
                if (commentsCommand is CreateComment createComment && !createComment.NoMention)
                {
                    await MentionUsersAsync(createComment);
                }

                var grain = grainFactory.GetGrain<ICommentsGrain>(commentsCommand.CommentsId);

                var result = await grain.ExecuteAsync(commentsCommand.AsJ());

                context.Complete(result);
            }

            await next();
        }

        private Task MentionUsersAsync(CreateComment createComment)
        {
            throw new NotImplementedException();
        }
    }
}
