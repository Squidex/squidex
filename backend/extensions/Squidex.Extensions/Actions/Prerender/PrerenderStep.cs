// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.ComponentModel.DataAnnotations;
using System.Text;
using Squidex.Domain.Apps.Core.HandleRules;
using Squidex.Domain.Apps.Core.Rules.EnrichedEvents;
using Squidex.Flows;
using Squidex.Flows.Steps.Utils;
using Squidex.Infrastructure.Json;
using Squidex.Infrastructure.Validation;

namespace Squidex.Extensions.Actions.Prerender;

[FlowStep(
    Title = "Prerender",
    IconImage = "<svg xmlns='http://www.w3.org/2000/svg' viewBox='0 0 32 32'><path d='M2.073 17.984l8.646-5.36v-1.787L.356 17.325v1.318l10.363 6.488v-1.787zM29.927 17.984l-8.646-5.36v-1.787l10.363 6.488v1.318l-10.363 6.488v-1.787zM18.228 6.693l-6.276 19.426 1.656.548 6.276-19.426z'/></svg>",
    IconColor = "#2c3e50",
    Display = "Recache URL",
    Description = "Prerender a javascript website for bots.",
    ReadMore = "https://prerender.io")]
public sealed class PrerenderStep : RuleFlowStep<EnrichedEvent>
{
    [LocalizedRequired]
    [Display(Name = "Token", Description = "The prerender token from your account.")]
    [Editor(FlowStepEditor.Text)]
    [Expression]
    public string Token { get; set; }

    [LocalizedRequired]
    [Display(Name = "Url", Description = "The url to recache.")]
    [Editor(FlowStepEditor.Text)]
    public string Url { get; set; }

    protected override async ValueTask<FlowStepResult> ExecuteAsync(RuleFlowContext context, EnrichedEvent @event, FlowExecutionContext executionContext,
        CancellationToken ct)
    {
        var requestObject = new { prerenderToken = Token, url = Url };
        var requestBody = executionContext.Resolve<IJsonSerializer>().Serialize(requestObject);

        var httpClient = executionContext.Resolve<IHttpClientFactory>().CreateClient($"Prerender");

        var request = new HttpRequestMessage(HttpMethod.Post, "/recache")
        {
            Content = new StringContent(requestBody, Encoding.UTF8, "application/json")
        };

        await httpClient.OneWayRequestAsync(request, executionContext.Log, requestBody, ct);
        return FlowStepResult.Next();
    }
}
