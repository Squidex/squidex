﻿// ==========================================================================
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
using Squidex.Shared.Users;

namespace Squidex.Extensions.Actions.Notification;

[FlowStep(
    Title = "Notification",
    IconImage = "<svg version='1.1' xmlns='http://www.w3.org/2000/svg' width='24' height='24' viewBox='0 0 24 24'><path d='M20.016 15.984v-12h-16.031v14.016l2.016-2.016h14.016zM20.016 2.016c1.078 0 1.969 0.891 1.969 1.969v12c0 1.078-0.891 2.016-1.969 2.016h-14.016l-3.984 3.984v-18c0-1.078 0.891-1.969 1.969-1.969h16.031z'></path></svg>",
    IconColor = "#3389ff",
    Display = "Send a notification",
    Description = "Send an integrated notification to a user.")]
#pragma warning disable CS0618 // Type or member is obsolete
public sealed record NotificationFlowStep : FlowStep, IConvertibleToAction
#pragma warning restore CS0618 // Type or member is obsolete
{
    [LocalizedRequired]
    [Display(Name = "User", Description = "The user id or email.")]
    [Editor(FlowStepEditor.Text)]
    public string User { get; set; }

    [LocalizedRequired]
    [Display(Name = "Title", Description = "The text to send.")]
    [Editor(FlowStepEditor.TextArea)]
    [Expression]
    public string Text { get; set; }

    [Display(Name = "Url", Description = "The optional url to attach to the notification.")]
    [Editor(FlowStepEditor.Text)]
    [Expression]
    public string? Url { get; set; }

    [Display(Name = "Client", Description = "An optional client name.")]
    [Editor(FlowStepEditor.Text)]
    public string? Client { get; set; }

    public override async ValueTask<FlowStepResult> ExecuteAsync(FlowExecutionContext executionContext,
        CancellationToken ct)
    {
        var @event = ((FlowEventContext)executionContext.Context).Event;
        if (@event is not EnrichedUserEventBase userEvent)
        {
            executionContext.LogSkipped("Not an event with user information");
            return Next();
        }

        if (executionContext.IsSimulation)
        {
            executionContext.LogSkipSimulation();
            return Next();
        }

        var user =
            await executionContext.Resolve<IUserResolver>()
                .FindByIdOrEmailAsync(User, ct)
                ?? throw new InvalidOperationException($"Cannot find user by '{User}'");

        var actor = userEvent.Actor;
        if (!string.IsNullOrEmpty(Client))
        {
            actor = RefToken.Client(Client);
        }

        Uri? url = null;
        if (!string.IsNullOrWhiteSpace(Url) && !Uri.TryCreate(Url, UriKind.RelativeOrAbsolute, out url))
        {
            executionContext.Log($"Invalid URL: {Url}");
        }

        await executionContext.Resolve<ICollaborationService>()
            .NotifyAsync(user.Id, Text, actor, url, true, ct);

        executionContext.Log("Notified", Text);
        return Next();
    }

#pragma warning disable CS0618 // Type or member is obsolete
    public RuleAction ToAction()
    {
        return SimpleMapper.Map(this, new NotificationAction());
    }
#pragma warning restore CS0618 // Type or member is obsolete
}
