﻿// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Domain.Apps.Core.Rules.EnrichedEvents;
using Squidex.Domain.Apps.Core.Scripting;
using Squidex.Flows;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Http;

namespace Squidex.Extensions.Actions;

public static class RuleHelper
{
    public static bool ShouldDelete(this EnrichedEvent @event, IScriptEngine scriptEngine, string? expression)
    {
        if (!string.IsNullOrWhiteSpace(expression))
        {
            // Script vars are just wrappers over dictionaries for better performance.
            var vars = new EventScriptVars
            {
                Event = @event,
            };

            return scriptEngine.Evaluate(vars, expression);
        }

        return IsContentDeletion(@event) || IsAssetDeletion(@event);
    }

    public static bool ShouldDelete(this EnrichedEvent @event, FlowExecutionContext context, string? expression)
    {
        if (!string.IsNullOrWhiteSpace(expression))
        {
            return context.Evaluate(expression, context.Context);
        }

        return IsContentDeletion(@event) || IsAssetDeletion(@event);
    }

    public static bool IsContentDeletion(this EnrichedEvent @event)
    {
        return @event is EnrichedContentEvent { Type: EnrichedContentEventType.Deleted or EnrichedContentEventType.Unpublished };
    }

    public static bool IsAssetDeletion(this EnrichedEvent @event)
    {
        return @event is EnrichedAssetEvent { Type: EnrichedAssetEventType.Deleted };
    }

    public static async Task<(string Response, string Dump)> SendAsync(this HttpClient client,
        FlowExecutionContext executionContext,
        HttpRequestMessage request,
        string? requestBody = null,
        CancellationToken ct = default)
    {
        HttpResponseMessage? response = null;
        try
        {
            response = await client.SendAsync(request, ct);

            var responseString = await response.Content.ReadAsStringAsync(ct);
            var requestDump = DumpFormatter.BuildDump(request, response, requestBody, responseString);

            if (!response.IsSuccessStatusCode)
            {
                executionContext.Log("Http request failed", requestDump);
                throw new HttpRequestException($"Response code does not indicate success: {(int)response.StatusCode} ({response.StatusCode}).");
            }

            return (responseString, requestDump);
        }
        catch (Exception ex)
        {
            var requestDump = DumpFormatter.BuildDump(request, response, requestBody, ex.ToString());

            executionContext.Log("Http request failed", requestDump);
            throw;
        }
    }

    public static (string Id, bool IsGenerated) GetOrCreateId(this EnrichedEvent @event)
    {
        if (@event is IEnrichedEntityEvent enrichedEntityEvent)
        {
            return (enrichedEntityEvent.Id.ToString(), false);
        }
        else
        {
            return (DomainId.NewGuid().ToString(), true);
        }
    }
}
