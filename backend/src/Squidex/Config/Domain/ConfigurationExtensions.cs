// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Microsoft.Extensions.Configuration;

namespace Squidex.Config.Domain
{
    public static class ConfigurationExtensions
    {
        public static void ConfigureForSquidex(this IConfigurationBuilder builder)
        {
            builder.AddJsonFile("appsettings.Custom.json", true);
        }
    }
}
