// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using Algolia.Search.Clients;
using Newtonsoft.Json.Linq;
using Squidex.Domain.Apps.Core.HandleRules;
using Squidex.Domain.Apps.Core.Rules.Deprecated;
using Squidex.Flows;
using Squidex.Infrastructure.Json;
using Squidex.Infrastructure.Reflection;
using Squidex.Infrastructure.Validation;

namespace Squidex.Extensions.Actions.Algolia;

[FlowStep(
    Title = "Algolia",
    IconImage = "<svg xmlns='http://www.w3.org/2000/svg' viewBox='0 0 32 32'><path d='M16 .842C7.633.842.842 7.625.842 16S7.625 31.158 16 31.158c8.374 0 15.158-6.791 15.158-15.166S24.375.842 16 .842zm0 25.83c-5.898 0-10.68-4.781-10.68-10.68S10.101 5.313 16 5.313s10.68 4.781 10.68 10.679-4.781 10.68-10.68 10.68zm0-19.156v7.956c0 .233.249.388.458.279l7.055-3.663a.312.312 0 0 0 .124-.434 8.807 8.807 0 0 0-7.319-4.447z'/></svg>",
    IconColor = "#0d9bf9",
    Display = "Populate Algolia index",
    Description = "Populate a full text search index in Algolia.",
    ReadMore = "https://www.algolia.com/")]
#pragma warning disable CS0618 // Type or member is obsolete
public record AlgoliaFlowStep : FlowStep, IConvertibleToAction
#pragma warning restore CS0618 // Type or member is obsolete
{
    private static readonly ClientPool<(string AppId, string ApiKey, string IndexName), ISearchIndex> Clients = new ClientPool<(string AppId, string ApiKey, string IndexName), ISearchIndex>(key =>
    {
        var client = new SearchClient(key.AppId, key.ApiKey);

        return client.InitIndex(key.IndexName);
    });

    [LocalizedRequired]
    [Display(Name = "Application Id", Description = "The application ID.")]
    [Editor(FlowStepEditor.Text)]
    [Expression]
    public string AppId { get; set; }

    [LocalizedRequired]
    [Display(Name = "Api Key", Description = "The API key to grant access to Squidex.")]
    [Editor(FlowStepEditor.Text)]
    [Expression]
    public string ApiKey { get; set; }

    [LocalizedRequired]
    [Display(Name = "Index Name", Description = "The name of the index.")]
    [Editor(FlowStepEditor.Text)]
    [Expression]
    public string IndexName { get; set; }

    [Display(Name = "Document", Description = "The optional custom document.")]
    [Editor(FlowStepEditor.TextArea)]
    [Expression(ExpressionFallback.Event)]
    public string? Document { get; set; }

    [Display(Name = "Deletion", Description = "The condition when to delete the entry.")]
    [Editor(FlowStepEditor.Text)]
    public string? Delete { get; set; }

    public override ValueTask PrepareAsync(FlowExecutionContext executionContext,
        CancellationToken ct)
    {
        var @event = ((FlowEventContext)executionContext.Context).Event;

        if (!@event.ShouldDelete(executionContext, Delete))
        {
            Document = null;
            return default;
        }

        AlgoliaContent content;
        try
        {
            content = executionContext.DeserializeJson<AlgoliaContent>(Document!);
            content.ObjectID = @event.GetOrCreateId().Id;
        }
        catch (Exception ex)
        {
            content = new AlgoliaContent
            {
                More = new Dictionary<string, object>
                {
                    ["error"] = $"Invalid JSON: {ex.Message}",
                },
                ObjectID = @event.GetOrCreateId().Id,
            };
        }

        Document = executionContext.SerializeJson(content);
        return default;
    }

    public override async ValueTask<FlowStepResult> ExecuteAsync(FlowExecutionContext executionContext,
        CancellationToken ct)
    {
        var @event = ((FlowEventContext)executionContext.Context).Event;

        var (id, isGenerated) = @event.GetOrCreateId();
        if (isGenerated && Document == null)
        {
            executionContext.LogSkipped("Can only delete content for static identities.");
            return Next();
        }

        if (executionContext.IsSimulation)
        {
            executionContext.LogSkipSimulation();
            return Next();
        }

        try
        {
            var serializer = executionContext.Resolve<IJsonSerializer>();

            var index = await Clients.GetClientAsync((AppId, ApiKey, IndexName));
            if (Document != null)
            {
                var response = await index.SaveObjectsAsync([new JRaw(Document)], null, ct, true);

                executionContext.Log($"Document with ID '{id}' upserted", serializer.Serialize(response, true));
            }
            else
            {
                var response = await index.DeleteObjectAsync(id, null, ct);

                executionContext.Log($"Document with ID '{id}' deleted", serializer.Serialize(response, true));
            }

            return Next();
        }
        catch (Exception ex)
        {
            executionContext.Log("Failed with error", ex.Message);
            throw;
        }
    }

#pragma warning disable CS0618 // Type or member is obsolete
    public RuleAction ToAction()
    {
        return SimpleMapper.Map(this, new AlgoliaAction());
    }
#pragma warning restore CS0618 // Type or member is obsolete

    private sealed class AlgoliaContent
    {
        [JsonPropertyName("objectID")]
        public string ObjectID { get; set; }

        [JsonExtensionData]
        public Dictionary<string, object> More { get; set; } = [];
    }
}
