// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Text.RegularExpressions;
using Squidex.Log;

namespace Squidex.Config.Startup;

public sealed class LogConfigurationHost(IConfiguration configuration, ISemanticLog log) : IHostedService
{
    private static readonly string RedactedValue = "*****";
    private static readonly Regex[] SensitivePatterns =
    [
        // Authentication and API keys
#pragma warning disable MA0110 // Use the Regex source generator
        new Regex(@"(?i)(secret|token|key|password|credential|auth|api[_-]?key)$"),
        new Regex(@"(?i)^(aws|azure|google|microsoft|github)[_-]"),
        new Regex(@"(?i)(jwt|bearer|oauth|saml)"),
        new Regex(@"(?i)(client|secret|password)$"),

        // Connection strings and credentials
        new Regex(@"(?i)(connectionstring|connection)$"),
        new Regex(@"(?i)(username|password|credential)$"),

        // Cloud provider specific
        new Regex(@"(?i)(accesskey|secretkey|privatekey|publickey)$"),
        new Regex(@"(?i)(projectid|tenantid)$"),

        // Database specific
        new Regex(@"(?i)(mongodb|sqlserver|postgres|mysql)://.*"),
        new Regex(@"(?i)(database|db|server|host|port|user|pass)="),
#pragma warning restore MA0110 // Use the Regex source generator
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

                    var keyLower = key.ToLowerInvariant();

                    if (logged.Add(keyLower))
                    {
                        var formattedValue = IsSensitiveKey(keyLower) || IsSensitiveValue(value) ? RedactedValue : value;

                        c.WriteProperty(keyLower, value);
                    }
                }
            }));

        return Task.CompletedTask;
    }

    private static bool IsSensitiveKey(string key)
    {
        return SensitivePatterns.Any(pattern => pattern.IsMatch(key));
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
