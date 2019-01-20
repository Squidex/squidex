// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Collections.Generic;
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

            log.LogInformation(w => w
                .WriteProperty("message", "Application started")
                .WriteObject("environment", c =>
                {
                    var config = services.GetRequiredService<IConfiguration>();

                    var logged = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

                    var orderedConfigs = config.AsEnumerable().Where(kvp => kvp.Value != null).OrderBy(x => x.Key, StringComparer.OrdinalIgnoreCase);

                    foreach (var (key, val) in orderedConfigs)
                    {
                        if (logged.Add(key))
                        {
                            c.WriteProperty(key.ToLowerInvariant(), val);
                        }
                    }
                }));
        }
    }
}
