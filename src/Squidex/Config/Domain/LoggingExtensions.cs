// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Linq;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Squidex.Infrastructure.Log;

namespace Squidex.Config.Domain
{
    public static class LoggingExtensions
    {
        public static void LogConfiguration(this IServiceProvider services)
        {
            var log = services.GetRequiredService<ISemanticLog>();

            var config = services.GetRequiredService<IConfiguration>();

            log.LogInformation(w => w
                .WriteProperty("message", "Application started")
                .WriteObject("environment", c =>
                {
                    foreach (var kvp in config.AsEnumerable().Where(kvp => kvp.Value != null))
                    {
                        c.WriteProperty(kvp.Key, kvp.Value);
                    }
                }));
        }
    }
}
