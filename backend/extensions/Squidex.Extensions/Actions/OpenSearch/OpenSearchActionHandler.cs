// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Text.Json.Serialization;
using OpenSearch.Net;
using Squidex.Domain.Apps.Core.HandleRules;
using Squidex.Domain.Apps.Core.Rules.EnrichedEvents;
using Squidex.Domain.Apps.Core.Scripting;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Json;

#pragma warning disable IDE0059 // Value assigned to symbol is never used
#pragma warning disable MA0048 // File name must match type name

namespace Squidex.Extensions.Actions.OpenSearch;

public sealed class OpenSearchActionHandler : RuleActionHandler<OpenSearchAction, OpenSearchJob>
{
    private readonly ClientPool<(Uri Host, string Username, string Password), OpenSearchLowLevelClient> clients;
    private readonly IScriptEngine scriptEngine;
    private readonly IJsonSerializer serializer;

    public OpenSearchActionHandler(RuleEventFormatter formatter, IScriptEngine scriptEngine, IJsonSerializer serializer)
        : base(formatter)
    {
        clients = new ClientPool<(Uri Host, string Username, string Password), OpenSearchLowLevelClient>(key =>
        {
            var config = new ConnectionConfiguration(key.Host);

            if (!string.IsNullOrEmpty(key.Username) && !string.IsNullOrWhiteSpace(key.Password))
            {
                config = config.BasicAuthentication(key.Username, key.Password);
            }

            return new OpenSearchLowLevelClient(config);
        });

        this.scriptEngine = scriptEngine;
        this.serializer = serializer;
    }

    protected override async Task<(string Description, OpenSearchJob Data)> CreateJobAsync(EnrichedEvent @event, OpenSearchAction action)
    {
        var delete = @event.ShouldDelete(scriptEngine, action.Delete);

        string contentId;

        if (@event is IEnrichedEntityEvent enrichedEntityEvent)
        {
            contentId = enrichedEntityEvent.Id.ToString();
        }
        else
        {
            contentId = DomainId.NewGuid().ToString();
        }

        var ruleDescription = string.Empty;
        var ruleJob = new OpenSearchJob
        {
            IndexName = await FormatAsync(action.IndexName, @event),
            ServerHost = action.Host.ToString(),
            ServerUser = action.Username,
            ServerPassword = action.Password,
            ContentId = contentId
        };

        if (delete)
        {
            ruleDescription = $"Delete entry index: {action.IndexName}";
        }
        else
        {
            ruleDescription = $"Upsert to index: {action.IndexName}";

            OpenSearchContent content;
            try
            {
                string jsonString;

                if (!string.IsNullOrEmpty(action.Document))
                {
                    jsonString = await FormatAsync(action.Document, @event);
                    jsonString = jsonString?.Trim();
                }
                else
                {
                    jsonString = ToJson(@event);
                }

                content = serializer.Deserialize<OpenSearchContent>(jsonString);
            }
            catch (Exception ex)
            {
                content = new OpenSearchContent
                {
                    More = new Dictionary<string, object>
                    {
                        ["error"] = $"Invalid JSON: {ex.Message}"
                    }
                };
            }

            content.ContentId = contentId;

            ruleJob.Content = serializer.Serialize(content, true);
        }

        return (ruleDescription, ruleJob);
    }

    protected override async Task<Result> ExecuteJobAsync(OpenSearchJob job,
        CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(job.ServerHost))
        {
            return Result.Ignored();
        }

        var client = await clients.GetClientAsync((new Uri(job.ServerHost, UriKind.Absolute), job.ServerUser, job.ServerPassword));

        try
        {
            if (job.Content != null)
            {
                var response = await client.IndexAsync<StringResponse>(job.IndexName, job.ContentId, job.Content, ctx: ct);

                return Result.SuccessOrFailed(response.OriginalException, response.Body);
            }
            else
            {
                var response = await client.DeleteAsync<StringResponse>(job.IndexName, job.ContentId, ctx: ct);

                return Result.SuccessOrFailed(response.OriginalException, response.Body);
            }
        }
        catch (OpenSearchClientException ex)
        {
            return Result.Failed(ex);
        }
    }
}

public sealed class OpenSearchContent
{
    public string ContentId { get; set; }

    [JsonExtensionData]
    public Dictionary<string, object> More { get; set; } = new Dictionary<string, object>();
}

public sealed class OpenSearchJob
{
    public string ServerHost { get; set; }

    public string ServerUser { get; set; }

    public string ServerPassword { get; set; }

    public string ContentId { get; set; }

    public string Content { get; set; }

    public string IndexName { get; set; }
}
