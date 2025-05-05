// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.ComponentModel.DataAnnotations;
using Squidex.Domain.Apps.Core.HandleRules;
using Squidex.Domain.Apps.Core.Rules.Deprecated;
using Squidex.Domain.Apps.Core.Rules.EnrichedEvents;
using Squidex.Domain.Apps.Entities.Collaboration;
using Squidex.Flows;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Reflection;
using Squidex.Infrastructure.Validation;

namespace Squidex.Extensions.Actions.Comment;

[FlowStep(
    Title = "Comment",
    IconImage = "<svg version='1.1' xmlns='http://www.w3.org/2000/svg' width='24' height='24' viewBox='0 0 24 24'><path d='M20.016 15.984v-12h-16.031v14.016l2.016-2.016h14.016zM20.016 2.016c1.078 0 1.969 0.891 1.969 1.969v12c0 1.078-0.891 2.016-1.969 2.016h-14.016l-3.984 3.984v-18c0-1.078 0.891-1.969 1.969-1.969h16.031z'></path></svg>",
    IconColor = "#3389ff",
    Display = "Create comment",
    Description = "Create a comment for a content event.")]
#pragma warning disable CS0618 // Type or member is obsolete
public sealed record CommentFlowStep : FlowStep, IConvertibleToAction
#pragma warning restore CS0618 // Type or member is obsolete
{
    [LocalizedRequired]
    [Display(Name = "Text", Description = "The comment text.")]
    [Editor(FlowStepEditor.TextArea)]
    [Expression]
    public string Text { get; set; }

    [Display(Name = "Client", Description = "An optional client name.")]
    [Editor(FlowStepEditor.Text)]
    public string? Client { get; set; }

    public async override ValueTask<FlowStepResult> ExecuteAsync(FlowExecutionContext executionContext,
        CancellationToken ct)
    {
        var @event = ((FlowEventContext)executionContext.Context).Event;
        if (@event is not EnrichedContentEvent contentEvent)
        {
            executionContext.LogSkipped("Invalid event.");
            return Next();
        }

        if (string.IsNullOrWhiteSpace(Text))
        {
            executionContext.LogSkipped("No text.");
            return Next();
        }

        RefToken actor;
        if (!string.IsNullOrEmpty(Client))
        {
            actor = RefToken.Client(Client);
        }
        else
        {
            actor = contentEvent.Actor;
        }

        var collaboration = executionContext.Resolve<ICollaborationService>();

        await collaboration.CommentAsync(
            contentEvent.AppId,
            contentEvent.Id,
            Text,
            actor,
            null,
            true,
            ct);

        executionContext.Log("Commented", Text);
        return Next();
    }

#pragma warning disable CS0618 // Type or member is obsolete
    public RuleAction ToAction()
    {
        return SimpleMapper.Map(this, new CommentAction());
    }
#pragma warning restore CS0618 // Type or member is obsolete
}
