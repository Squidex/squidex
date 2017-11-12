// ==========================================================================
//  AppConfiguration.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using Microsoft.Extensions.Configuration;

namespace Squidex
{
    public static class AppConfiguration
    {
        public static void AddAppConfiguration(this IConfigurationBuilder builder, string environmentName, string[] args)
        {
            builder.Sources.Clear();
            builder.AddJsonFile("appsettings.json", true, true);
            builder.AddJsonFile($"appsettings.{environmentName}.json", true);
            builder.AddEnvironmentVariables();
            builder.AddCommandLine(args);
        }
    }
}
