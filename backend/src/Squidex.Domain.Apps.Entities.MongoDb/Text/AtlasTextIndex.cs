// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Lucene.Net.Analysis.Standard;
using Lucene.Net.Analysis.Util;
using Lucene.Net.Util;
using Microsoft.Extensions.Options;
using MongoDB.Bson;
using MongoDB.Driver;
using Squidex.Domain.Apps.Entities.Apps;
using Squidex.Domain.Apps.Entities.Contents;
using Squidex.Domain.Apps.Entities.Contents.Text;
using Squidex.Infrastructure;
using LuceneQueryAnalyzer = Lucene.Net.QueryParsers.Classic.QueryParser;

namespace Squidex.Domain.Apps.Entities.MongoDb.Text;

public sealed class AtlasTextIndex : MongoTextIndexBase<Dictionary<string, string>>
{
    private static readonly LuceneQueryVisitor QueryVisitor = new LuceneQueryVisitor(AtlasIndexDefinition.GetFieldPath);
    private static readonly LuceneQueryAnalyzer QueryParser =
        new LuceneQueryAnalyzer(LuceneVersion.LUCENE_48, "*",
            new StandardAnalyzer(LuceneVersion.LUCENE_48, CharArraySet.EMPTY_SET));
    private readonly AtlasOptions options;
    private string index;

    public AtlasTextIndex(IMongoDatabase database, IOptions<AtlasOptions> options)
        : base(database)
    {
        this.options = options.Value;
    }

    protected override async Task SetupCollectionAsync(IMongoCollection<MongoTextIndexEntity<Dictionary<string, string>>> collection,
        CancellationToken ct)
    {
        await base.SetupCollectionAsync(collection, ct);

        index = await AtlasIndexDefinition.CreateIndexAsync(options,
            Database.DatabaseNamespace.DatabaseName, CollectionName(), ct);
    }

    protected override Dictionary<string, string> BuildTexts(Dictionary<string, string> source)
    {
        var texts = new Dictionary<string, string>();

        foreach (var (key, value) in source)
        {
            var text = value;

            var languageCode = AtlasIndexDefinition.GetFieldName(key);

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
        Guard.NotNull(app);
        Guard.NotNull(query);

        var (search, take) = query;

        if (string.IsNullOrWhiteSpace(search))
        {
            return null;
        }

        var luceneQuery = QueryParser.Parse(search);

        var serveField = scope == SearchScope.All ? "fa" : "fp";

        var compound = new BsonDocument
        {
            ["must"] = new BsonArray
            {
                QueryVisitor.Visit(luceneQuery)
            },
            ["filter"] = new BsonArray
            {
                new BsonDocument
                {
                    ["text"] = new BsonDocument
                    {
                        ["path"] = "_ai",
                        ["query"] = app.Id.ToString()
                    }
                },
                new BsonDocument
                {
                    ["equals"] = new BsonDocument
                    {
                        ["path"] = serveField,
                        ["value"] = true
                    }
                }
            }
        };

        if (query.PreferredSchemaId != null)
        {
            compound["should"] = new BsonArray
            {
                new BsonDocument
                {
                    ["text"] = new BsonDocument
                    {
                        ["path"] = "_si",
                        ["query"] = query.PreferredSchemaId.Value.ToString()
                    }
                }
            };
        }
        else if (query.RequiredSchemaIds?.Count > 0)
        {
            compound["should"] = new BsonArray(query.RequiredSchemaIds.Select(x =>
                new BsonDocument
                {
                    ["text"] = new BsonDocument
                    {
                        ["path"] = "_si",
                        ["query"] = x.ToString()
                    }
                }));

            compound["minimumShouldMatch"] = 1;
        }

        var searchQuery = new BsonDocument
        {
            ["compound"] = compound
        };

        if (index != null)
        {
            searchQuery["index"] = index;
        }

        var results =
            await Collection.Aggregate().Search(searchQuery).Limit(take)
                .Project<MongoTextResult>(
                    Projection.Include(x => x.ContentId)
                )
                .ToListAsync(ct);

        return results.Select(x => x.ContentId).ToList();
    }
}
