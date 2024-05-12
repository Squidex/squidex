// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Domain.Apps.Core.HandleRules;
using Squidex.Domain.Apps.Core.Rules.EnrichedEvents;
using Squidex.Domain.Apps.Entities.Collaboration;
using Squidex.Domain.Apps.Events.Comments;
using Squidex.Infrastructure;
using Squidex.Shared.Users;

namespace Squidex.Extensions.Actions.Notification;

public sealed class NotificationActionHandler : RuleActionHandler<NotificationAction, CommentCreated>
{
    private const string Description = "Send a Notification";
    private readonly ICollaborationService collaboration;
    private readonly IUserResolver userResolver;

    public NotificationActionHandler(RuleEventFormatter formatter, ICollaborationService collaboration, IUserResolver userResolver)
        : base(formatter)
    {
        this.collaboration = collaboration;
        this.userResolver = userResolver;
    }

    protected override async Task<(string Description, CommentCreated Data)> CreateJobAsync(EnrichedEvent @event, NotificationAction action)
    {
        if (@event is not EnrichedUserEventBase userEvent)
        {
            return ("Ignore", new CommentCreated());
        }

        var user = await userResolver.FindByIdOrEmailAsync(action.User)
            ?? throw new InvalidOperationException($"Cannot find user by '{action.User}'");

        var actor = userEvent.Actor;

        if (!string.IsNullOrEmpty(action.Client))
        {
            actor = RefToken.Client(action.Client);
        }

        var ruleJob = new CommentCreated
        {
            Actor = actor,
            CommentId = DomainId.NewGuid(),
            CommentsId = DomainId.Create(user.Id),
            FromRule = true,
            Text = (await FormatAsync(action.Text, @event))!
        };

        if (!string.IsNullOrWhiteSpace(action.Url))
        {
            var url = await FormatAsync(action.Url, @event);

            if (Uri.TryCreate(url, UriKind.RelativeOrAbsolute, out var uri))
            {
                ruleJob.Url = uri;
            }
        }

        return (Description, ruleJob);
    }

    protected override async Task<Result> ExecuteJobAsync(CommentCreated job,
        CancellationToken ct = default)
    {
        if (job.CommentsId == default)
        {
            return Result.Ignored();
        }

        await collaboration.NotifyAsync(job.CommentsId.ToString(), job.Text, job.Actor, job.Url, true, ct);

        return Result.Success($"Notified: {job.Text}");
    }
}
