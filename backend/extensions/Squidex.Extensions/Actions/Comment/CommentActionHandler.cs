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

namespace Squidex.Extensions.Actions.Comment;

public sealed class CommentActionHandler : RuleActionHandler<CommentAction, CommentCreated>
{
    private const string Description = "Send a Comment";
    private readonly ICollaborationService collaboration;

    public CommentActionHandler(RuleEventFormatter formatter, ICollaborationService collaboration)
        : base(formatter)
    {
        this.collaboration = collaboration;
    }

    protected override async Task<(string Description, CommentCreated Data)> CreateJobAsync(EnrichedEvent @event, CommentAction action)
    {
        if (@event is not EnrichedContentEvent contentEvent)
        {
            return ("Ignore", new CommentCreated());
        }

        var ruleJob = new CommentCreated
        {
            AppId = contentEvent.AppId
        };

        var text = await FormatAsync(action.Text, @event);

        if (string.IsNullOrWhiteSpace(text))
        {
            return ("NoText", new CommentCreated());
        }

        ruleJob.Text = text;

        if (!string.IsNullOrEmpty(action.Client))
        {
            ruleJob.Actor = RefToken.Client(action.Client);
        }
        else
        {
            ruleJob.Actor = contentEvent.Actor;
        }

        ruleJob.CommentsId = contentEvent.Id;

        return (Description, ruleJob);
    }

    protected override async Task<Result> ExecuteJobAsync(CommentCreated job,
        CancellationToken ct = default)
    {
        if (job.CommentsId == default)
        {
            return Result.Ignored();
        }

        await collaboration.CommentAsync(job.AppId, job.CommentsId, job.Text, job.Actor, job.Url, true, ct);

        return Result.Success($"Commented: {job.Text}");
    }
}
