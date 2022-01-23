// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Text;
using Squidex.Domain.Apps.Core;
using Squidex.Infrastructure.Json;

namespace Squidex.Domain.Apps.Entities.Contents.Queries.Steps
{
    public sealed class CalculateTokens : IContentEnricherStep
    {
        private readonly IJsonSerializer jsonSerializer;
        private readonly IUrlGenerator urlGenerator;

        public CalculateTokens(IUrlGenerator urlGenerator, IJsonSerializer jsonSerializer)
        {
            this.jsonSerializer = jsonSerializer;
            this.urlGenerator = urlGenerator;
        }

        public Task EnrichAsync(Context context, IEnumerable<ContentEntity> contents, ProvideSchema schemas,
            CancellationToken ct)
        {
            if (context.IsFrontendClient)
            {
                return Task.CompletedTask;
            }

            var url = urlGenerator.Root();

            foreach (var content in contents)
            {
                var token = new
                {
                    a = content.AppId.Name,
                    s = content.SchemaId.Name,
                    i = content.Id.ToString(),
                    u = url
                };

                var json = jsonSerializer.Serialize(token);

                content.EditToken = Convert.ToBase64String(Encoding.UTF8.GetBytes(json));
            }

            return Task.CompletedTask;
        }
    }
}
