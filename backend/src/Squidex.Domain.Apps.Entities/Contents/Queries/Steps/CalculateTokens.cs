// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Domain.Apps.Core;
using Squidex.Infrastructure.Json;

namespace Squidex.Domain.Apps.Entities.Contents.Queries.Steps;

public sealed class CalculateTokens(IUrlGenerator urlGenerator, IJsonSerializer serializer) : IContentEnricherStep
{
    public Task EnrichAsync(Context context, IEnumerable<EnrichedContent> contents, ProvideSchema schemas,
        CancellationToken ct)
    {
        var url = urlGenerator.Root();

        foreach (var content in contents)
        {
            // We have to use these short names here because they are later read like this.
            var token = new
            {
                a = content.AppId.Name,
                s = content.SchemaId.Name,
                i = content.Id.ToString(),
                u = url,
            };

            var json = serializer.SerializeToBytes(token);

            content.EditToken = Convert.ToBase64String(json);
        }

        return Task.CompletedTask;
    }
}
