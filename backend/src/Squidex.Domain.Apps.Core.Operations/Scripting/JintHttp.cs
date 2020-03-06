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
using Jint;
using Jint.Native;
using Jint.Native.Json;
using Jint.Runtime;
using Squidex.Infrastructure.Tasks;

namespace Squidex.Domain.Apps.Core.Scripting
{
    internal sealed class JintHttp
    {
        private delegate void GetJsonDelegate(string url, Action<JsValue> callback, JsValue? headers = null);
        private readonly IHttpClientFactory httpClientFactory;
        private readonly Action<Exception> exceptionHandler;
        private readonly CancellationToken cancellationToken;
        private JsonParser parser;

        public JintHttp(IHttpClientFactory httpClientFactory, CancellationToken cancellationToken, Action<Exception> exceptionHandler)
        {
            this.httpClientFactory = httpClientFactory;
            this.exceptionHandler = exceptionHandler;
            this.cancellationToken = cancellationToken;
        }

        public Engine Add(Engine engine)
        {
            parser = new JsonParser(engine);

            engine.SetValue("getJSON", new GetJsonDelegate(GetJson));

            return engine;
        }

        private void GetJson(string url, Action<JsValue> callback, JsValue? headers)
        {
            GetJSONAsync(url, callback, headers).Forget();
        }

        private async Task GetJSONAsync(string url, Action<JsValue> callback, JsValue? headers)
        {
            try
            {
                using (var httpClient = httpClientFactory.CreateClient())
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

                    var response = await httpClient.SendAsync(request, cancellationToken);

                    response.EnsureSuccessStatusCode();

                    cancellationToken.ThrowIfCancellationRequested();

                    var responseString = await response.Content.ReadAsStringAsync();

                    cancellationToken.ThrowIfCancellationRequested();

                    var responseJson = parser.Parse(responseString);

                    callback(responseJson);
                }
            }
            catch (Exception ex)
            {
                exceptionHandler(ex);
            }
        }
    }
}
