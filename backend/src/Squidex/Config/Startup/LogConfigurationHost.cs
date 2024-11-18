// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Log;

namespace Squidex.Config.Startup;

public sealed class LogConfigurationHost(IConfiguration configuration, ISemanticLog log) : IHostedService
{
    public Task StartAsync(
        CancellationToken cancellationToken)
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

    public Task StopAsync(
        CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}
