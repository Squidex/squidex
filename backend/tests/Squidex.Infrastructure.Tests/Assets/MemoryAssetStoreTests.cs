// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Xunit;

namespace Squidex.Infrastructure.Assets
{
    public class MemoryAssetStoreTests : AssetStoreTests<MemoryAssetStore>
    {
        public override MemoryAssetStore CreateStore()
        {
            return new MemoryAssetStore();
        }

        [Fact]
        public void Should_not_calculate_source_url()
        {
            var url = Sut.GeneratePublicUrl(FileName);

            Assert.Null(url);
        }
    }
}