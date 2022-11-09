// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Text.RegularExpressions;
using Squidex.Domain.Apps.Entities.Comments.Commands;
using Squidex.Infrastructure.Commands;
using Squidex.Shared.Users;

namespace Squidex.Domain.Apps.Entities.Comments.DomainObject;

public sealed class CommentsCommandMiddleware : AggregateCommandMiddleware<CommentsCommandBase, CommentsStream>
{
    private static readonly Regex MentionRegex = new Regex(@"@(?=.{1,254}$)(?=.{1,64}@)[-!#$%&'*+\/0-9=?A-Z^_`a-z{|}~]+(\.[-!#$%&'*+\/0-9=?A-Z^_`a-z{|}~]+)*@[A-Za-z0-9]([A-Za-z0-9-]{0,61}[A-Za-z0-9])?(\.[A-Za-z0-9]([A-Za-z0-9-]{0,61}[A-Za-z0-9])?)*", RegexOptions.Compiled | RegexOptions.ExplicitCapture, TimeSpan.FromMilliseconds(100));
    private readonly IUserResolver userResolver;

    public CommentsCommandMiddleware(IDomainObjectFactory domainObjectFactory, IUserResolver userResolver)
        : base(domainObjectFactory)
    {
        this.userResolver = userResolver;
    }

    public override async Task HandleAsync(CommandContext context, NextDelegate next,
        CancellationToken ct)
    {
        if (context.Command is CommentsCommand commentsCommand)
        {
            if (commentsCommand is CreateComment createComment && !IsMention(createComment))
            {
                await MentionUsersAsync(createComment);
            }
        }

        await base.HandleAsync(context, next, ct);
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
