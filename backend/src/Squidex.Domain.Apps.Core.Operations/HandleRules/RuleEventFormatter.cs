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
using Squidex.Flows.Internal.Execution;
using Squidex.Infrastructure.Json;
using Squidex.Shared;
using Squidex.Shared.Identity;

namespace Squidex.Domain.Apps.Core.HandleRules;

public sealed class RuleEventFormatter(
    IJsonSerializer serializer,
    IScriptEngine scriptEngine,
    ITemplateEngine templateEngine,
    SimpleFormatter simpleFormatter)
    : IFlowExpressionEngine
{
    private const string GlobalFallback = "null";

    public string SerializeJson<T>(T value)
    {
        return serializer.Serialize(value, true);
    }

    public T DeserializeJson<T>(string json)
    {
        return serializer.Deserialize<T>(json);
    }

    public bool Evaluate<T>(string? expression, T value)
    {
        if (string.IsNullOrWhiteSpace(expression))
        {
            return false;
        }

        if (value is not EnrichedEvent @event)
        {
            if (value is not FlowEventContext context)
            {
                return false;
            }

            @event = context.Event;
        }

        var vars = CreateScriptVars(value, @event);

        return scriptEngine.Evaluate(vars, expression);
    }

    public async ValueTask<string?> RenderAsync<T>(string? expression, T value, ExpressionFallback fallback = default)
    {
        if (value is not EnrichedEvent @event)
        {
            if (value is not FlowEventContext context)
            {
                return null;
            }

            @event = context.Event;
        }

        if (string.IsNullOrWhiteSpace(expression))
        {
            switch (fallback)
            {
                case ExpressionFallback.Context:
                    return serializer.Serialize(value, true);
                case ExpressionFallback.Event:
                    return serializer.Serialize(@event, true);
                case ExpressionFallback.Envelope:
                    return serializer.Serialize(new { type = @event.Name, payload = @event, timestamp = @event.Timestamp }, true);
                default:
                    return expression;
            }
        }

        if (TryGetTemplate(expression.Trim(), out var template))
        {
            var input = CreateTemplateVars(value, @event);
            var result = await templateEngine.RenderAsync(template, input);

            return result;
        }

        if (TryGetScript(expression.Trim(), out var script))
        {
            var input = CreateScriptVars(value, @event);
            var result = (await scriptEngine.ExecuteAsync(input, script)).ToString();

            if (result == "undefined")
            {
                return GlobalFallback;
            }

            return result;
        }

        return await simpleFormatter.Format(expression, @event, GlobalFallback);
    }

    private static ClaimsPrincipal Admin()
    {
        var claimsIdentity = new ClaimsIdentity();
        var claimsPrincipal = new ClaimsPrincipal(claimsIdentity);

        claimsIdentity.AddClaim(new Claim(SquidexClaimTypes.Permissions, PermissionIds.All));

        return claimsPrincipal;
    }

    private static EventScriptVars CreateScriptVars<T>(T value, EnrichedEvent @event)
    {
        return new EventScriptVars
        {
            AppId = @event.AppId.Id,
            AppName = @event.AppId.Name,
            Event = @event,
            User = Admin(),
            UserData = (value as FlowEventContext)?.UserData ?? [],
        };
    }

    private static TemplateVars CreateTemplateVars<T>(T value, EnrichedEvent @event)
    {
        return new TemplateVars
        {
            ["appId"] = @event.AppId.Id,
            ["appName"] = @event.AppId.Name,
            ["event"] = @event,
            ["user"] = new { displayName = "FlowUser" },
            ["userData"] = (value as FlowEventContext)?.UserData ?? [],
        };
    }

    private static bool TryGetScript(string text, out string script)
    {
        const string ScriptSuffix = ")";
        const string ScriptPrefix = "Script(";

        script = null!;

        const StringComparison comparer = StringComparison.OrdinalIgnoreCase;

        if (text.StartsWith(ScriptPrefix, comparer) && text.EndsWith(ScriptSuffix, comparer))
        {
            script = text.Substring(ScriptPrefix.Length, text.Length - ScriptPrefix.Length - ScriptSuffix.Length);
            return true;
        }

        return false;
    }

    private static bool TryGetTemplate(string text, out string script)
    {
        const string TemplateSuffix = ")";
        const string TemplatePrefix = "Liquid(";

        script = null!;

        const StringComparison comparer = StringComparison.OrdinalIgnoreCase;

        if (text.StartsWith(TemplatePrefix, comparer) && text.EndsWith(TemplateSuffix, comparer))
        {
            script = text.Substring(TemplatePrefix.Length, text.Length - TemplatePrefix.Length - TemplateSuffix.Length);
            return true;
        }

        return false;
    }
}
