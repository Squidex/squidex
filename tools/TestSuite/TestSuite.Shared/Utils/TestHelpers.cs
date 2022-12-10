// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Microsoft.Extensions.Configuration;

namespace TestSuite.Utils;

public static class TestHelpers
{
    public static IConfiguration Configuration { get; }

    static TestHelpers()
    {
        var basePath = Path.GetFullPath("../../../");

        Configuration = new ConfigurationBuilder()
            .SetBasePath(basePath)
            .AddJsonFile("appsettings.json", true)
            .AddJsonFile("appsettings.Development.json", true)
            .AddEnvironmentVariables()
            .Build();
    }

    public static string GetAndPrintValue(string name, string fallback)
    {
        var value = Configuration[name];

        if (string.IsNullOrWhiteSpace(value))
        {
            value = fallback;
        }
        else
        {
            Console.WriteLine("Using <{0}>=<{1}>", name, value);
        }

        return value;
    }
}
