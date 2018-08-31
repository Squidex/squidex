// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Net.Http;
using System.Threading.Tasks;
using Squidex.Infrastructure.Http;

namespace Squidex.Domain.Apps.Core.HandleRules.Actions.Utils
{
    public static class HttpHelper
    {
        public static async Task<(string Dump, Exception Exception)> OneWayRequestAsync(this HttpClient client, HttpRequestMessage request, string requestBody = null)
        {
            HttpResponseMessage response = null;
            try
            {
                response = await client.SendAsync(request);

                var responseString = await response.Content.ReadAsStringAsync();

                var requestDump = DumpFormatter.BuildDump(request, response, requestBody, responseString);

                Exception ex = null;

                if (!response.IsSuccessStatusCode)
                {
                    ex = new HttpRequestException($"Response code does not indicate success: {(int)response.StatusCode} ({response.StatusCode}).");
                }

                return (requestDump, ex);
            }
            catch (Exception ex)
            {
                var requestDump = DumpFormatter.BuildDump(request, response, requestBody, ex.ToString());

                return (requestDump, ex);
            }
        }
    }
}
