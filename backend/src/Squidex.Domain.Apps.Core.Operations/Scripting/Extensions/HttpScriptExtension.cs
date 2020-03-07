﻿// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Net.Http;
using System.Threading.Tasks;
using Jint.Native;
using Jint.Native.Json;
using Jint.Runtime;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Tasks;

namespace Squidex.Domain.Apps.Core.Scripting.Extensions
{
    public sealed class HttpScriptExtension : IScriptExtension
    {
        private delegate void GetJsonDelegate(string url, Action<JsValue> callback, JsValue? headers = null);
        private readonly IHttpClientFactory httpClientFactory;

        public HttpScriptExtension(IHttpClientFactory httpClientFactory)
        {
            Guard.NotNull(httpClientFactory);

            this.httpClientFactory = httpClientFactory;
        }

        public void Extend(ExecutionContext context, bool async)
        {
            if (async)
            {
                var engine = context.Engine;

                engine.SetValue("getJSON", new GetJsonDelegate((url, callback, headers) => GetJson(context, url, callback, headers)));
            }
        }

        private void GetJson(ExecutionContext context, string url, Action<JsValue> callback, JsValue? headers)
        {
            GetJsonAsync(context, url, callback, headers).Forget();
        }

        private async Task GetJsonAsync(ExecutionContext context, string url, Action<JsValue> callback, JsValue? headers)
        {
            try
            {
                using (var httpClient = httpClientFactory.CreateClient())
                {
                    var request = CreateRequest(url, headers);
                    var response = await httpClient.SendAsync(request, context.CancellationToken);

                    response.EnsureSuccessStatusCode();

                    var responseObject = await ParseResponse(context, response);

                    context.Engine.ResetTimeoutTicks();

                    callback(responseObject);
                }
            }
            catch (Exception ex)
            {
                context.Fail(ex);
            }
        }

        private static HttpRequestMessage CreateRequest(string url, JsValue? headers)
        {
            if (!Uri.TryCreate(url, UriKind.Absolute, out var uri))
            {
                throw new ArgumentException("Url must be an absolute URL");
            }

            var request = new HttpRequestMessage(HttpMethod.Get, uri);

            if (headers != null && headers.Type == Types.Object)
            {
                var obj = headers.AsObject();

                foreach (var (key, property) in obj.GetOwnProperties())
                {
                    var value = TypeConverter.ToString(property.Value);

                    if (!string.IsNullOrWhiteSpace(key))
                    {
                        request.Headers.TryAddWithoutValidation(key, value ?? string.Empty);
                    }
                }
            }

            return request;
        }

        private async Task<JsValue> ParseResponse(ExecutionContext context, HttpResponseMessage response)
        {
            var responseString = await response.Content.ReadAsStringAsync();

            context.CancellationToken.ThrowIfCancellationRequested();

            var jsonParser = new JsonParser(context.Engine);
            var jsonValue = jsonParser.Parse(responseString);

            context.CancellationToken.ThrowIfCancellationRequested();

            return jsonValue;
        }
    }
}
