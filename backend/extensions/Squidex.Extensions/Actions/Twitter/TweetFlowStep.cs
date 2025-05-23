﻿// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.ComponentModel.DataAnnotations;
using CoreTweet;
using Microsoft.Extensions.Options;
using Squidex.Domain.Apps.Core.Rules.Deprecated;
using Squidex.Flows;
using Squidex.Infrastructure.Reflection;
using Squidex.Infrastructure.Validation;

namespace Squidex.Extensions.Actions.Twitter;

[FlowStep(
    Title = "Twitter",
    IconImage = "<svg xmlns='http://www.w3.org/2000/svg' viewBox='0 0 32 32'><path d='M32 7.075a12.941 12.941 0 0 1-3.769 1.031 6.601 6.601 0 0 0 2.887-3.631 13.21 13.21 0 0 1-4.169 1.594A6.565 6.565 0 0 0 22.155 4a6.563 6.563 0 0 0-6.563 6.563c0 .512.056 1.012.169 1.494A18.635 18.635 0 0 1 2.23 5.195a6.56 6.56 0 0 0-.887 3.3 6.557 6.557 0 0 0 2.919 5.463 6.565 6.565 0 0 1-2.975-.819v.081a6.565 6.565 0 0 0 5.269 6.437 6.574 6.574 0 0 1-2.968.112 6.588 6.588 0 0 0 6.131 4.563 13.17 13.17 0 0 1-9.725 2.719 18.568 18.568 0 0 0 10.069 2.95c12.075 0 18.681-10.006 18.681-18.681 0-.287-.006-.569-.019-.85A13.216 13.216 0 0 0 32 7.076z'/></svg>",
    IconColor = "#1da1f2",
    Display = "Tweet",
    Description = "Tweet an update with your twitter account.",
    ReadMore = "https://twitter.com")]
#pragma warning disable CS0618 // Type or member is obsolete
public sealed record TweetFlowStep : FlowStep, IConvertibleToAction
#pragma warning restore CS0618 // Type or member is obsolete
{
    [LocalizedRequired]
    [Display(Name = "Access Token", Description = " The generated access token.")]
    [Editor(FlowStepEditor.Text)]
    public string AccessToken { get; set; }

    [LocalizedRequired]
    [Display(Name = "Access Secret", Description = " The generated access secret.")]
    [Editor(FlowStepEditor.Text)]
    public string AccessSecret { get; set; }

    [LocalizedRequired]
    [Display(Name = "Text", Description = "The text that is sent as tweet to twitter.")]
    [Editor(FlowStepEditor.TextArea)]
    [Expression]
    public string Text { get; set; }

    public override async ValueTask<FlowStepResult> ExecuteAsync(FlowExecutionContext executionContext,
        CancellationToken ct)
    {
        if (executionContext.IsSimulation)
        {
            executionContext.LogSkipSimulation();
            return Next();
        }

        var twitterOptions = executionContext.Resolve<IOptions<TwitterOptions>>().Value;

        var tokens = Tokens.Create(
            twitterOptions.ClientId,
            twitterOptions.ClientSecret,
            AccessToken,
            AccessSecret);

        var request = new Dictionary<string, object>
        {
            ["status"] = Text,
        };

        await tokens.Statuses.UpdateAsync(request, ct);

        executionContext.Log("Tweeted", Text);
        return Next();
    }

#pragma warning disable CS0618 // Type or member is obsolete
    public RuleAction ToAction()
    {
        return SimpleMapper.Map(this, new TweetAction());
    }
#pragma warning restore CS0618 // Type or member is obsolete
}
