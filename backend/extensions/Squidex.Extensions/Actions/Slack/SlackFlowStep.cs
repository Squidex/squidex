// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.ComponentModel.DataAnnotations;
using System.Text;
using Google.Apis.Json;
using Migrations.OldActions;
using Squidex.Domain.Apps.Core.Rules.Deprecated;
using Squidex.Flows;
using Squidex.Infrastructure.Reflection;
using Squidex.Infrastructure.Validation;

namespace Squidex.Extensions.Actions.Slack;

[FlowStep(
    Title = "Slack",
    IconImage = "<svg xmlns='http://www.w3.org/2000/svg' viewBox='0 0 26 28'><path d='M23.734 12.125c1.281 0 2.266.938 2.266 2.219 0 1-.516 1.703-1.453 2.031l-2.688.922.875 2.609c.078.234.109.484.109.734 0 1.234-1 2.266-2.234 2.266a2.271 2.271 0 0 1-2.172-1.547l-.859-2.578-4.844 1.656.859 2.562c.078.234.125.484.125.734 0 1.219-1 2.266-2.25 2.266a2.25 2.25 0 0 1-2.156-1.547l-.859-2.547-2.391.828c-.25.078-.516.141-.781.141-1.266 0-2.219-.938-2.219-2.203 0-.969.625-1.844 1.547-2.156l2.438-.828-1.641-4.891-2.438.844c-.25.078-.5.125-.75.125-1.25 0-2.219-.953-2.219-2.203 0-.969.625-1.844 1.547-2.156l2.453-.828-.828-2.484a2.337 2.337 0 0 1-.125-.734c0-1.234 1-2.266 2.25-2.266a2.25 2.25 0 0 1 2.156 1.547l.844 2.5L13.14 5.5 12.296 3a2.337 2.337 0 0 1-.125-.734c0-1.234 1.016-2.266 2.25-2.266.984 0 1.859.625 2.172 1.547l.828 2.516 2.531-.859c.219-.063.438-.094.672-.094 1.219 0 2.266.906 2.266 2.156 0 .969-.75 1.781-1.625 2.078l-2.453.844 1.641 4.937 2.562-.875a2.32 2.32 0 0 1 .719-.125zm-12.406 4.094l4.844-1.641-1.641-4.922-4.844 1.672z'/></svg>",
    IconColor = "#5c3a58",
    Display = "Send to Slack",
    Description = "Create a status update to a slack channel.",
    ReadMore = "https://slack.com")]
#pragma warning disable CS0618 // Type or member is obsolete
public sealed record SlackFlowStep : FlowStep, IConvertibleToAction
#pragma warning restore CS0618 // Type or member is obsolete
{
    [AbsoluteUrl]
    [LocalizedRequired]
    [Display(Name = "Webhook Url", Description = "The slack webhook url.")]
    [Editor(FlowStepEditor.Text)]
    public Uri WebhookUrl { get; set; }

    [LocalizedRequired]
    [Display(Name = "Text", Description = "The text that is sent as message to slack.")]
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

        var body = new { text = Text };

        var jsonRequest = executionContext.SerializeJson(body);

        var request = new HttpRequestMessage(HttpMethod.Post, WebhookUrl)
        {
            Content = new StringContent(jsonRequest, Encoding.UTF8, "application/json"),
        };

        var httpClient =
            executionContext.Resolve<IHttpClientFactory>()
                .CreateClient("SlackAction");

        var (_, dump) = await httpClient.SendAsync(executionContext, request, jsonRequest, ct);

        executionContext.Log("Notification sent", dump);
        return Next();
    }

#pragma warning disable CS0618 // Type or member is obsolete
    public RuleAction ToAction()
    {
        return SimpleMapper.Map(this, new SlackAction());
    }
#pragma warning restore CS0618 // Type or member is obsolete
}
