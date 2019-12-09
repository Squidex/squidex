// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Infrastructure.Assets;

namespace Squidex.Domain.Apps.Entities.Contents.Text
{
    public class TextIndexerGrainTests_Assets : TextIndexerGrainTestsBase
    {
        public override IIndexStorage Storage { get; } = CreateStorage();

        private static IIndexStorage CreateStorage()
        {
            var storage = new AssetIndexStorage(new MemoryAssetStore());

            return storage;
        }
    }
}
