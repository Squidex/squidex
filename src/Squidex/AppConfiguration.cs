// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
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
