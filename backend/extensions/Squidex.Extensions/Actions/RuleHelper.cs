// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Domain.Apps.Core.HandleRules;
using Squidex.Domain.Apps.Core.Rules.EnrichedEvents;
using Squidex.Domain.Apps.Core.Scripting;
using Squidex.Infrastructure.Http;

namespace Squidex.Extensions.Actions;

public static class RuleHelper
{
    public static bool ShouldDelete(this EnrichedEvent @event, IScriptEngine scriptEngine, string expression)
    {
        if (!string.IsNullOrWhiteSpace(expression))
        {
            // Script vars are just wrappers over dictionaries for better performance.
            var vars = new EventScriptVars
            {
                Event = @event
            };

            return scriptEngine.Evaluate(vars, expression);
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

    public static async Task<Result> OneWayRequestAsync(this HttpClient client, HttpRequestMessage request, string requestBody = null,
        CancellationToken ct = default)
    {
        HttpResponseMessage response = null;
        try
        {
            response = await client.SendAsync(request, ct);

            var responseString = await response.Content.ReadAsStringAsync(ct);
            var requestDump = DumpFormatter.BuildDump(request, response, requestBody, responseString);

            if (!response.IsSuccessStatusCode)
            {
                var ex = new HttpRequestException($"Response code does not indicate success: {(int)response.StatusCode} ({response.StatusCode}).");

                return Result.Failed(ex, requestDump);
            }
            else
            {
                return Result.Success(requestDump);
            }
        }
        catch (Exception ex)
        {
            var requestDump = DumpFormatter.BuildDump(request, response, requestBody, ex.ToString());

            return Result.Failed(ex, requestDump);
        }
    }
}
