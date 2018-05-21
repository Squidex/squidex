// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using Squidex.Domain.Apps.Entities.Assets;
using Squidex.Domain.Apps.Entities.Assets.Repositories;
using Squidex.Infrastructure.Commands;

namespace Squidex.Domain.Apps.Entities.Contents.GraphQL
{
    public sealed class GraphQLExecutionContext : QueryExecutionContext
    {
        public ICommandBus CommandBus { get; }

        public IGraphQLUrlGenerator UrlGenerator { get; }

        public GraphQLExecutionContext(QueryContext context,
            IAssetRepository assetRepository,
            ICommandBus commandBus,
            IContentQueryService contentQuery,
            IGraphQLUrlGenerator urlGenerator)
            : base(context, assetRepository, contentQuery)
        {
            CommandBus = commandBus;

            UrlGenerator = urlGenerator;
        }

        public Task<IReadOnlyList<IAssetEntity>> GetReferencedAssetsAsync(JToken value)
        {
            var ids = ParseIds(value);

            return GetReferencedAssetsAsync(ids);
        }

        public Task<IReadOnlyList<IContentEntity>> GetReferencedContentsAsync(Guid schemaId, JToken value)
        {
            var ids = ParseIds(value);

            return GetReferencedContentsAsync(schemaId, ids);
        }

        private static ICollection<Guid> ParseIds(JToken value)
        {
            try
            {
                var result = new List<Guid>();

                if (value is JArray)
                {
                    foreach (var id in value)
                    {
                        result.Add(Guid.Parse(id.ToString()));
                    }
                }

                return result;
            }
            catch
            {
                return new List<Guid>();
            }
        }
    }
}
