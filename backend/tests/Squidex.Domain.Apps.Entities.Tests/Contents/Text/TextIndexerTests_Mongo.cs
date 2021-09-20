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
using Squidex.Infrastructure;
using Squidex.Infrastructure.MongoDb;
using Xunit;

#pragma warning disable SA1115 // Parameter should follow comma

namespace Squidex.Domain.Apps.Entities.Contents.Text
{
    [Trait("Category", "Dependencies")]
    public class TextIndexerTests_Mongo : TextIndexerTestsBase
    {
        private sealed class MongoFactory : IIndexerFactory
        {
            private readonly MongoClient mongoClient = new MongoClient("mongodb://localhost");

            public Task CleanupAsync()
            {
                return Task.CompletedTask;
            }

            public async Task<ITextIndex> CreateAsync(DomainId schemaId)
            {
                var database = mongoClient.GetDatabase("Squidex_Testing");

                var index = new MongoTextIndex(database, false);

                await index.InitializeAsync(default);

                return index;
            }
        }

        public override IIndexerFactory Factory { get; } = new MongoFactory();

        public TextIndexerTests_Mongo()
        {
            BsonJsonConvention.Register(JsonSerializer.Create(TestUtils.CreateSerializerSettings()));

            DomainIdSerializer.Register();

#pragma warning disable MA0056 // Do not call overridable members in constructor
            SupportsQuerySyntax = false;
            SupportsGeo = true;
#pragma warning restore MA0056 // Do not call overridable members in constructor
        }

        [Fact]
        public async Task Should_index_localized_content_without_stop_words_and_retrieve()
        {
            var both = ids2.Union(ids1).ToList();

            await TestCombinations(
                CreateText(ids1[0], "de", "and und"),
                CreateText(ids2[0], "en", "and und"),

                SearchText(expected: both, text: "and"),
                SearchText(expected: both, text: "und")
            );
        }
    }
}
