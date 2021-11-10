// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using MongoDB.Bson.Serialization;
using MongoDB.Driver;
using MongoDB.Labs.Search;
using Squidex.Domain.Apps.Entities.Apps;
using Squidex.Domain.Apps.Entities.Contents;
using Squidex.Domain.Apps.Entities.Contents.Text;
using Squidex.Infrastructure;

namespace Squidex.Domain.Apps.Entities.MongoDb.FullText
{
    public sealed class AtlasTextIndex : MongoTextIndexBase<Dictionary<string, string>>
    {
        private readonly AtlasOptions options;

        public AtlasTextIndex(IMongoDatabase database, IOptions<AtlasOptions> options, bool setup = false)
            : base(database, setup)
        {
            this.options = options.Value;
        }

        protected override async Task SetupCollectionAsync(IMongoCollection<MongoTextIndexEntity<Dictionary<string, string>>> collection,
            CancellationToken ct)
        {
            await base.SetupCollectionAsync(collection, ct);

            await AtlasIndexDefinition.CreateIndexAsync(options,
                Database.DatabaseNamespace.DatabaseName, CollectionName(), ct);
        }

        protected override Dictionary<string, string> BuildTexts(Dictionary<string, string> source)
        {
            var texts = new Dictionary<string, string>();

            foreach (var (key, value) in source)
            {
                var text = value;

                var languageCode = AtlasIndexDefinition.GetTextField(key);

                if (texts.TryGetValue(languageCode, out var existing))
                {
                    text = $"{existing} {value}";
                }

                texts[languageCode] = text;
            }

            return texts;
        }

        public override async Task<List<DomainId>?> SearchAsync(IAppEntity app, TextQuery query, SearchScope scope,
            CancellationToken ct = default)
        {
            Guard.NotNull(app, nameof(app));
            Guard.NotNull(query, nameof(query));

            var (search, take) = query;

            if (string.IsNullOrWhiteSpace(search))
            {
                return null;
            }

            var builder = SearchBuilders<MongoTextIndexEntity<Dictionary<string, string>>>.Search;

            var searchDefinitions = new List<SearchDefinition<MongoTextIndexEntity<Dictionary<string, string>>>>
            {
                builder.QueryString(x => x.Texts, search),
                builder.Text(new SingleQueryDefinition(app.Id.ToString()), x => x.AppId)
            };

            if (scope == SearchScope.All)
            {
                searchDefinitions.Add(builder.Eq(x => x.ServeAll, true));
            }
            else
            {
                searchDefinitions.Add(builder.Eq(x => x.ServePublished, true));
            }

            if (query.PreferredSchemaId != null)
            {
                searchDefinitions.Add(
                    builder.Should(
                        builder.Text(new SingleQueryDefinition(query.PreferredSchemaId.Value.ToString()), x => x.SchemaId)));
            }
            else if (query.RequiredSchemaIds != null)
            {
                // var queryString = string.Join(" OR ", query.RequiredSchemaIds.Select(x => $"_si: {x}"));

                // searchDefinitions.Add(
                 //   builder.Filter(queryString));
            }

            var searchQuery = builder.Filter(searchDefinitions);


        var registry = BsonSerializer.SerializerRegistry;
        var serializer = BsonSerializer.SerializerRegistry.GetSerializer<MongoTextIndexEntity<Dictionary<string, string>>>();
            var x = searchQuery.Render(serializer, registry);

            var results =
                await Collection.Aggregate().Search(searchQuery).Limit(take)
                    .Project<MongoTextResult>(
                        Projection.Include(x => x.ContentId)
                    )
                    .ToListAsync(ct);

            return results.Select(x => x.ContentId).ToList();
        }
    }
}
