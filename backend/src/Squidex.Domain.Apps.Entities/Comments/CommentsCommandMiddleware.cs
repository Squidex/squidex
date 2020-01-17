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
using Squidex.Infrastructure;
using Squidex.Infrastructure.Commands;
using Squidex.Infrastructure.Orleans;
using Squidex.Infrastructure.Reflection;
using Squidex.Infrastructure.Tasks;
using Squidex.Shared.Users;

namespace Squidex.Domain.Apps.Entities.Comments
{
    public sealed class CommentsCommandMiddleware : ICommandMiddleware
    {
        private static readonly Regex MentionRegex = new Regex(@"@(?=.{1,254}$)(?=.{1,64}@)[-!#$%&'*+\/0-9=?A-Z^_`a-z{|}~]+(\.[-!#$%&'*+\/0-9=?A-Z^_`a-z{|}~]+)*@[A-Za-z0-9]([A-Za-z0-9-]{0,61}[A-Za-z0-9])?(\.[A-Za-z0-9]([A-Za-z0-9-]{0,61}[A-Za-z0-9])?)*", RegexOptions.Compiled, TimeSpan.FromMilliseconds(100));
        private readonly IGrainFactory grainFactory;
        private readonly IUserResolver userResolver;

        public CommentsCommandMiddleware(IGrainFactory grainFactory, IUserResolver userResolver)
        {
            Guard.NotNull(grainFactory);
            Guard.NotNull(userResolver);

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

                    if (createComment.Mentions != null)
                    {
                        foreach (var userId in createComment.Mentions)
                        {
                            var notificationCommand = SimpleMapper.Map(createComment, new CreateComment());

                            notificationCommand.AppId = null!;
                            notificationCommand.Mentions = null;
                            notificationCommand.CommentsId = userId;
                            notificationCommand.ExpectedVersion = EtagVersion.Any;
                            notificationCommand.IsMention = true;

                            context.CommandBus.PublishAsync(notificationCommand).Forget();
                        }
                    }
                }

                await ExecuteCommandAsync(context, commentsCommand);
            }

            await next(context);
        }

        private async Task ExecuteCommandAsync(CommandContext context, CommentsCommand commentsCommand)
        {
            var grain = grainFactory.GetGrain<ICommentsGrain>(commentsCommand.CommentsId);

            var result = await grain.ExecuteAsync(commentsCommand.AsJ());

            context.Complete(result.Value);
        }

        private static bool IsMention(CreateComment createComment)
        {
            return createComment.IsMention;
        }

        private async Task MentionUsersAsync(CreateComment createComment)
        {
            if (!string.IsNullOrWhiteSpace(createComment.Text))
            {
                var emails = MentionRegex.Matches(createComment.Text).Select(x => x.Value.Substring(1)).ToArray();

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
                        createComment.Mentions = mentions.ToArray();
                    }
                }
            }
        }
    }
}
