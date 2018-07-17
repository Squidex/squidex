// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using Xunit;

#pragma warning disable xUnit1000 // Test classes must be public

namespace Squidex.Infrastructure.Assets
{
    internal class AzureBlobAssetStoreTests : AssetStoreTests<AzureBlobAssetStore>
    {
        public override AzureBlobAssetStore CreateStore()
        {
            return new AzureBlobAssetStore("UseDevelopmentStorage=true", "squidex-test-container");
        }

        public override void Dispose()
        {
        }

        [Fact]
        public void Should_calculate_source_url()
        {
            Sut.Initialize();

            var id = Guid.NewGuid().ToString();

            Assert.Equal($"http://127.0.0.1:10000/squidex-test-container/{id}_1", Sut.GenerateSourceUrl(id, 1, null));
        }
    }
}
