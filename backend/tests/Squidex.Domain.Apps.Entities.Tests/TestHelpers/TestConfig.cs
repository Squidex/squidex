// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Microsoft.Extensions.Configuration;

namespace Squidex.Domain.Apps.Entities.TestHelpers;

public static class TestConfig
{
    public static IConfiguration Configuration { get; }

    static TestConfig()
    {
        var basePath = Path.GetFullPath("../../../");

        Configuration = new ConfigurationBuilder()
            .SetBasePath(basePath)
            .AddJsonFile("appsettings.json", true)
            .AddJsonFile("appsettings.Development.json", true)
            .AddEnvironmentVariables()
            .Build();
    }
}
