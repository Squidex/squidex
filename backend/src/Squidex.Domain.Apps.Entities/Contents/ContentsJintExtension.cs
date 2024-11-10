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
using Squidex.Domain.Apps.Core.Scripting.Internal;
using Squidex.Domain.Apps.Entities.Properties;
using Squidex.Infrastructure;

namespace Squidex.Domain.Apps.Entities.Contents;

public sealed class ContentsJintExtension : IJintExtension, IScriptDescriptor
{
    private delegate void GetContentsDelegate(string schema, JsValue query, Action<JsValue> callback);
    private readonly IServiceProvider serviceProvider;

    public ContentsJintExtension(IServiceProvider serviceProvider)
    {
        this.serviceProvider = serviceProvider;
    }

    public void ExtendAsync(ScriptExecutionContext context)
    {
        if (!context.TryGetValueIfExists<DomainId>("appId", out var appId))
        {
            return;
        }

        if (!context.TryGetValueIfExists<ClaimsPrincipal>("user", out var user))
        {
            return;
        }

        var getContents = new GetContentsDelegate((schemas, query, callback) =>
        {
            GetContents(context, appId, user, schemas, query, callback);
        });

        context.Engine.SetValue("getContents", getContents);
    }

    private void GetContents(ScriptExecutionContext context, DomainId appId, ClaimsPrincipal user,
        string schema, JsValue query, Action<JsValue> callback)
    {
        if (callback == null)
        {
            throw new JavaScriptException("Callback is not defined.");
        }

        context.Schedule(async (scheduler, ct) =>
        {
            var app = await GetAppAsync(appId);

            if (app == null)
            {
                scheduler.Run(callback, new JsArray(context.Engine));
                return;
            }

            var contentQuery = serviceProvider.GetRequiredService<IContentQueryService>();

            var requestContext =
                new Context(user, app).Clone(b => b
                    .WithFields(null)
                    .WithNoEnrichment()
                    .WithUnpublished()
                    .WithNoTotal());

            var q = Q.Empty;
            if (query is ObjectInstance obj)
            {
                if (obj.TryGetValue("query", out var t) && t is JsString oDataQuery)
                {
                    q = q.WithODataQuery(oDataQuery.AsString());
                }
            }
            else if (query is JsString oDataQuery)
            {
                q = q.WithODataQuery(oDataQuery.AsString());
            }

            var contents = await contentQuery.QueryAsync(requestContext, schema, q, ct);

            scheduler.Run(callback, JsValue.FromObject(context.Engine, contents.ToArray()));
        });
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
