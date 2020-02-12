// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FakeItEasy;
using Orleans.Concurrency;
using Squidex.Domain.Apps.Entities.Contents.Text.Lucene;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Log;
using Xunit;

#pragma warning disable xUnit1004 // Test methods should not be skipped

namespace Squidex.Domain.Apps.Entities.Contents.Text
{
    [Trait("Category", "Dependencies")]
    public class TextIndexerBenchmark
    {
        private const int Size = 200;
        private readonly NamedId<Guid> appId = NamedId.Of(Guid.NewGuid(), "my-app");
        private readonly NamedId<Guid> schemaId = NamedId.Of(Guid.NewGuid(), "my-schema");

        [Fact]
        public async Task Should_index_and_search_in_temp_folder()
        {
            await IndexAndSearchAsync(TestStorages.TempFolder());
        }

        [Fact]
        public async Task Should_index_and_search_in_assets()
        {
            await IndexAndSearchAsync(TestStorages.Assets());
        }

        [Fact]
        public async Task Should_index_and_search_in_mongoDB()
        {
            await IndexAndSearchAsync(TestStorages.MongoDB());
        }

        private async Task IndexAndSearchAsync(IIndexStorage storage)
        {
            var factory = new IndexManager(storage, A.Fake<ISemanticLog>());

            var grain = new LuceneTextIndexGrain(factory);

            await grain.ActivateAsync(appId.Id);

            var elapsed1 = await IndexAsync(grain);
            var elapsed2 = await SearchAsync(grain);
            var elapsed3 = await SearchAsync(grain);

            Assert.Equal(new long[0], new[] { elapsed1, elapsed2, elapsed3 });
        }

        private async Task<long> IndexAsync(LuceneTextIndexGrain grain)
        {
            var text = new Dictionary<string, string>
            {
                ["iv"] = "Hello World"
            };

            var ids = new Guid[Size];

            for (var i = 0; i < ids.Length; i++)
            {
                ids[i] = Guid.NewGuid();
            }

            var watch = ValueStopwatch.StartNew();

            foreach (var id in ids)
            {
                var commands = new IndexCommand[]
                {
                    new UpsertIndexEntry
                    {
                         ContentId = id,
                         DocId = id.ToString(),
                         ServeAll = true,
                         ServePublished = true,
                         Texts = text
                    }
                };

                await grain.IndexAsync(schemaId, commands.AsImmutable());
            }

            return watch.Stop();
        }

        private async Task<long> SearchAsync(LuceneTextIndexGrain grain)
        {
            var searchContext = new SearchContext
            {
                Languages = new HashSet<string>()
            };

            var watch = ValueStopwatch.StartNew();

            for (var i = 0; i < Size; i++)
            {
                var result = await grain.SearchAsync("Hello", default, searchContext);

                Assert.NotEmpty(result);
            }

            return watch.Stop();
        }
    }
}
