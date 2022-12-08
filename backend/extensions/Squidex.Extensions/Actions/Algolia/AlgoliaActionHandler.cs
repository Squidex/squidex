// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Text.Json.Serialization;
using Algolia.Search.Clients;
using Newtonsoft.Json.Linq;
using Squidex.Domain.Apps.Core.HandleRules;
using Squidex.Domain.Apps.Core.Rules.EnrichedEvents;
using Squidex.Domain.Apps.Core.Scripting;
using Squidex.Infrastructure.Json;

#pragma warning disable IDE0059 // Value assigned to symbol is never used
#pragma warning disable MA0048 // File name must match type name

namespace Squidex.Extensions.Actions.Algolia;

public sealed class AlgoliaActionHandler : RuleActionHandler<AlgoliaAction, AlgoliaJob>
{
    private readonly ClientPool<(string AppId, string ApiKey, string IndexName), ISearchIndex> clients;
    private readonly IScriptEngine scriptEngine;
    private readonly IJsonSerializer serializer;

    public AlgoliaActionHandler(RuleEventFormatter formatter, IScriptEngine scriptEngine, IJsonSerializer serializer)
        : base(formatter)
    {
        clients = new ClientPool<(string AppId, string ApiKey, string IndexName), ISearchIndex>(key =>
        {
            var client = new SearchClient(key.AppId, key.ApiKey);

            return client.InitIndex(key.IndexName);
        });

        this.scriptEngine = scriptEngine;
        this.serializer = serializer;
    }

    protected override async Task<(string Description, AlgoliaJob Data)> CreateJobAsync(EnrichedEvent @event, AlgoliaAction action)
    {
        if (@event is IEnrichedEntityEvent entityEvent)
        {
            var delete = @event.ShouldDelete(scriptEngine, action.Delete);

            var ruleDescription = string.Empty;
            var contentId = entityEvent.Id.ToString();
            var content = (AlgoliaContent)null;

            if (delete)
            {
                ruleDescription = $"Delete entry from Algolia index: {action.IndexName}";
            }
            else
            {
                ruleDescription = $"Add entry to Algolia index: {action.IndexName}";

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

                    content = serializer.Deserialize<AlgoliaContent>(jsonString);
                }
                catch (Exception ex)
                {
                    content = new AlgoliaContent
                    {
                        More = new Dictionary<string, object>
                        {
                            ["error"] = $"Invalid JSON: {ex.Message}"
                        }
                    };
                }

                content.ObjectID = contentId;
            }

            var ruleJob = new AlgoliaJob
            {
                AppId = action.AppId,
                ApiKey = action.ApiKey,
                Content = serializer.Serialize(content, true),
                ContentId = contentId,
                IndexName = await FormatAsync(action.IndexName, @event)
            };

            return (ruleDescription, ruleJob);
        }

        return ("Ignore", new AlgoliaJob());
    }

    protected override async Task<Result> ExecuteJobAsync(AlgoliaJob job,
        CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(job.AppId))
        {
            return Result.Ignored();
        }

        var index = await clients.GetClientAsync((job.AppId, job.ApiKey, job.IndexName));

        try
        {
            if (job.Content != null)
            {
                var raw = new[]
                {
                    new JRaw(job.Content)
                };

                var response = await index.SaveObjectsAsync(raw, null, ct, true);

                return Result.Success(serializer.Serialize(response, true));
            }
            else
            {
                var response = await index.DeleteObjectAsync(job.ContentId, null, ct);

                return Result.Success(serializer.Serialize(response, true));
            }
        }
        catch (Exception ex)
        {
            return Result.Failed(ex);
        }
    }
}

public sealed class AlgoliaContent
{
    [JsonPropertyName("objectID")]
    public string ObjectID { get; set; }

    [JsonExtensionData]
    public Dictionary<string, object> More { get; set; } = new Dictionary<string, object>();
}

public sealed class AlgoliaJob
{
    public string AppId { get; set; }

    public string ApiKey { get; set; }

    public string ContentId { get; set; }

    public string IndexName { get; set; }

    public string Content { get; set; }
}
