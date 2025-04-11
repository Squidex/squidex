// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using OpenSearch.Net;
using Squidex.Domain.Apps.Core.HandleRules;
using Squidex.Domain.Apps.Core.Rules.Deprecated;
using Squidex.Flows;
using Squidex.Infrastructure.Json;
using Squidex.Infrastructure.Reflection;
using Squidex.Infrastructure.Validation;

namespace Squidex.Extensions.Actions.OpenSearch;

[FlowStep(
    Title = "OpenSearch",
    IconImage = "<svg xmlns='http://www.w3.org/2000/svg' viewBox='0 0 64 64'><path d='M61.737 23.5a2.263 2.263 0 0 0-2.262 2.263c0 18.618-15.094 33.712-33.712 33.712a2.263 2.263 0 1 0 0 4.525C46.88 64 64 46.88 64 25.763a2.263 2.263 0 0 0-2.263-2.263Z' fill='#fff'/><path d='M48.081 38c2.176-3.55 4.28-8.282 3.866-14.908C51.09 9.367 38.66-1.045 26.921.084c-4.596.441-9.314 4.187-8.895 10.896.182 2.916 1.61 4.637 3.928 5.96 2.208 1.26 5.044 2.057 8.259 2.961 3.883 1.092 8.388 2.32 11.85 4.87 4.15 3.058 6.986 6.603 6.018 13.229Z' fill='#fff'/><path d='M3.919 14C1.743 17.55-.361 22.282.052 28.908.91 42.633 13.342 53.045 25.08 51.916c4.596-.441 9.314-4.187 8.895-10.896-.182-2.916-1.61-4.637-3.928-5.96-2.208-1.26-5.044-2.057-8.259-2.961-3.883-1.092-8.388-2.32-11.85-4.87C5.787 24.17 2.95 20.625 3.919 14Z' fill='#fff'/></svg>",
    IconColor = "#005EB8",
    Display = "Populate OpenSearch index",
    Description = "Populate a full text search index in OpenSearch.",
    ReadMore = "https://opensearch.org/")]
#pragma warning disable CS0618 // Type or member is obsolete
public sealed record OpenSearchFlowStep : FlowStep, IConvertibleToAction
#pragma warning restore CS0618 // Type or member is obsolete
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
    [Expression(ExpressionFallback.Event)]
    public string? Document { get; set; }

    [Display(Name = "Deletion", Description = "The condition when to delete the document.")]
    [Editor(FlowStepEditor.Text)]
    public string? Delete { get; set; }

    private static readonly ClientPool<(Uri Host, string? Username, string? Password), OpenSearchLowLevelClient> Clients = new (key =>
    {
        var config = new ConnectionConfiguration(key.Host);

        if (!string.IsNullOrEmpty(key.Username) && !string.IsNullOrWhiteSpace(key.Password))
        {
            config = config.BasicAuthentication(key.Username, key.Password);
        }

        return new OpenSearchLowLevelClient(config);
    });

    public override ValueTask PrepareAsync(FlowExecutionContext executionContext,
        CancellationToken ct)
    {
        var @event = ((FlowEventContext)executionContext.Context).Event;

        if (@event.ShouldDelete(executionContext, Delete))
        {
            OpenSearchContent content;
            try
            {
                content = executionContext.DeserializeJson<OpenSearchContent>(Document!);
            }
            catch (Exception ex)
            {
                content = new OpenSearchContent
                {
                    More = new Dictionary<string, object>
                    {
                        ["error"] = $"Invalid JSON: {ex.Message}",
                    },
                };
            }

            Document = executionContext.SerializeJson(content);
        }
        else
        {
            Document = null;
        }

        return base.PrepareAsync(executionContext, ct);
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
            void HandleResult(StringResponse response, string message)
            {
                if (response.OriginalException != null)
                {
                    executionContext.Log("Failed with error", response.OriginalException.Message);
                    throw response.OriginalException;
                }

                var serializer = executionContext.Resolve<IJsonSerializer>();
                executionContext.Log(message, serializer.Serialize(response, true));
            }

            var client = await Clients.GetClientAsync((Host, Username, Password));
            if (Document != null)
            {
                var response = await client.IndexAsync<StringResponse>(IndexName, id, Document, ctx: ct);

                HandleResult(response, $"Document with ID '{id}' upserted");
            }
            else
            {
                var response = await client.DeleteAsync<StringResponse>(IndexName, id, ctx: ct);

                HandleResult(response, $"Document with ID '{id}' deleted");
            }

            return Next();
        }
        catch (OpenSearchClientException ex)
        {
            executionContext.Log("Failed with error", ex.Message);
            throw;
        }
    }

#pragma warning disable CS0618 // Type or member is obsolete
    public RuleAction ToAction()
    {
        return SimpleMapper.Map(this, new OpenSearchAction());
    }
#pragma warning restore CS0618 // Type or member is obsolete

    private sealed class OpenSearchContent
    {
        public string ContentId { get; set; }

        [JsonExtensionData]
        public Dictionary<string, object> More { get; set; } = [];
    }
}
