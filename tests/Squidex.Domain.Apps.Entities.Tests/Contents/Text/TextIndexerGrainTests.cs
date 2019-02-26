// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Squidex.Domain.Apps.Core;
using Squidex.Domain.Apps.Core.Contents;
using Squidex.Domain.Apps.Core.Schemas;
using Squidex.Infrastructure.Assets;
using Xunit;

namespace Squidex.Domain.Apps.Entities.Contents.Text
{
    public class TextIndexerGrainTests : IDisposable
    {
        private readonly Schema schema =
            new Schema("test")
                .AddString(1, "test", Partitioning.Invariant)
                .AddString(2, "localized", Partitioning.Language);
        private readonly Guid schemaId = Guid.NewGuid();
        private readonly List<Guid> ids1 = new List<Guid> { Guid.NewGuid() };
        private readonly List<Guid> ids2 = new List<Guid> { Guid.NewGuid() };
        private readonly SearchContext context;
        private readonly IAssetStore assetStore = new MemoryAssetStore();
        private readonly TextIndexerGrain sut;

        public TextIndexerGrainTests()
        {
            context = new SearchContext
            {
                AppVersion = 1,
                Schema = schema,
                SchemaVersion = 1,
                Languages = new List<string> { "de", "en" }
            };

            sut = new TextIndexerGrain(assetStore);
            sut.ActivateAsync(schemaId).Wait();
        }

        public void Dispose()
        {
            sut.OnDeactivateAsync().Wait();
        }

        [Fact]
        public async Task Should_read_index_and_retrieve()
        {
            await AddInvariantContent();

            await sut.DeactivateAsync();

            var other = new TextIndexerGrain(assetStore);
            try
            {
                await other.ActivateAsync(schemaId);

                var helloIds = await other.SearchAsync("Hello", context);

                Assert.Equal(ids1, helloIds);

                var worldIds = await other.SearchAsync("World", context);

                Assert.Equal(ids2, worldIds);
            }
            finally
            {
                await other.OnDeactivateAsync();
            }
        }

        [Fact]
        public async Task Should_index_invariant_content_and_retrieve()
        {
            await AddInvariantContent();

            var helloIds = await sut.SearchAsync("Hello", context);

            Assert.Equal(ids1, helloIds);

            var worldIds = await sut.SearchAsync("World", context);

            Assert.Equal(ids2, worldIds);
        }

        [Fact]
        public async Task Should_delete_documents_from_index()
        {
            await AddInvariantContent();

            await sut.DeleteAsync(ids1[0]);
            await sut.FlushAsync();

            var helloIds = await sut.SearchAsync("Hello", context);

            Assert.Empty(helloIds);

            var worldIds = await sut.SearchAsync("World", context);

            Assert.Equal(ids2, worldIds);
        }

        [Fact]
        public async Task Should_index_localized_content_and_retrieve()
        {
            await AddLocalizedContent();

            var german1 = await sut.SearchAsync("Stadt", context);
            var german2 = await sut.SearchAsync("and", context);

            var germanStopwordsIds = await sut.SearchAsync("und", context);

            Assert.Equal(ids1, german1);
            Assert.Equal(ids1, german2);

            Assert.Equal(ids2, germanStopwordsIds);

            var english1 = await sut.SearchAsync("City", context);
            var english2 = await sut.SearchAsync("und", context);

            var englishStopwordsIds = await sut.SearchAsync("and", context);

            Assert.Equal(ids2, english1);
            Assert.Equal(ids2, english2);

            Assert.Equal(ids1, englishStopwordsIds);
        }

        private async Task AddLocalizedContent()
        {
            var germanData =
                new NamedContentData()
                    .AddField("localized",
                        new ContentFieldData()
                            .AddValue("de", "Stadt und Umgebung and whatever"));

            var englishData =
                new NamedContentData()
                    .AddField("localized",
                        new ContentFieldData()
                            .AddValue("en", "City and Surroundings und sonstiges"));

            await sut.IndexAsync(ids1[0], new IndexData { Data = germanData });
            await sut.IndexAsync(ids2[0], new IndexData { Data = englishData });
            await sut.FlushAsync();
        }

        private async Task AddInvariantContent()
        {
            var data1 =
                new NamedContentData()
                    .AddField("test",
                        new ContentFieldData()
                            .AddValue("iv", "Hello"));

            var data2 =
                new NamedContentData()
                    .AddField("test",
                        new ContentFieldData()
                            .AddValue("iv", "World"));

            await sut.IndexAsync(ids1[0], new IndexData { Data = data1 });
            await sut.IndexAsync(ids2[0], new IndexData { Data = data2 });

            await sut.FlushAsync();
        }
    }
}
