// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Squidex.Log;

namespace Squidex.Config.Startup
{
    public sealed class LogConfigurationHost : SafeHostedService
    {
        private readonly IConfiguration configuration;

        public LogConfigurationHost(ISemanticLog log, IConfiguration configuration)
            : base(log)
        {
            this.configuration = configuration;
        }

        protected override Task StartAsync(ISemanticLog log, CancellationToken ct)
        {
            log.LogInformation(w => w
                .WriteProperty("message", "Application started")
                .WriteObject("environment", c =>
                {
                    var logged = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

                    var orderedConfigs = configuration.AsEnumerable().Where(kvp => kvp.Value != null).OrderBy(x => x.Key, StringComparer.OrdinalIgnoreCase);

                    foreach (var (key, val) in orderedConfigs)
                    {
                        if (logged.Add(key))
                        {
                            c.WriteProperty(key.ToLowerInvariant(), val);
                        }
                    }
                }));

            return Task.CompletedTask;
        }
    }
}
