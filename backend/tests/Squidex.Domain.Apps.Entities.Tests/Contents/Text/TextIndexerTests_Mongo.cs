// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Linq;
using System.Threading.Tasks;
using MongoDB.Driver;
using Newtonsoft.Json;
using Squidex.Domain.Apps.Core.TestHelpers;
using Squidex.Domain.Apps.Entities.MongoDb.FullText;
using Squidex.Infrastructure.MongoDb;
using Xunit;

namespace Squidex.Domain.Apps.Entities.Contents.Text
{
    [Trait("Category", "Dependencies")]
    public class TextIndexerTests_Mongo : TextIndexerTestsBase
    {
        public override bool SupportsQuerySyntax => false;

        public override bool SupportsGeo => true;

        static TextIndexerTests_Mongo()
        {
            BsonJsonConvention.Register(JsonSerializer.Create(TestUtils.CreateSerializerSettings()));

            DomainIdSerializer.Register();
        }

        public override ITextIndex CreateIndex()
        {
            var mongoClient = new MongoClient("mongodb://localhost");
            var mongoDatabase = mongoClient.GetDatabase("Squidex_Testing");

            var index = new MongoTextIndex(mongoDatabase, false);

            index.InitializeAsync(default).Wait();

            return index;
        }

        [Fact]
        public async Task Should_retrieve_all_stopwords_for_english_query()
        {
            var both = ids2.Union(ids1).ToList();

            await CreateTextAsync(ids1[0], "de", "and und");
            await CreateTextAsync(ids2[0], "en", "and und");

            await SearchText(expected: both, text: "and");
        }

        [Fact]
        public async Task Should_retrieve_all_stopwords_for_german_query()
        {
            var both = ids2.Union(ids1).ToList();

            await CreateTextAsync(ids1[0], "de", "and und");
            await CreateTextAsync(ids2[0], "en", "and und");

            await SearchText(expected: both, text: "und");
        }
    }
}
