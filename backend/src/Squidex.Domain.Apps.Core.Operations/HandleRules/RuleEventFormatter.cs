// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Security.Claims;
using Squidex.Domain.Apps.Core.Rules.EnrichedEvents;
using Squidex.Domain.Apps.Core.Scripting;
using Squidex.Domain.Apps.Core.Templates;
using Squidex.Flows;
using Squidex.Flows.Execution;
using Squidex.Infrastructure.Json;
using Squidex.Shared;
using Squidex.Shared.Identity;

namespace Squidex.Domain.Apps.Core.HandleRules;

public sealed class RuleEventFormatter(IJsonSerializer serializer, ITemplateEngine templateEngine, IScriptEngine scriptEngine, SimpleFormatter formatter) : IExpressionEngine
{
    private const string GlobalFallback = "null";

    public bool Evaluate<T>(string? expression, T value)
    {
        if (string.IsNullOrWhiteSpace(expression))
        {
            return false;
        }

        object? @event = value;
        if (value is RuleFlowContext context)
        {
            @event = context.Event;
        }

        var vars = new EventScriptVars
        {
            Event = @event
        };

        return scriptEngine.Evaluate(vars, expression);
    }

    public async ValueTask<string?> RenderAsync<T>(string? expression, T value, ExpressionFallback fallback)
    {
        if (value is not RuleFlowContext context)
        {
            return string.Empty;
        }

        var @event = context.Event;
        if (!string.IsNullOrWhiteSpace(expression))
        {
            return await FormatAsync(expression, @event);
        }

        if (fallback == ExpressionFallback.Envelope)
        {
            return Serialize(new { type = @event.Name, payload = @event, timestamp = @event.Timestamp });
        }

        if (fallback == ExpressionFallback.Event)
        {
            return Serialize(@event);
        }

        return string.Empty;
    }

    public string Serialize<T>(T value)
    {
        return serializer.Serialize(value, true);
    }

    public async ValueTask<string?> FormatAsync(string? text, EnrichedEvent @event)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            return text;
        }

        var result = await FormatCoreAsync(text.Trim(), @event);
        if (result == "undefined")
        {
            return GlobalFallback;
        }

        return result;
    }

    private async ValueTask<string?> FormatCoreAsync(string text, EnrichedEvent @event)
    {
        if (TryGetWrapped(text, "Liquid(", ")", out var template))
        {
            // Expose the event only.
            var vars = new TemplateVars
            {
                ["event"] = @event
            };

            return await templateEngine.RenderAsync(template, vars);
        }

        if (TryGetWrapped(text, "Script(", ")", out var script))
        {
            // Expand the event and additional variables for extensions.
            var vars = new EventScriptVars
            {
                Event = @event,
                AppId = @event.AppId.Id,
                AppName = @event.AppId.Name,
                User = Admin()
            };

            return (await scriptEngine.ExecuteAsync(vars, script)).ToString();
        }

        return await formatter.FormatAsync(text, @event, GlobalFallback);
    }

    private static ClaimsPrincipal Admin()
    {
        var claimsIdentity = new ClaimsIdentity();
        var claimsPrincipal = new ClaimsPrincipal(claimsIdentity);

        claimsIdentity.AddClaim(new Claim(SquidexClaimTypes.Permissions, PermissionIds.All));

        return claimsPrincipal;
    }

    private static bool TryGetWrapped(string text, string prefix, string suffix, out string script)
    {
        script = null!;

        if (text.StartsWith(prefix, StringComparison.OrdinalIgnoreCase) && text.EndsWith(suffix, StringComparison.OrdinalIgnoreCase))
        {
            script = text.Substring(prefix.Length, text.Length - prefix.Length - suffix.Length);
            return true;
        }

        return false;
    }
}
