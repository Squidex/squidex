// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Threading;
using FakeItEasy;
using FluentFTP;
using Squidex.Infrastructure.Log;

namespace Squidex.Infrastructure.Assets
{
    public sealed class FTPAssetStoreFixture
    {
        public FTPAssetStore AssetStore { get; }
        public IFtpClient FtpClient { get; private set; }
        public ISemanticLog Log { get; private set; }
        public FTPAssetStoreFixture()
        {
            FtpClient = A.Fake<IFtpClient>();
            Log = A.Fake<ISemanticLog>();
            AssetStore = new FTPAssetStore(() => FtpClient, "/", Log);
        }
    }
}
