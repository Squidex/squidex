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

                    foreach (var kvp in config.AsEnumerable().Where(kvp => kvp.Value != null).Select(x => new { Key = x.Key.ToLowerInvariant(), x.Value }).OrderBy(x => x.Key))
                    {
                        if (logged.Add(kvp.Key))
                        {
                            c.WriteProperty(kvp.Key, kvp.Value);
                        }
                    }
                }));
        }
    }
}
