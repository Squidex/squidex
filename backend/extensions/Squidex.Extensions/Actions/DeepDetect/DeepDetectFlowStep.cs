// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.ComponentModel.DataAnnotations;
using System.Net.Http.Json;
using System.Text.RegularExpressions;
using Google.Apis.Json;
using Squidex.Domain.Apps.Core;
using Squidex.Domain.Apps.Core.Assets;
using Squidex.Domain.Apps.Core.HandleRules;
using Squidex.Domain.Apps.Core.Rules.EnrichedEvents;
using Squidex.Domain.Apps.Entities;
using Squidex.Domain.Apps.Entities.Assets;
using Squidex.Domain.Apps.Entities.Assets.Commands;
using Squidex.Flows;
using Squidex.Infrastructure.Commands;
using Squidex.Text;

namespace Squidex.Extensions.Actions.DeepDetect;

[FlowStep(
    Title = "DeepDetect",
    IconImage = "<svg viewBox='0 0 28 28' xmlns='http://www.w3.org/2000/svg'><g style='stroke-width:1.24962' fill='none'><path fill='#ff5252' d='M13 21.92H0v-8.032h9.386V10.92h3.57v11zm-9.386-4.889v1.702H9.43v-1.702z' style='stroke-width:1.24962' transform='matrix(.78667 0 0 .81405 2.529 2.668)'/><path fill='#fff' d='M29.164 21.92h-13V14.028H25.7V5.92h3.464zm-9.536-4.804v1.673H25.7v-1.673z' style='stroke-width:1.24962' transform='matrix(.78667 0 0 .81405 2.529 2.668)'/></g></svg>",
    IconColor = "#526a75",
    Display = "Annotate image",
    Description = "Annotate an image using deep detect.")]
internal sealed partial record DeepDetectFlowStep : FlowStep
{
    [Display(Name = "Min Propability", Description = "The minimum probability for objects to be recognized (0 - 100).")]
    [Editor(FlowStepEditor.Number)]
    public long MinimumPropability { get; set; }

    [Display(Name = "Max Tags", Description = "The maximum number of tags to use.")]
    [Editor(FlowStepEditor.Number)]
    public long MaximumTags { get; set; }

    public override async ValueTask<FlowStepResult> ExecuteAsync(FlowExecutionContext executionContext,
        CancellationToken ct)
    {
        var @event = ((FlowEventContext)executionContext.Context).Event;
        if (@event is not EnrichedAssetEvent assetEvent)
        {
            executionContext.Log("Ignored: Invalid event.");
            return Next();
        }

        if (assetEvent.AssetType != AssetType.Image)
        {
            executionContext.Log("Ignored: Invalid event (not an image).");
            return Next();
        }

        var urlToDownload =
            executionContext.Resolve<IUrlGenerator>()
                .AssetContent(assetEvent.AppId, assetEvent.Id.ToString(), assetEvent.FileVersion);

        var httpClient =
            executionContext.Resolve<IHttpClientFactory>()
                .CreateClient("DeepDetect");

        var response = await httpClient.PostAsJsonAsync("predict", new
        {
            service = "squidexdetector",
            output = new
            {
                best = MaximumTags,
                confidence_threshold = MinimumPropability / 100d,
            },
            data = new[]
            {
                urlToDownload,
            },
        }, ct);

        var responseBody = await response.Content.ReadAsStringAsync(ct);
        if (!response.IsSuccessStatusCode)
        {
            executionContext.Log($"Failed with status code {response.StatusCode}", responseBody);
            response.EnsureSuccessStatusCode();
        }

        var jsonSerializer = executionContext.Resolve<IJsonSerializer>();
        var jsonResponse = jsonSerializer.Deserialize<DetectResponse>(responseBody);

        var tags = jsonResponse!.Body.Predictions.SelectMany(x => x.Classes);
        if (!tags.Any())
        {
            executionContext.Log("Warning: No tags returned.", responseBody);
            return Next();
        }

        var app =
            await executionContext.Resolve<IAppProvider>()
                .GetAppAsync(assetEvent.AppId.Id, true, ct);

        if (app == null)
        {
            executionContext.Log("Ignored: App not found.");
            return Next();
        }

        var context = Context.Admin(app);

        var asset =
            await executionContext.Resolve<IAssetQueryService>()
                .FindAsync(context, assetEvent.Id, ct: ct);

        if (asset == null)
        {
            executionContext.Log("Ignored: Asset not found.");
            return Next();
        }

        var command = new AnnotateAsset
        {
            Tags = asset.TagNames,
            AssetId = asset.Id,
            AppId = asset.AppId,
            Actor = assetEvent.Actor,
            FromRule = true,
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

        await executionContext.Resolve<ICommandBus>()
            .PublishAsync(command, ct);

        executionContext.Log("Tags Added.", responseBody);
        return Next();
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
