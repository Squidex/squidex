// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Microsoft.Azure.CognitiveServices.Vision.ComputerVision;
using Microsoft.Azure.CognitiveServices.Vision.ComputerVision.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Squidex.Domain.Apps.Core.Assets;
using Squidex.Domain.Apps.Entities.Assets;
using Squidex.Domain.Apps.Entities.Assets.Commands;
using Squidex.Infrastructure.Json.Objects;

namespace Squidex.Extensions.Assets.Azure;

public sealed class AzureMetadataSource : IAssetMetadataSource
{
    private const long MaxSize = 5 * 1025 * 1024;
    private readonly ILogger<AzureMetadataSource> log;
    private readonly ComputerVisionClient client;
    private readonly char[] trimChars =
    {
        ' ',
        '_',
        '-'
    };
    private readonly List<VisualFeatureTypes?> features = new List<VisualFeatureTypes?>
    {
        VisualFeatureTypes.Categories,
        VisualFeatureTypes.Description,
        VisualFeatureTypes.Color
    };

    public int Order => int.MaxValue;

    public AzureMetadataSource(IOptions<AzureMetadataSourceOptions> options,
        ILogger<AzureMetadataSource> log)
    {
        client = new ComputerVisionClient(new ApiKeyServiceClientCredentials(options.Value.ApiKey))
        {
            Endpoint = options.Value.Endpoint
        };

        this.log = log;
    }

    public async Task EnhanceAsync(UploadAssetCommand command,
        CancellationToken ct)
    {
        try
        {
            if (command.Type == AssetType.Image && command.File.FileSize <= MaxSize)
            {
                await using (var stream = command.File.OpenRead())
                {
                    var result = await client.AnalyzeImageInStreamAsync(stream, features, cancellationToken: ct);

                    command.Tags ??= new HashSet<string>();

                    if (result.Color?.DominantColorForeground != null)
                    {
                        command.Tags.Add($"color/{result.Color.DominantColorForeground.Trim(trimChars).ToLowerInvariant()}");
                    }

                    if (result.Categories != null)
                    {
                        foreach (var category in result.Categories.OrderByDescending(x => x.Score).Take(3))
                        {
                            command.Tags.Add($"category/{category.Name.Trim(trimChars).ToLowerInvariant()}");
                        }
                    }

                    var description = result.Description?.Captions.MaxBy(x => x.Confidence)?.Text;

                    if (description != null)
                    {
                        command.Metadata["caption"] = JsonValue.Create(description);
                    }
                }
            }
        }
        catch (Exception ex)
        {
            log.LogError(ex, "Failed to enrich asset.");
        }
    }

    public IEnumerable<string> Format(IAssetEntity asset)
    {
        yield break;
    }
}
