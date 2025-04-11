// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Security.Claims;
using NodaTime;
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

    private struct TextPart
    {
        public bool IsText;

        public int TextLength;

        public int TextOffset;

        public string VarFallback;

        public string VarTransform;

        public ValueTask<string?> Var;

        public static TextPart Text(int offset, int length)
        {
            var result = default(TextPart);

            result.TextOffset = offset;
            result.TextLength = length;
            result.IsText = true;

            return result;
        }

        public static TextPart Variable(ValueTask<string?> replacement, string fallback, string transform)
        {
            var result = default(TextPart);

            result.Var = replacement;
            result.VarFallback = fallback;
            result.VarTransform = transform;

            return result;
        }
    }

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

        return scriptEngine.Evaluate(new ScriptVars { ["@event"] = value }, expression);
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
                    // Provide this overload with object to serialize the derived type and not the static type.
                    return serializer.Serialize(value, true);
                case ExpressionFallback.Event:
                    // Provide this overload with object to serialize the derived type and not the static type.
                    return serializer.Serialize(@event, true);
                case ExpressionFallback.Envelope:
                    return ToEnvelope(@event.Name, @event, @event.Timestamp);
                default:
                    return expression;
            }
        }

        if (TryGetTemplate(expression.Trim(), out var template))
        {
            var vars = new TemplateVars
            {
                ["event"] = @event,
            };

            return await templateEngine.RenderAsync(template, vars);
        }

        if (TryGetScript(expression.Trim(), out var script))
        {
            // Script vars are just wrappers over dictionaries for better performance.
            var vars = new EventScriptVars
            {
                Event = @event,
                AppId = @event.AppId.Id,
                AppName = @event.AppId.Name,
                User = Admin(),
            };

            var result = (await scriptEngine.ExecuteAsync(vars, script)).ToString();

            if (result == "undefined")
            {
                return GlobalFallback;
            }

            return result;
        }

        return await simpleFormatter.Format(expression, @event, GlobalFallback);
    }

    private string ToEnvelope(string type, object payload, Instant timestamp)
    {
        // Provide this overload with object to serialize the derived type and not the static type.
        return serializer.Serialize(new { type, payload, timestamp }, true);
    }

    private static ClaimsPrincipal Admin()
    {
        var claimsIdentity = new ClaimsIdentity();
        var claimsPrincipal = new ClaimsPrincipal(claimsIdentity);

        claimsIdentity.AddClaim(new Claim(SquidexClaimTypes.Permissions, PermissionIds.All));

        return claimsPrincipal;
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
