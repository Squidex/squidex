// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

namespace Squidex.Config.Domain;

public static class ConfigurationExtensions
{
    public static void ConfigureForSquidex(this IConfigurationBuilder builder, IHostEnvironment environment)
    {
        builder.AddJsonFile("appsettings.Custom.json", true);
        builder.AddKeyPerFile(Path.Combine(environment.ContentRootPath, "Configuration"), true, true);
    }
}
