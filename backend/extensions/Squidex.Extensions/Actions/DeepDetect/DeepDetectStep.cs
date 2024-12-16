// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.ComponentModel.DataAnnotations;
using System.Net.Http.Json;
using System.Text.RegularExpressions;
using Squidex.Domain.Apps.Core;
using Squidex.Domain.Apps.Core.Assets;
using Squidex.Domain.Apps.Core.HandleRules;
using Squidex.Domain.Apps.Core.Rules.EnrichedEvents;
using Squidex.Domain.Apps.Entities;
using Squidex.Domain.Apps.Entities.Assets;
using Squidex.Domain.Apps.Entities.Assets.Commands;
using Squidex.Flows;
using Squidex.Infrastructure.Commands;
using Squidex.Infrastructure.Json;
using Squidex.Text;

namespace Squidex.Extensions.Actions.DeepDetect;

[FlowStep(
    Title = "DeepDetect",
    IconImage = "<svg viewBox='0 0 28 28' xmlns='http://www.w3.org/2000/svg'><g style='stroke-width:1.24962' fill='none'><path fill='#ff5252' d='M13 21.92H0v-8.032h9.386V10.92h3.57v11zm-9.386-4.889v1.702H9.43v-1.702z' style='stroke-width:1.24962' transform='matrix(.78667 0 0 .81405 2.529 2.668)'/><path fill='#fff' d='M29.164 21.92h-13V14.028H25.7V5.92h3.464zm-9.536-4.804v1.673H25.7v-1.673z' style='stroke-width:1.24962' transform='matrix(.78667 0 0 .81405 2.529 2.668)'/></g></svg>",
    IconColor = "#526a75",
    Display = "Annotate image",
    Description = "Annotate an image using deep detect.")]
public partial class DeepDetectStep : RuleFlowStep<EnrichedAssetEvent>
{
    [Display(Name = "Min Probability", Description = "The minimum probability for objects to be recognized (0 - 100).")]
    [Editor(FlowStepEditor.Number)]
    public long MinimumProbability { get; set; }

    [Display(Name = "Max Tags", Description = "The maximum number of tags to use.")]
    [Editor(FlowStepEditor.Number)]
    public long MaximumTags { get; set; }

    protected override async ValueTask<FlowStepResult> ExecuteAsync(RuleFlowContext context, EnrichedAssetEvent @event, FlowExecutionContext executionContext,
        CancellationToken ct)
    {
        var httpClient = executionContext.Resolve<IHttpClientFactory>().CreateClient("DeepDetect");

        var urlGenerator = executionContext.Resolve<IUrlGenerator>();
        var response = await httpClient.PostAsJsonAsync("predict", new
        {
            service = "squidexdetector",
            output = new
            {
                best = MaximumTags,
                confidence_threshold = MinimumProbability / 100d,
            },
            data = new[]
            {
                urlGenerator.AssetContent(@event.AppId, @event.Id.ToString(), @event.FileVersion),
            }
        }, ct);

        var body = await response.Content.ReadAsStringAsync(ct);

        if (!response.IsSuccessStatusCode)
        {
            throw new InvalidOperationException($"Failed with status code {response.StatusCode}\n\n{body}");
        }

        var jsonSerializer = executionContext.Resolve<IJsonSerializer>();
        var jsonResponse = jsonSerializer.Deserialize<DetectResponse>(body);

        var tags = jsonResponse!.Body.Predictions.SelectMany(x => x.Classes);

        if (!tags.Any())
        {
            executionContext.Log("No tags found", tags);
            return FlowStepResult.Next();
        }

        var appProvider = executionContext.Resolve<IAppProvider>();

        var app = await appProvider.GetAppAsync(@event.AppId.Id, true, ct)
            ?? throw new InvalidOperationException("App not found.");

        var assetContext = Context.Admin(app);
        var assetQuery = executionContext.Resolve<IAssetQueryService>();

        var asset = await assetQuery.FindAsync(assetContext, @event.Id, ct: ct)
            ?? throw new InvalidOperationException("Asset not found.");

        var command = new AnnotateAsset
        {
            FromRule = true,
            Actor = @event.Actor,
            AppId = asset.AppId,
            AssetId = asset.Id,
            Tags = asset.TagNames,
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

        var commandBus = executionContext.Resolve<ICommandBus>();
        await commandBus.PublishAsync(command, ct);

        return FlowStepResult.Next();
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
