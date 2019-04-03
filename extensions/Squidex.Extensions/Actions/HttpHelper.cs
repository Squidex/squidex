// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Squidex.Domain.Apps.Core.HandleRules;
using Squidex.Infrastructure.Http;

namespace Squidex.Extensions.Actions
{
    public static class HttpHelper
    {
        public static async Task<Result> OneWayRequestAsync(this HttpClient client, HttpRequestMessage request, string requestBody = null, CancellationToken ct = default)
        {
            HttpResponseMessage response = null;
            try
            {
                response = await client.SendAsync(request, ct);

                var responseString = await response.Content.ReadAsStringAsync();
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
}
