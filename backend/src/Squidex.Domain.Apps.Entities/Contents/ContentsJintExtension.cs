// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Security.Claims;
using Jint;
using Jint.Native;
using Jint.Native.Object;
using Jint.Runtime;
using Microsoft.Extensions.DependencyInjection;
using Squidex.Domain.Apps.Core.Apps;
using Squidex.Domain.Apps.Core.Scripting;
using Squidex.Domain.Apps.Entities.Properties;
using Squidex.Infrastructure;

namespace Squidex.Domain.Apps.Entities.Contents;

public sealed class ContentsJintExtension(IServiceProvider serviceProvider) : IJintExtension, IScriptDescriptor
{
    private delegate void GetContentsDelegate(string schema, JsValue query, Action<JsValue> callback);

    public void ExtendAsync(Engine engine)
    {
        var getContents = new GetContentsDelegate((schemas, query, callback) =>
        {
            if (!engine.TryGetVar<DomainId>("appId", out var appId) ||
                !engine.TryGetVar<ClaimsPrincipal>("user", out var user))
            {
                throw new JavaScriptException("'getContents' is not available in this script.");
            }

            GetContents(engine, appId, user, schemas, query, callback);
        });

        engine.SetValue("getContents", getContents);
    }

    private void GetContents(Engine engine, DomainId appId, ClaimsPrincipal user,
        string schema, JsValue query, Action<JsValue> callback)
    {
        if (callback == null)
        {
            throw new JavaScriptException("Callback is not defined.");
        }

        // The query is read while we are still inside the engine.
        var q = Q.Empty;
        if (query is ObjectInstance obj)
        {
            if (obj.TryGetValue("query", out var t) && t is JsString oDataQuery)
            {
                q = q.WithODataQuery(oDataQuery.AsString());
            }
        }
        else if (query is JsString oDataQueryValue)
        {
            q = q.WithODataQuery(oDataQueryValue.AsString());
        }

        engine.Schedule(async ct =>
        {
            var app = await GetAppAsync(appId);

            var contentQuery = serviceProvider.GetRequiredService<IContentQueryService>();

            var requestContext =
                new Context(user, app).Clone(b => b
                    .WithFields(null)
                    .WithNoEnrichment()
                    .WithUnpublished()
                    .WithNoTotal());

            return await contentQuery.QueryAsync(requestContext, schema, q, ct);
        },
        contents => callback(JsValue.FromObject(engine, contents.ToArray())));
    }

    private async Task<App> GetAppAsync(DomainId appId)
    {
        var appProvider = serviceProvider.GetRequiredService<IAppProvider>();

        var app = await appProvider.GetAppAsync(appId) ??
            throw new JavaScriptException("App does not exist.");

        return app;
    }

    public void Describe(AddDescription describe, ScriptScope scope)
    {
        if (!scope.HasFlag(ScriptScope.Async))
        {
            return;
        }

        describe(JsonType.Function, "getContents(schema, query, callback)",
            Resources.ScriptingGetContents);
    }
}
