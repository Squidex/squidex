// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.ClientLibrary;

namespace TestSuite.Fixtures
{
    public class AssetFixture : CreatedAppFixture
    {
        public SquidexAssetClient Assets { get; }

        public AssetFixture()
        {
            Assets = ClientManager.GetAssetClient();
        }
    }
}
