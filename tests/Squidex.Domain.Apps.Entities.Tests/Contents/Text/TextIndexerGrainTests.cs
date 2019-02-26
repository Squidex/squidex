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
        private readonly List<string> languages = new List<string> { "de", "en" };
        private readonly Guid schemaId = Guid.NewGuid();
        private readonly List<Guid> ids1 = new List<Guid> { Guid.NewGuid() };
        private readonly List<Guid> ids2 = new List<Guid> { Guid.NewGuid() };
        private readonly IAssetStore assetStore = new MemoryAssetStore();
        private readonly TextIndexerGrain sut;

        public TextIndexerGrainTests()
        {
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

                var helloIds = await other.SearchAsync("Hello", 0, 0, schema, languages);

                Assert.Equal(ids1, helloIds);

                var worldIds = await other.SearchAsync("World", 0, 0, schema, languages);

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

            var helloIds = await sut.SearchAsync("Hello", 0, 0, schema, languages);

            Assert.Equal(ids1, helloIds);

            var worldIds = await sut.SearchAsync("World", 0, 0, schema, languages);

            Assert.Equal(ids2, worldIds);
        }

        [Fact]
        public async Task Should_delete_documents_from_index()
        {
            await AddInvariantContent();

            await sut.DeleteContentAsync(ids1[0]);
            await sut.FlushAsync();

            var helloIds = await sut.SearchAsync("Hello", 0, 0, schema, languages);

            Assert.Empty(helloIds);

            var worldIds = await sut.SearchAsync("World", 0, 0, schema, languages);

            Assert.Equal(ids2, worldIds);
        }

        [Fact]
        public async Task Should_index_localized_content_and_retrieve()
        {
            await AddLocalizedContent();

            var german1 = await sut.SearchAsync("Stadt", 0, 0, schema, languages);
            var german2 = await sut.SearchAsync("and", 0, 0, schema, languages);

            var germanStopwordsIds = await sut.SearchAsync("und", 0, 0, schema, languages);

            Assert.Equal(ids1, german1);
            Assert.Equal(ids1, german2);

            Assert.Equal(ids2, germanStopwordsIds);

            var english1 = await sut.SearchAsync("City", 0, 0, schema, languages);
            var english2 = await sut.SearchAsync("und", 0, 0, schema, languages);

            var englishStopwordsIds = await sut.SearchAsync("and", 0, 0, schema, languages);

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

            await sut.AddContentAsync(ids1[0], germanData, false, false);
            await sut.AddContentAsync(ids2[0], englishData, false, false);
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

            await sut.AddContentAsync(ids1[0], data1, false, false);
            await sut.AddContentAsync(ids2[0], data2, false, false);

            await sut.FlushAsync();
        }
    }
}
