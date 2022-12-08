// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Text;
using Jint;
using Jint.Native;
using Jint.Native.Json;
using Jint.Runtime;
using Squidex.Domain.Apps.Core.Properties;

namespace Squidex.Domain.Apps.Core.Scripting.Extensions;

public sealed class HttpJintExtension : IJintExtension, IScriptDescriptor
{
    private delegate void HttpJson(string url, Action<JsValue> callback, JsValue? headers = null);
    private delegate void HttpJsonWithBody(string url, JsValue post, Action<JsValue> callback, JsValue? headers = null);
    private readonly IHttpClientFactory httpClientFactory;

    public HttpJintExtension(IHttpClientFactory httpClientFactory)
    {
        this.httpClientFactory = httpClientFactory;
    }

    public void ExtendAsync(ScriptExecutionContext context)
    {
        AddBodyMethod(context, HttpMethod.Patch, "patchJSON");
        AddBodyMethod(context, HttpMethod.Post, "postJSON");
        AddBodyMethod(context, HttpMethod.Put, "putJSON");
        AddMethod(context, HttpMethod.Delete, "deleteJSON");
        AddMethod(context, HttpMethod.Get, "getJSON");
    }

    public void Describe(AddDescription describe, ScriptScope scope)
    {
        if (!scope.HasFlag(ScriptScope.Async))
        {
            return;
        }

        describe(JsonType.Function, "getJSON(url, callback, headers?)",
            Resources.ScriptingGetJSON);

        describe(JsonType.Function, "postJSON(url, body, callback, headers?)",
            Resources.ScriptingPostJSON);

        describe(JsonType.Function, "putJSON(url, body, callback, headers?)",
            Resources.ScriptingPutJson);

        describe(JsonType.Function, "patchJSON(url, body, callback, headers?)",
            Resources.ScriptingPatchJson);

        describe(JsonType.Function, "deleteJSON(url, body, callback, headers?)",
            Resources.ScriptingDeleteJson);
    }

    private void AddMethod(ScriptExecutionContext context, HttpMethod method, string name)
    {
        var action = new HttpJson((url, callback, headers) =>
        {
            Request(context, method, url, null, callback, headers);
        });

        context.Engine.SetValue(name, action);
    }

    private void AddBodyMethod(ScriptExecutionContext context, HttpMethod method, string name)
    {
        var action = new HttpJsonWithBody((url, body, callback, headers) =>
        {
            Request(context, method, url, body, callback, headers);
        });

        context.Engine.SetValue(name, action);
    }

    private void Request(ScriptExecutionContext context, HttpMethod method, string url, JsValue? body, Action<JsValue> callback, JsValue? headers)
    {
        context.Schedule(async (scheduler, ct) =>
        {
            if (callback == null)
            {
                throw new JavaScriptException("Callback cannot be null.");
            }

            if (!Uri.TryCreate(url, UriKind.Absolute, out var uri))
            {
                throw new JavaScriptException("URL is not valid.");
            }

            using (var httpClient = httpClientFactory.CreateClient())
            {
                using (var request = CreateRequest(context, method, uri, body, headers))
                {
                    using (var response = await httpClient.SendAsync(request, ct))
                    {
                        response.EnsureSuccessStatusCode();

                        var responseObject = await ParseResponseasync(context, response, ct);

                        scheduler.Run(callback, responseObject);
                    }
                }
            }
        });
    }

    private static HttpRequestMessage CreateRequest(ScriptExecutionContext context, HttpMethod method, Uri uri, JsValue? body, JsValue? headers)
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

    private static async Task<JsValue> ParseResponseasync(ScriptExecutionContext context, HttpResponseMessage response,
        CancellationToken ct)
    {
        var responseString = await response.Content.ReadAsStringAsync(ct);

        ct.ThrowIfCancellationRequested();

        var jsonParser = new JsonParser(context.Engine);
        var jsonValue = jsonParser.Parse(responseString);

        ct.ThrowIfCancellationRequested();

        return jsonValue;
    }
}
