// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Text.Json.Serialization;
using Squidex.Domain.Apps.Core;
using Squidex.Infrastructure.Json;

namespace Squidex.Domain.Apps.Entities.Contents.Queries.Steps;

public sealed class CalculateTokens(IUrlGenerator urlGenerator, IJsonSerializer serializer) : IContentEnricherStep
{
    // We have to use these short names here because they are later read like this.
    private sealed class Token
    {
        [JsonPropertyName("a")]
        public string App { get; set; }

        [JsonPropertyName("s")]
        public string Schema { get; set; }

        [JsonPropertyName("i")]
        public string Id { get; set; }

        [JsonPropertyName("u")]
        public string Url { get; set; }
    }

    public Task EnrichAsync(Context context, IEnumerable<EnrichedContent> contents, ProvideSchema schemas,
        CancellationToken ct)
    {
        // Only the schema and the ID are different for each content, so the token is reused.
        var token = new Token { Url = urlGenerator.Root() };

        foreach (var content in contents)
        {
            token.Id = content.Id.ToString();
            token.App = content.AppId.Name;
            token.Schema = content.SchemaId.Name;

            var json = serializer.SerializeToBytes(token);

            content.EditToken = Convert.ToBase64String(json);
        }

        return Task.CompletedTask;
    }
}
