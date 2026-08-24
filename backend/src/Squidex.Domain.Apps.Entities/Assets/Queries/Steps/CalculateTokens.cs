// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Text.Json.Serialization;
using Squidex.Domain.Apps.Core;
using Squidex.Infrastructure.Json;

namespace Squidex.Domain.Apps.Entities.Assets.Queries.Steps;

public sealed class CalculateTokens(IUrlGenerator urlGenerator, IJsonSerializer serializer) : IAssetEnricherStep
{
    // We have to use these short names here because they are later read like this.
    private sealed class Token
    {
        [JsonPropertyName("a")]
        public string App { get; set; }

        [JsonPropertyName("i")]
        public string Id { get; set; }

        [JsonPropertyName("u")]
        public string Url { get; set; }
    }

    public Task EnrichAsync(Context context, IEnumerable<EnrichedAsset> assets,
        CancellationToken ct)
    {
        if (context.NoAssetEnrichment())
        {
            return Task.CompletedTask;
        }

        // Only the ID is different for each asset, so the token is reused for all of them.
        var token = new Token { Url = urlGenerator.Root() };

        foreach (var asset in assets)
        {
            token.App = asset.AppId.Name;
            token.Id = asset.Id.ToString();

            var json = serializer.SerializeToBytes(token);

            asset.EditToken = Convert.ToBase64String(json);
        }

        return Task.CompletedTask;
    }
}
