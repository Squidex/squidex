// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.IO;
using FakeItEasy;
using Squidex.Infrastructure.Log;

namespace Squidex.Infrastructure.Assets
{
    public sealed class FolderAssetStoreFixture : IDisposable
    {
        public string TestFolder { get; } = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());

        public FolderAssetStore AssetStore { get; }

        public FolderAssetStoreFixture()
        {
            AssetStore = new FolderAssetStore(TestFolder, A.Dummy<ISemanticLog>());
            AssetStore.InitializeAsync().Wait();
        }

        public void Dispose()
        {
            if (Directory.Exists(TestFolder))
            {
                Directory.Delete(TestFolder, true);
            }
        }
    }
}
