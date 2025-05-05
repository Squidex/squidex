// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.ComponentModel.DataAnnotations;
using System.Text;
using Squidex.Domain.Apps.Core.Rules.Deprecated;
using Squidex.Flows;
using Squidex.Infrastructure.Json;
using Squidex.Infrastructure.Reflection;
using Squidex.Infrastructure.Validation;

namespace Squidex.Extensions.Actions.Prerender;

[FlowStep(
    Title = "Prerender",
    IconImage = "<svg xmlns='http://www.w3.org/2000/svg' viewBox='0 0 32 32'><path d='M2.073 17.984l8.646-5.36v-1.787L.356 17.325v1.318l10.363 6.488v-1.787zM29.927 17.984l-8.646-5.36v-1.787l10.363 6.488v1.318l-10.363 6.488v-1.787zM18.228 6.693l-6.276 19.426 1.656.548 6.276-19.426z'/></svg>",
    IconColor = "#2c3e50",
    Display = "Recache URL",
    Description = "Prerender a javascript website for bots.",
    ReadMore = "https://prerender.io")]
#pragma warning disable CS0618 // Type or member is obsolete
public sealed record PrerenderFlowStep : FlowStep, IConvertibleToAction
#pragma warning restore CS0618 // Type or member is obsolete
{
    [LocalizedRequired]
    [Display(Name = "Token", Description = "The prerender token from your account.")]
    [Editor(FlowStepEditor.Text)]
    [Expression]
    public string Token { get; set; }

    [LocalizedRequired]
    [Display(Name = "Url", Description = "The url to recache.")]
    [Editor(FlowStepEditor.Text)]
    [Expression]
    public string Url { get; set; }

    public override async ValueTask<FlowStepResult> ExecuteAsync(FlowExecutionContext executionContext,
        CancellationToken ct)
    {
        if (executionContext.IsSimulation)
        {
            executionContext.LogSkipSimulation();
            return Next();
        }

        var requestObject = new { prerenderToken = Token, Url };
        var requestBody = executionContext.SerializeJson(requestObject);

        var request = new HttpRequestMessage(HttpMethod.Post, "/recache")
        {
            Content = new StringContent(requestBody, Encoding.UTF8, "application/json"),
        };

        var httpClient =
            executionContext.Resolve<IHttpClientFactory>()
                .CreateClient("Prerender");

        var (_, dump) = await httpClient.SendAsync(executionContext, request, requestBody, ct);

        executionContext.Log("Success", dump);
        return Next();
    }

#pragma warning disable CS0618 // Type or member is obsolete
    public RuleAction ToAction()
    {
        return SimpleMapper.Map(this, new PrerenderAction());
    }
#pragma warning restore CS0618 // Type or member is obsolete
}
