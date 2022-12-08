// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Text;
using Squidex.Domain.Apps.Core;
using Squidex.Infrastructure.Json;

namespace Squidex.Domain.Apps.Entities.Contents.Queries.Steps;

public sealed class CalculateTokens : IContentEnricherStep
{
    private readonly IJsonSerializer serializer;
    private readonly IUrlGenerator urlGenerator;

    public CalculateTokens(IUrlGenerator urlGenerator, IJsonSerializer serializer)
    {
        this.serializer = serializer;
        this.urlGenerator = urlGenerator;
    }

    public Task EnrichAsync(Context context, IEnumerable<ContentEntity> contents, ProvideSchema schemas,
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
                u = url
            };

            var json = serializer.Serialize(token);

            content.EditToken = Convert.ToBase64String(Encoding.UTF8.GetBytes(json));
        }

        return Task.CompletedTask;
    }
}
