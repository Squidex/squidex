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
using Squidex.Infrastructure;

namespace Squidex.Domain.Apps.Core.Scripting.Extensions;

public sealed class HttpJintExtension : IJintExtension, IScriptDescriptor
{
    private delegate void HttpJsonDelegate(string url, Action<JsValue> callback, JsValue? headers = null, bool ignoreError = false);
    private delegate void HttpJsonWithBodyDelegate(string url, JsValue post, Action<JsValue> callback, JsValue? headers = null, bool ignoreError = false);
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

    private void AddMethod(ScriptExecutionContext context, HttpMethod method, string name)
    {
        var action = new HttpJsonDelegate((url, callback, headers, ignoreError) =>
        {
            Request(context, method, url, null, callback, headers, ignoreError);
        });

        context.Engine.SetValue(name, action);
    }

    private void AddBodyMethod(ScriptExecutionContext context, HttpMethod method, string name)
    {
        var action = new HttpJsonWithBodyDelegate((url, body, callback, headers, ignoreError) =>
        {
            Request(context, method, url, body, callback, headers, ignoreError);
        });

        context.Engine.SetValue(name, action);
    }

    private void Request(ScriptExecutionContext context, HttpMethod method, string url, JsValue? body, Action<JsValue> callback, JsValue? headers, bool ignoreError)
    {
        if (!Uri.TryCreate(url, UriKind.Absolute, out var uri))
        {
            throw new JavaScriptException("URL is not valid.");
        }

        if (callback == null)
        {
            throw new JavaScriptException("Callback is not defined.");
        }

        context.Schedule(async (scheduler, ct) =>
        {
            try
            {
                var httpClient = httpClientFactory.CreateClient("Jint");

                var request = CreateRequest(context, method, uri, body, headers);
                var response = await httpClient.SendAsync(request, ct);

                if (!ignoreError)
                {
                    response.EnsureSuccessStatusCode();
                }

                JsValue responseObject;

                if (ignoreError && !response.IsSuccessStatusCode)
                {
                    var responseString = await response.Content.ReadAsStringAsync(ct);

                    responseObject = JsValue.FromObject(context.Engine, new Dictionary<string, object?>
                    {
                        ["statusCode"] = (int)response.StatusCode,
                        ["headers"] =
                            response.Content.Headers
                                .Concat(response.Headers)
                                .Concat(response.TrailingHeaders)
                                .GroupBy(x => x.Key)
                                .ToDictionary(x => x.Key, x => x.Last().Value.First()),
                        ["body"] = responseString,
                    });
                }
                else
                {
                    responseObject = await ParseResponseAsync(context, response, ct);
                }

                scheduler.Run(callback, responseObject);
            }
            catch (Exception ex)
            {
                throw new JavaScriptException(ex.Message);
            }
        });
    }

    private static HttpRequestMessage CreateRequest(ScriptExecutionContext context, HttpMethod method, Uri uri, JsValue? body, JsValue? headers)
    {
        var request = new HttpRequestMessage(method, uri);

        if (body != null)
        {
            var jsonWriter = new JsonSerializer(context.Engine);
            var jsonContent = jsonWriter.Serialize(body, JsValue.Undefined, JsValue.Undefined)?.ToString();

            if (jsonContent != null)
            {
                request.Content = new StringContent(jsonContent, Encoding.UTF8, "application/json");
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

    private static async Task<JsValue> ParseResponseAsync(ScriptExecutionContext context, HttpResponseMessage response,
        CancellationToken ct)
    {
        var responseString = await response.Content.ReadAsStringAsync(ct);

        ct.ThrowIfCancellationRequested();

        var jsonParser = new JsonParser(context.Engine);
        var jsonValue = jsonParser.Parse(responseString);

        ct.ThrowIfCancellationRequested();

        return jsonValue;
    }

    public void Describe(AddDescription describe, ScriptScope scope)
    {
        if (!scope.HasFlag(ScriptScope.Async))
        {
            return;
        }

        describe(JsonType.Function, "getJSON(url, callback, headers?, ignoreError?)",
            Resources.ScriptingGetJSON);

        describe(JsonType.Function, "postJSON(url, body, callback, headers?, ignoreError?)",
            Resources.ScriptingPostJSON);

        describe(JsonType.Function, "putJSON(url, body, callback, headers?, ignoreError?)",
            Resources.ScriptingPutJson);

        describe(JsonType.Function, "patchJSON(url, body, callback, headers?, ignoreError?)",
            Resources.ScriptingPatchJson);

        describe(JsonType.Function, "deleteJSON(url, callback, headers?, ignoreError?)",
            Resources.ScriptingDeleteJson);
    }
}
