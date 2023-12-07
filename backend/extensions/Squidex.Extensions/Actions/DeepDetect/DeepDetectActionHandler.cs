// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Net.Http.Json;
using System.Text.RegularExpressions;
using Squidex.Domain.Apps.Core;
using Squidex.Domain.Apps.Core.Assets;
using Squidex.Domain.Apps.Core.HandleRules;
using Squidex.Domain.Apps.Core.Rules.EnrichedEvents;
using Squidex.Domain.Apps.Entities;
using Squidex.Domain.Apps.Entities.Assets;
using Squidex.Domain.Apps.Entities.Assets.Commands;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Commands;
using Squidex.Infrastructure.Json;
using Squidex.Text;

namespace Squidex.Extensions.Actions.DeepDetect;

#pragma warning disable MA0048 // File name must match type name

internal sealed partial class DeepDetectActionHandler : RuleActionHandler<DeepDetectAction, DeepDetectJob>
{
    private const string Description = "Analyze Image";
    private readonly IHttpClientFactory httpClientFactory;
    private readonly IJsonSerializer jsonSerializer;
    private readonly IAppProvider appProvider;
    private readonly IAssetQueryService assetQuery;
    private readonly ICommandBus commandBus;
    private readonly IUrlGenerator urlGenerator;

    public DeepDetectActionHandler(RuleEventFormatter formatter, IHttpClientFactory httpClientFactory,
        IJsonSerializer jsonSerializer,
        IAppProvider appProvider,
        IAssetQueryService assetQuery,
        ICommandBus commandBus,
        IUrlGenerator urlGenerator)
        : base(formatter)
    {
        this.httpClientFactory = httpClientFactory;
        this.jsonSerializer = jsonSerializer;
        this.appProvider = appProvider;
        this.assetQuery = assetQuery;
        this.commandBus = commandBus;
        this.urlGenerator = urlGenerator;
    }

    protected override Task<(string Description, DeepDetectJob Data)> CreateJobAsync(EnrichedEvent @event, DeepDetectAction action)
    {
        if (@event is not EnrichedAssetEvent assetEvent)
        {
            return Task.FromResult(("Ignore", new DeepDetectJob()));
        }

        if (assetEvent.AssetType != AssetType.Image)
        {
            return Task.FromResult(("Ignore", new DeepDetectJob()));
        }

        var ruleJob = new DeepDetectJob
        {
            Actor = assetEvent.Actor,
            AppId = assetEvent.AppId.Id,
            AssetId = assetEvent.Id,
            MaximumTags = action.MaximumTags,
            MinimumPropability = action.MinimumProbability,
            Url = urlGenerator.AssetContent(assetEvent.AppId, assetEvent.Id.ToString(), assetEvent.FileVersion)
        };

        return Task.FromResult((Description, ruleJob));
    }

    protected override async Task<Result> ExecuteJobAsync(DeepDetectJob job,
        CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(job.Url))
        {
            return Result.Ignored();
        }

        var httpClient = httpClientFactory.CreateClient("DeepDetect");

        var response = await httpClient.PostAsJsonAsync("predict", new
        {
            service = "squidexdetector",
            output = new
            {
                best = job.MaximumTags,
                confidence_threshold = job.MinimumPropability / 100d,
            },
            data = new[]
            {
                job.Url,
            }
        }, ct);

        var body = await response.Content.ReadAsStringAsync(ct);

        if (!response.IsSuccessStatusCode)
        {
            return Result.Failed(new InvalidOperationException($"Failed with status code {response.StatusCode}\n\n{body}"));
        }

        var responseJson = jsonSerializer.Deserialize<DetectResponse>(body);

        var tags = responseJson!.Body.Predictions.SelectMany(x => x.Classes);

        if (!tags.Any())
        {
            return Result.Success(body);
        }

        var app = await appProvider.GetAppAsync(job.AppId, true, ct);
        if (app == null)
        {
            return Result.Failed(new InvalidOperationException("App not found."));
        }

        var context = Context.Admin(app);

        var asset = await assetQuery.FindAsync(context, job.AssetId, ct: ct);
        if (asset == null)
        {
            return Result.Failed(new InvalidOperationException("Asset not found."));
        }

        var command = new AnnotateAsset
        {
            Tags = asset.TagNames,
            AssetId = asset.Id,
            AppId = asset.AppId,
            Actor = job.Actor,
            FromRule = true
        };

        foreach (var tag in tags)
        {
            var tagParts = tag.Cat.Split(',')[0].Split(' ', StringSplitOptions.RemoveEmptyEntries);

            if (IdRegex().IsMatch(tagParts[0]))
            {
                tagParts = tagParts.Skip(1).ToArray();
            }

            var tagName = string.Join('_', tagParts.Select(x => x.Slugify()));

            command.Tags.Add($"ai/{tagName}");
        }

        await commandBus.PublishAsync(command, ct);
        return Result.Success(body);
    }

    private sealed class DetectResponse
    {
        public DetectBody Body { get; set; }
    }

    private sealed class DetectBody
    {
        public DetectPredications[] Predictions { get; set; }
    }

    private sealed class DetectPredications
    {
        public DetectClass[] Classes { get; set; }
    }

    private sealed class DetectClass
    {
        public double Prob { get; set; }

        public string Cat { get; set; }
    }

    [GeneratedRegex("^n[0-9]+$")]
    private static partial Regex IdRegex();
}

public sealed class DeepDetectJob
{
    public DomainId AppId { get; set; }

    public DomainId AssetId { get; set; }

    public RefToken Actor { get; set; }

    public long MaximumTags { get; set; }

    public long MinimumPropability { get; set; }

    public string? Url { get; set; }
}
