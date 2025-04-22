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
    private static readonly Regex[] SensitivePatterns =
    [
        // Authentication and API keys
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
    ];

    private static readonly string RedactedValue = "*****";

    public Task StartAsync(
        CancellationToken cancellationToken)
    {
        log.LogInformation(w => w
            .WriteProperty("message", "Application started")
            .WriteObject("environment", c =>
            {
                var logged = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

                var orderedConfigs = configuration.AsEnumerable()
                    .Where(kvp => kvp.Value != null)
                    .OrderBy(x => x.Key, StringComparer.OrdinalIgnoreCase);

                foreach (var (key, val) in orderedConfigs)
                {
                    if (logged.Add(key))
                    {
                        var keyLower = key.ToLowerInvariant();
                        var value = IsSensitiveKey(keyLower) || IsSensitiveValue(val) ? RedactedValue : val;

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
        if (string.IsNullOrEmpty(value))
        {
            return false;
        }

        // Check for connection strings and URLs with credentials
        return value.Contains("://") && (
            value.Contains("@") || // Contains username/password in URL
            value.Contains(";") || // Contains connection string parameters
            value.Contains("=") // Contains key-value pairs
        );
    }

    public Task StopAsync(
        CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}
