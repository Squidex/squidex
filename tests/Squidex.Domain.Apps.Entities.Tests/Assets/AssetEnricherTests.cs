// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Threading.Tasks;
using FakeItEasy;
using Squidex.Domain.Apps.Core.Tags;
using Xunit;

namespace Squidex.Domain.Apps.Entities.Assets
{
    public class AssetEnricherTests
    {
        private readonly ITagService tagService = A.Fake<ITagService>();
        private readonly AssetEnricher sut;

        public AssetEnricherTests()
        {
            sut = new AssetEnricher(tagService);
        }

        [Fact]
        public async Task Should_not_enrich_if_asset_contains_null_tags()
        {
            var source = new AssetEntity();

            var result = await sut.EnrichAsync(source);

            Assert.Empty(result.TagNames);
        }
    }
}
