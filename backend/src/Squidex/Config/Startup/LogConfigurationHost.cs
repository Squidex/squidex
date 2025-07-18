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
    private const int MaxValueLength = 30;
    private static readonly string RedactedValue = "*****";
    private static readonly string[] SensitiveValues =
    [
        "aws",
        "azure",
        "bearer",
        "clientid",
        "credential",
        "database",
        "db",
        "github",
        "google",
        "jwt",
        "key",
        "microsoft",
        "pass",
        "secret",
        "server",
        "tenant",
        "token",
        "username",
    ];

    public Task StartAsync(
        CancellationToken cancellationToken)
    {
        log.LogInformation(w => w
            .WriteProperty("message", "Application started")
            .WriteObject("environment", c =>
            {
                var logged = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

                var orderedConfigs =
                    configuration.AsEnumerable()
                        .OrderBy(x => x.Key, StringComparer.OrdinalIgnoreCase);

                foreach (var (key, value) in orderedConfigs)
                {
                    if (string.IsNullOrWhiteSpace(value))
                    {
                        continue;
                    }

                    var lowerKey = key.ToLowerInvariant();
                    if (!logged.Add(lowerKey))
                    {
                        continue;
                    }

                    var formattedValue = value;
                    if (IsSensitiveKey(lowerKey) || IsSensitiveKey(value) || IsSensitiveValue(value))
                    {
                        formattedValue = RedactedValue;
                    }
                    else if (formattedValue.Length > MaxValueLength)
                    {
                        formattedValue = formattedValue[.. (MaxValueLength - 3)] + "...";
                    }

                    c.WriteProperty(lowerKey, formattedValue);
                }
            }));

        return Task.CompletedTask;
    }

    private static bool IsSensitiveKey(string key)
    {
        return SensitiveValues.Any(pattern => key.Contains(pattern, StringComparison.OrdinalIgnoreCase));
    }

    private static bool IsSensitiveValue(string? value)
    {
        // Check for connection strings and URLs with credentials
        if (string.IsNullOrEmpty(value) || !value.Contains("://", StringComparison.Ordinal))
        {
            return false;
        }

        // Contains username/password, connection string parameters or query strings.
        return value.Contains('@', StringComparison.Ordinal)
            || value.Contains(';', StringComparison.Ordinal)
            || value.Contains('=', StringComparison.Ordinal);
    }

    public Task StopAsync(
        CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}
