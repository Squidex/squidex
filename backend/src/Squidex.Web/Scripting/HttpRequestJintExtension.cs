// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Jint;
using Microsoft.AspNetCore.Http;
using Squidex.Domain.Apps.Core.Scripting;

namespace Squidex.Web.Scripting;

public sealed class HttpRequestJintExtension : IJintExtension, IScriptDescriptor
{
    private delegate RequestInfo? GetRequestDelegate();
    private readonly IHttpContextAccessor httpContextAccessor;

    private sealed class RequestInfo
    {
        public string Method { get; set; }

        public string Path { get; set; }

        public string? QueryString { get; set; }

        public Dictionary<string, string?[]> Query { get; set; }
    }

    public HttpRequestJintExtension(IHttpContextAccessor httpContextAccessor)
    {
        this.httpContextAccessor = httpContextAccessor;
    }

    public void Extend(Engine engine)
    {
        var getRequest = new GetRequestDelegate(() =>
        {
            var request = httpContextAccessor.HttpContext?.Request;
            if (request == null)
            {
                return null;
            }

            var result = new RequestInfo
            {
                Method = request.Method,
                Path = request.Path,
                Query = request.Query.ToDictionary(x => x.Key, x => x.Value.ToArray()),
                QueryString = request.QueryString.Value
            };

            return result;
        });

        engine.SetValue("getRequest", getRequest);
    }

    public void Describe(AddDescription describe, ScriptScope scope)
    {
        describe(JsonType.Function, "getRequest()",
            "Gets information about the actual HTTP request.");
    }
}
