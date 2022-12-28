﻿// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Text;
using Squidex.Domain.Apps.Core;
using Squidex.Infrastructure.Json;

namespace Squidex.Domain.Apps.Entities.Assets.Queries.Steps;

public sealed class CalculateTokens : IAssetEnricherStep
{
    private readonly IJsonSerializer serializer;
    private readonly IUrlGenerator urlGenerator;

    public CalculateTokens(IUrlGenerator urlGenerator, IJsonSerializer serializer)
    {
        this.serializer = serializer;
        this.urlGenerator = urlGenerator;
    }

    public Task EnrichAsync(Context context, IEnumerable<AssetEntity> assets,
        CancellationToken ct)
    {
        if (!context.ShouldSkipAssetEnrichment())
        {
            return Task.CompletedTask;
        }

        var url = urlGenerator.Root();

        foreach (var asset in assets)
        {
            // We have to use these short names here because they are later read like this.
            var token = new
            {
                a = asset.AppId.Name,
                i = asset.Id.ToString(),
                u = url
            };

            var json = serializer.Serialize(token);

            asset.EditToken = Convert.ToBase64String(Encoding.UTF8.GetBytes(json));
        }

        return Task.CompletedTask;
    }
}
