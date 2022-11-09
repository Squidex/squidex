// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Domain.Apps.Core.HandleRules;
using Squidex.Domain.Apps.Core.Rules.EnrichedEvents;
using Squidex.Domain.Apps.Entities.Comments.Commands;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Commands;

namespace Squidex.Extensions.Actions.Comment;

public sealed class CommentActionHandler : RuleActionHandler<CommentAction, CreateComment>
{
    private const string Description = "Send a Comment";
    private readonly ICommandBus commandBus;

    public CommentActionHandler(RuleEventFormatter formatter, ICommandBus commandBus)
        : base(formatter)
    {
        this.commandBus = commandBus;
    }

    protected override async Task<(string Description, CreateComment Data)> CreateJobAsync(EnrichedEvent @event, CommentAction action)
    {
        if (@event is EnrichedContentEvent contentEvent)
        {
            var ruleJob = new CreateComment
            {
                AppId = contentEvent.AppId
            };

            ruleJob.Text = await FormatAsync(action.Text, @event);

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

        return ("Ignore", new CreateComment());
    }

    protected override async Task<Result> ExecuteJobAsync(CreateComment job,
        CancellationToken ct = default)
    {
        var command = job;

        if (command.CommentsId == default)
        {
            return Result.Ignored();
        }

        command.FromRule = true;

        await commandBus.PublishAsync(command, ct);

        return Result.Success($"Commented: {command.Text}");
    }
}
