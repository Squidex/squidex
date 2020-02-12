// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.ClientLibrary;

namespace TestSuite.Fixtures
{
    public sealed class CloudFixture
    {
        public SquidexClientManager ClientManager { get; private set; }

        public SquidexClientManager CDNClientManager { get; private set; }

        public CloudFixture()
        {
            ClientManager = new SquidexClientManager(
                new SquidexOptions
                {
                    AppName = "squidex-website",
                    ClientId = "squidex-website:reader",
                    ClientSecret = "yy9x4dcxsnp1s34r2z19t88wedbzxn1tfq7uzmoxf60x"
                });

            CDNClientManager = new SquidexClientManager(
                new SquidexOptions
                {
                    AppName = "squidex-website",
                    AssetCDN = "https://assets.squidex.io",
                    ClientId = "squidex-website:reader",
                    ClientSecret = "yy9x4dcxsnp1s34r2z19t88wedbzxn1tfq7uzmoxf60x",
                    ContentCDN = "https://contents.squidex.io",
                });
        }
    }
}
