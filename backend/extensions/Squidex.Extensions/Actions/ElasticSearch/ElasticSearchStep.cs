// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using Elasticsearch.Net;
using Squidex.Domain.Apps.Core.HandleRules;
using Squidex.Domain.Apps.Core.Rules.EnrichedEvents;
using Squidex.Flows;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Json;
using Squidex.Infrastructure.Validation;

namespace Squidex.Extensions.Actions.ElasticSearch;

[FlowStep(
    Title = "ElasticSearch",
    IconImage = "<svg xmlns='http://www.w3.org/2000/svg' viewBox='0 0 29 28'><path d='M13.427 17.436H4.163C3.827 16.354 3.636 15.2 3.636 14s.182-2.355.527-3.436h15.245c1.891 0 3.418 1.545 3.418 3.445a3.421 3.421 0 0 1-3.418 3.427h-5.982zm-.436 1.146H4.6a11.508 11.508 0 0 0 4.2 4.982 11.443 11.443 0 0 0 15.827-3.209 5.793 5.793 0 0 0-4.173-1.773H12.99zm7.464-9.164a5.794 5.794 0 0 0 4.173-1.773 11.45 11.45 0 0 0-9.536-5.1c-2.327 0-4.491.7-6.3 1.891a11.554 11.554 0 0 0-4.2 4.982h15.864z'/></svg>",
    IconColor = "#1e5470",
    Display = "Populate ElasticSearch index",
    Description = "Populate a full text search index in ElasticSearch.",
    ReadMore = "https://www.elastic.co/")]
public sealed class ElasticSearchStep : RuleFlowStep<EnrichedEvent>
{
    [AbsoluteUrl]
    [LocalizedRequired]
    [Display(Name = "Server Url", Description = "The url to the instance or cluster.")]
    [Editor(FlowStepEditor.Url)]
    public Uri Host { get; set; }

    [LocalizedRequired]
    [Display(Name = "Index Name", Description = "The name of the index.")]
    [Editor(FlowStepEditor.Text)]
    [Expression]
    public string IndexName { get; set; }

    [Display(Name = "Username", Description = "The optional username.")]
    [Editor(FlowStepEditor.Text)]
    public string? Username { get; set; }

    [Display(Name = "Password", Description = "The optional password.")]
    [Editor(FlowStepEditor.Text)]
    public string? Password { get; set; }

    [Display(Name = "Document", Description = "The optional custom document.")]
    [Editor(FlowStepEditor.TextArea)]
    [Expression]
    public string? Document { get; set; }

    [Display(Name = "Deletion", Description = "The condition when to delete the document.")]
    [Editor(FlowStepEditor.Text)]
    public string? Delete { get; set; }

    protected async override ValueTask<FlowStepResult> ExecuteAsync(RuleFlowContext context, EnrichedEvent @event, FlowExecutionContext executionContext,
        CancellationToken ct)
    {
        string contentId;
        if (@event is IEnrichedEntityEvent enrichedEntityEvent)
        {
            contentId = enrichedEntityEvent.Id.ToString();
        }
        else
        {
            contentId = DomainId.NewGuid().ToString();
        }

        var elasticPool = executionContext.Resolve<ElasticSearchClientPool>();
        var elasticClient = await elasticPool.GetClientAsync((Host, Username, Password));

        var delete = executionContext.ShouldDelete(context, Delete);
        if (delete)
        {
            await DeleteAsync(executionContext, contentId, elasticClient, ct);
        }
        else
        {
            await UpsertAsync(executionContext, contentId, elasticClient, ct);
        }

        return FlowStepResult.Next();
    }

    private async Task DeleteAsync(FlowExecutionContext executionContext, string contentId, ElasticLowLevelClient client,
        CancellationToken ct)
    {
        var response = await client.DeleteAsync<StringResponse>(IndexName, contentId, ctx: ct);

        executionContext.Log($"Deleted entry from Elastic index: {IndexName}", response);
    }

    private async Task UpsertAsync(FlowExecutionContext executionContext, string contentId, ElasticLowLevelClient client,
        CancellationToken ct)
    {
        var serializer = executionContext.Resolve<IJsonSerializer>();

        ElasticSearchContent content;
        try
        {
            content = serializer.Deserialize<ElasticSearchContent>(Document!);
            content.ContentId = contentId;
        }
        catch (Exception ex)
        {
            content = new ElasticSearchContent
            {
                More = new Dictionary<string, object>
                {
                    ["error"] = $"Invalid JSON: {ex.Message}"
                },
                ContentId = contentId
            };
        }

        var request = serializer.Serialize(content);
        var response = await client.IndexAsync<StringResponse>(IndexName, contentId, request, ctx: ct);

        executionContext.Log($"Added entry to Elastic index: {IndexName}", response);
    }

    private sealed class ElasticSearchContent
    {
        public string ContentId { get; set; }

        [JsonExtensionData]
        public Dictionary<string, object> More { get; set; } = [];
    }
}
