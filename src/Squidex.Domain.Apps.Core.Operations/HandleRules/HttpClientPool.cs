// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Net.Http;

namespace Squidex.Domain.Apps.Core.HandleRules
{
    public static class HttpClientPool
    {
        private static readonly ClientPool<string, HttpClient> Pool = new ClientPool<string, HttpClient>(key =>
        {
            return new HttpClient { Timeout = TimeSpan.FromSeconds(2) };
        });

        public static HttpClient GetHttpClient()
        {
            return Pool.GetClient(string.Empty);
        }
    }
}
