// ==========================================================================
//  GraphQLQueryContext.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using Squidex.Domain.Apps.Read.Apps;
using Squidex.Domain.Apps.Read.Assets;
using Squidex.Domain.Apps.Read.Assets.Repositories;

namespace Squidex.Domain.Apps.Read.Contents.GraphQL
{
    public sealed class GraphQLQueryContext : QueryContext
    {
        public IGraphQLUrlGenerator UrlGenerator { get; }

        public GraphQLQueryContext(IAppEntity app, IAssetRepository assetRepository, IContentQueryService contentQuery, ClaimsPrincipal user,
            IGraphQLUrlGenerator urlGenerator)
            : base(app, assetRepository, contentQuery, user)
        {
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
