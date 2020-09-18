// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Threading.Tasks;
using Squidex.Domain.Apps.Entities.Contents.Text.Elastic;
using Squidex.Infrastructure;
using Xunit;

namespace Squidex.Domain.Apps.Entities.Contents.Text
{
    [Trait("Category", "Dependencies")]
    public class TextIndexerTests_Elastic : TextIndexerTestsBase
    {
        private sealed class TheFactory : IIndexerFactory
        {
            public Task CleanupAsync()
            {
                return Task.CompletedTask;
            }

            public Task<ITextIndex> CreateAsync(DomainId schemaId)
            {
                var index = new ElasticSearchTextIndex("http://localhost:9200", "squidex", true);

                return Task.FromResult<ITextIndex>(index);
            }
        }

        public override IIndexerFactory Factory { get; } = new TheFactory();

        public TextIndexerTests_Elastic()
        {
            SupportsSearchSyntax = false;
            SupportsMultiLanguage = false;
        }
    }
}
