// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Orleans;
using Squidex.Domain.Apps.Entities.Comments.Commands;
using Squidex.Infrastructure.Commands;
using Squidex.Infrastructure.Orleans;
using Squidex.Shared.Users;

namespace Squidex.Domain.Apps.Entities.Comments.DomainObject
{
    public sealed class CommentsCommandMiddleware : ICommandMiddleware
    {
        private static readonly Regex MentionRegex = new Regex(@"@(?=.{1,254}$)(?=.{1,64}@)[-!#$%&'*+\/0-9=?A-Z^_`a-z{|}~]+(\.[-!#$%&'*+\/0-9=?A-Z^_`a-z{|}~]+)*@[A-Za-z0-9]([A-Za-z0-9-]{0,61}[A-Za-z0-9])?(\.[A-Za-z0-9]([A-Za-z0-9-]{0,61}[A-Za-z0-9])?)*", RegexOptions.Compiled | RegexOptions.ExplicitCapture, TimeSpan.FromMilliseconds(100));
        private readonly IGrainFactory grainFactory;
        private readonly IUserResolver userResolver;

        public CommentsCommandMiddleware(IGrainFactory grainFactory, IUserResolver userResolver)
        {
            this.grainFactory = grainFactory;

            this.userResolver = userResolver;
        }

        public async Task HandleAsync(CommandContext context, NextDelegate next)
        {
            if (context.Command is CommentsCommand commentsCommand)
            {
                if (commentsCommand is CreateComment createComment && !IsMention(createComment))
                {
                    await MentionUsersAsync(createComment);
                }

                await ExecuteCommandAsync(context, commentsCommand);
            }

            await next(context);
        }

        private async Task ExecuteCommandAsync(CommandContext context, CommentsCommand commentsCommand)
        {
            var result = await GetGrain(commentsCommand).ExecuteAsync(commentsCommand.AsJ());

            context.Complete(result.Value);
        }

        private ICommentsGrain GetGrain(CommentsCommand commentsCommand)
        {
            return grainFactory.GetGrain<ICommentsGrain>(commentsCommand.CommentsId.ToString());
        }

        private static bool IsMention(CreateComment createComment)
        {
            return createComment.IsMention;
        }

        private async Task MentionUsersAsync(CommentTextCommand command)
        {
            if (!string.IsNullOrWhiteSpace(command.Text))
            {
                var emails = MentionRegex.Matches(command.Text).Select(x => x.Value[1..]).ToArray();

                if (emails.Length > 0)
                {
                    var mentions = new List<string>();

                    foreach (var email in emails)
                    {
                        var user = await userResolver.FindByIdOrEmailAsync(email);

                        if (user != null)
                        {
                            mentions.Add(user.Id);
                        }
                    }

                    if (mentions.Count > 0)
                    {
                        command.Mentions = mentions.ToArray();
                    }
                }
            }
        }
    }
}
