// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Threading.Tasks;
using Squidex.Extensions.Text.Azure;
using Xunit;

namespace Squidex.Domain.Apps.Entities.Contents.Text
{
    [Trait("Category", "Dependencies")]
    public class TextIndexerTests_Azure : TextIndexerTestsBase
    {
        public override ITextIndex CreateIndex()
        {
            var index = new AzureTextIndex("https://squidex.search.windows.net", "API_KEY", "test", 2000);

            index.InitializeAsync(default).Wait();

            return index;
        }

        [Fact]
        public async Task Should_retrieve_english_stopword_only_for_german_query()
        {
            await CreateTextAsync(ids1[0], "de", "and und");
            await CreateTextAsync(ids2[0], "en", "and und");

            await SearchText(expected: ids2, text: "und");
        }

        [Fact]
        public async Task Should_retrieve_german_stopword_only_for_english_query()
        {
            await CreateTextAsync(ids1[0], "de", "and und");
            await CreateTextAsync(ids2[0], "en", "and und");

            await SearchText(expected: ids1, text: "and");
        }

        [Fact]
        public async Task Should_index_cjk_content_and_retrieve()
        {
            await CreateTextAsync(ids1[0], "zh", "東京大学");

            await SearchText(expected: ids1, text: "東京");
        }
    }
}
