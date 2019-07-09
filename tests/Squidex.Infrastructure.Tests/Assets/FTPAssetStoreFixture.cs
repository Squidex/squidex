// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using FakeItEasy;
using FluentFTP;
using Squidex.Infrastructure.Log;

namespace Squidex.Infrastructure.Assets
{
    public sealed class FTPAssetStoreFixture : IDisposable
    {
        public FTPAssetStore AssetStore { get; }

        public FTPAssetStoreFixture()
        {
            AssetStore = new FTPAssetStore(() => new FtpClient("localhost", 21, "test", "test"), "assets", A.Fake<ISemanticLog>());
            AssetStore.InitializeAsync().Wait();
        }

        public void Dispose()
        {
        }
    }
}
