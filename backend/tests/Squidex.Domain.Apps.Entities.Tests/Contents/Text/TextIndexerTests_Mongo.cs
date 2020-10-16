// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Linq;
using System.Threading.Tasks;
using MongoDB.Driver;
using Squidex.Domain.Apps.Entities.MongoDb.FullText;
using Squidex.Infrastructure;
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
                var database = mongoClient.GetDatabase("FullText");

                var index = new MongoTextIndex(database, false);

                await index.InitializeAsync();

                return index;
            }
        }

        public override IIndexerFactory Factory { get; } = new MongoFactory();

        public TextIndexerTests_Mongo()
        {
            SupportssQuerySyntax = false;
        }

        [Fact]
        public async Task Should_index_localized_content_without_stop_words_and_retrieve()
        {
            var both = ids2.Union(ids1).ToList();

            await TestCombinations(
                Create(ids1[0], "de", "and und"),
                Create(ids2[0], "en", "and und"),

                Search(expected: both, text: "and"),
                Search(expected: both, text: "und")
            );
        }
    }
}