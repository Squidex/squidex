// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Microsoft.Extensions.DependencyInjection;
using Squidex.ClientLibrary;

namespace TestSuite.Fixtures;

public sealed class ClientCloudFixture
{
    public ISquidexClientManager ClientManager { get; private set; }

    public ISquidexClientManager CDNClientManager { get; private set; }

    public ClientCloudFixture()
    {
        ClientManager =
            new ServiceCollection()
                .AddSquidexClient(options =>
                {
                    options.AppName = "squidex-website";
                    options.ClientId = "squidex-website:reader";
                    options.ClientSecret = "yy9x4dcxsnp1s34r2z19t88wedbzxn1tfq7uzmoxf60x";
                    options.ReadResponseAsString = true;
                })
                .BuildServiceProvider()
                .GetRequiredService<ISquidexClientManager>();

        CDNClientManager =
            new ServiceCollection()
                .AddSquidexClient(options =>
                {
                    options.AppName = "squidex-website";
                    options.AssetCDN = "https://assets.squidex.io";
                    options.ClientId = "squidex-website:reader";
                    options.ClientSecret = "yy9x4dcxsnp1s34r2z19t88wedbzxn1tfq7uzmoxf60x";
                    options.ContentCDN = "https://contents.squidex.io";
                })
                .BuildServiceProvider()
                .GetRequiredService<ISquidexClientManager>();
    }
}
