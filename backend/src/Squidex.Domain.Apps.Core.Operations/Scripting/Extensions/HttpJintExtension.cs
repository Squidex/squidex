// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Jint;
using Jint.Native;
using Jint.Native.Json;
using Jint.Runtime;
using Squidex.Infrastructure.Tasks;

namespace Squidex.Domain.Apps.Core.Scripting.Extensions
{
    public sealed class HttpJintExtension : IJintExtension
    {
        private delegate void HttpJson(string url, Action<JsValue> callback, JsValue? headers = null);
        private delegate void HttpJsonWithBody(string url, JsValue post, Action<JsValue> callback, JsValue? headers = null);
        private readonly IHttpClientFactory httpClientFactory;

        public HttpJintExtension(IHttpClientFactory httpClientFactory)
        {
            this.httpClientFactory = httpClientFactory;
        }

        public void ExtendAsync(ExecutionContext context)
        {
            AddMethod(context, HttpMethod.Get, "getJSON");
            AddMethod(context, HttpMethod.Delete, "deleteJSON");

            AdBodyMethod(context, HttpMethod.Patch, "patchJSON");
            AdBodyMethod(context, HttpMethod.Post, "postJSON");
            AdBodyMethod(context, HttpMethod.Put, "putJSON");
        }

        private void AddMethod(ExecutionContext context, HttpMethod method, string name)
        {
            var action = new HttpJson((url, callback, headers) =>
            {
                RequestAsync(context, method, url, null, callback, headers).Forget();
            });

            context.Engine.SetValue(name, action);
        }

        private void AdBodyMethod(ExecutionContext context, HttpMethod method, string name)
        {
            var action = new HttpJsonWithBody((url, body, callback, headers) =>
            {
                RequestAsync(context, method, url, body, callback, headers).Forget();
            });

            context.Engine.SetValue(name, action);
        }

        private async Task RequestAsync(ExecutionContext context, HttpMethod method, string url, JsValue? body, Action<JsValue> callback, JsValue? headers)
        {
            if (callback == null)
            {
                context.Fail(new JavaScriptException("Callback cannot be null."));
                return;
            }

            if (!Uri.TryCreate(url, UriKind.Absolute, out var uri))
            {
                context.Fail(new JavaScriptException("URL is not valid."));
                return;
            }

            context.MarkAsync();

            try
            {
                using (var httpClient = httpClientFactory.CreateClient())
                {
                    using (var request = CreateRequest(context, method, uri, body, headers))
                    {
                        using (var response = await httpClient.SendAsync(request, context.CancellationToken))
                        {
                            response.EnsureSuccessStatusCode();

                            var responseObject = await ParseResponse(context, response);

                            context.Engine.ResetConstraints();

                            callback(responseObject);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                context.Fail(ex);
            }
        }

        private static HttpRequestMessage CreateRequest(ExecutionContext context, HttpMethod method, Uri uri, JsValue? body, JsValue? headers)
        {
            var request = new HttpRequestMessage(method, uri);

            if (body != null)
            {
                var serializer = new JsonSerializer(context.Engine);

                var json = serializer.Serialize(body, JsValue.Undefined, JsValue.Undefined)?.ToString();

                if (json != null)
                {
                    request.Content = new StringContent(json, Encoding.UTF8, "text/json");
                }
            }

            if (headers != null && headers.Type == Types.Object)
            {
                var obj = headers.AsObject();

                foreach (var (key, property) in obj.GetOwnProperties())
                {
                    var value = TypeConverter.ToString(property.Value);

                    var keyString = key.AsString();

                    if (!string.IsNullOrWhiteSpace(keyString))
                    {
                        request.Headers.TryAddWithoutValidation(keyString, value ?? string.Empty);
                    }
                }
            }

            return request;
        }

        private static async Task<JsValue> ParseResponse(ExecutionContext context, HttpResponseMessage response)
        {
            var responseString = await response.Content.ReadAsStringAsync(context.CancellationToken);

            context.CancellationToken.ThrowIfCancellationRequested();

            var jsonParser = new JsonParser(context.Engine);
            var jsonValue = jsonParser.Parse(responseString);

            context.CancellationToken.ThrowIfCancellationRequested();

            return jsonValue;
        }
    }
}
