using Elasticsearch.Net;
using Squidex.ClientLibrary;
using Squidex.Domain.Apps.Core.HandleRules;
using Squidex.Flows;
using Squidex.Infrastructure.Json;
using Squidex.Infrastructure.Validation;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using static Squidex.Extensions.Actions.Algolia.AlgoliaFlowStep;

namespace Squidex.Extensions.Actions.ElasticSearch;

[FlowStep(
    Title = "ElasticSearch",
    IconImage = "<svg xmlns='http://www.w3.org/2000/svg' viewBox='0 0 29 28'><path d='M13.427 17.436H4.163C3.827 16.354 3.636 15.2 3.636 14s.182-2.355.527-3.436h15.245c1.891 0 3.418 1.545 3.418 3.445a3.421 3.421 0 0 1-3.418 3.427h-5.982zm-.436 1.146H4.6a11.508 11.508 0 0 0 4.2 4.982 11.443 11.443 0 0 0 15.827-3.209 5.793 5.793 0 0 0-4.173-1.773H12.99zm7.464-9.164a5.794 5.794 0 0 0 4.173-1.773 11.45 11.45 0 0 0-9.536-5.1c-2.327 0-4.491.7-6.3 1.891a11.554 11.554 0 0 0-4.2 4.982h15.864z'/></svg>",
    IconColor = "#1e5470",
    Display = "Populate ElasticSearch index",
    Description = "Populate a full text search index in ElasticSearch.",
    ReadMore = "https://www.elastic.co/")]
internal sealed record ElasticSearchFlowStep : FlowStep
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

    private static readonly ClientPool<(Uri Host, string? Username, string? Password), ElasticLowLevelClient> Clients = new ClientPool<(Uri Host, string? Username, string? Password), ElasticLowLevelClient>(key =>
    {
        var config = new ConnectionConfiguration(key.Host);

        if (!string.IsNullOrEmpty(key.Username) && !string.IsNullOrWhiteSpace(key.Password))
        {
            config = config.BasicAuthentication(key.Username, key.Password);
        }

        return new ElasticLowLevelClient(config);
    });

    public override ValueTask PrepareAsync(FlowExecutionContext executionContext,
        CancellationToken ct)
    {
        var @event = ((FlowEventContext)executionContext.Context).Event;

        if (@event.ShouldDelete(executionContext, Delete))
        {
            var serializer = executionContext.Resolve<IJsonSerializer>();

            ElasticSearchContent content;
            try
            {
                content = serializer.Deserialize<ElasticSearchContent>(Document!);
            }
            catch (Exception ex)
            {
                content = new ElasticSearchContent
                {
                    More = new Dictionary<string, object>
                    {
                        ["error"] = $"Invalid JSON: {ex.Message}",
                    },
                };
            }

            Document = serializer.Serialize(content);
        }
        else
        {
            Document = null;
        }

        return base.PrepareAsync(executionContext, ct);
    }

    public override ValueTask<FlowStepResult> ExecuteAsync(FlowExecutionContext executionContext,
        CancellationToken ct)
    {
        throw new NotImplementedException();
    }

    public sealed class ElasticSearchContent
    {
        public string ContentId { get; set; }

        [JsonExtensionData]
        public Dictionary<string, object> More { get; set; } = [];
    }
}
