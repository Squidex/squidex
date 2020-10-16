﻿// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Threading.Tasks;
using Squidex.Domain.Apps.Entities.Contents.Text.Elastic;
using Squidex.Infrastructure;
using Xunit;

#pragma warning disable SA1115 // Parameter should follow comma

namespace Squidex.Domain.Apps.Entities.Contents.Text
{
    [Trait("Category", "Dependencies")]
    public class TextIndexerTests_Elastic : TextIndexerTestsBase
    {
        private sealed class ElasticFactory : IIndexerFactory
        {
            public Task CleanupAsync()
            {
                return Task.CompletedTask;
            }

            public async Task<ITextIndex> CreateAsync(DomainId schemaId)
            {
                var index = new ElasticSearchTextIndex("http://localhost:9200", "squidex", true);

                await index.InitializeAsync();

                return index;
            }
        }

        public override IIndexerFactory Factory { get; } = new ElasticFactory();

        public TextIndexerTests_Elastic()
        {
            SupportssQuerySyntax = true;
        }

        [Fact]
        public async Task Should_index_localized_content_without_stop_words_and_retrieve()
        {
            await TestCombinations(
                Create(ids1[0], "de", "and und"),
                Create(ids2[0], "en", "and und"),

                Search(expected: ids1, text: "and"),
                Search(expected: ids2, text: "und")
            );
        }

        [Fact]
        public async Task Should_index_cjk_content_and_retrieve()
        {
            await TestCombinations(
                Create(ids1[0], "zh", "東京大学"),

                Search(expected: ids1, text: "東京")
            );
        }
    }
}
