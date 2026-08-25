// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Security.Claims;
using Jint;
using Jint.Native;
using Jint.Runtime;
using Microsoft.Extensions.DependencyInjection;
using Squidex.Domain.Apps.Core.Apps;
using Squidex.Domain.Apps.Core.Scripting;
using Squidex.Domain.Apps.Core.Scripting.Internal;
using Squidex.Domain.Apps.Entities.Properties;
using Squidex.Infrastructure;

namespace Squidex.Domain.Apps.Entities.Contents;

public sealed class ReferencesJintExtension(IServiceProvider serviceProvider) : IJintExtension, IScriptDescriptor
{
    private delegate void GetReferencesDelegate(JsValue references, Action<JsValue> callback);

    public void ExtendAsync(Engine engine)
    {
        var getReference = new GetReferencesDelegate((references, callback) =>
        {
            if (!engine.TryGetVar<DomainId>("appId", out var appId) ||
                !engine.TryGetVar<ClaimsPrincipal>("user", out var user))
            {
                throw new JavaScriptException("'getReference' is not available in this script.");
            }

            GetReference(engine, appId, user, references, callback);
        });

        var getReferences = new GetReferencesDelegate((references, callback) =>
        {
            if (!engine.TryGetVar<DomainId>("appId", out var appId) ||
                !engine.TryGetVar<ClaimsPrincipal>("user", out var user))
            {
                throw new JavaScriptException("'getReferences' is not available in this script.");
            }

            GetReferences(engine, appId, user, references, callback);
        });

        engine.SetValue("getReference", getReferences);
        engine.SetValue("getReferenceV2", getReference);
        engine.SetValue("getReferences", getReferences);
    }

    private void GetReferences(Engine engine, DomainId appId, ClaimsPrincipal user,
        JsValue references, Action<JsValue> callback)
    {
        if (callback == null)
        {
            throw new JavaScriptException("Callback is not defined.");
        }

        // The javascript values are read while we are still inside the engine.
        var ids = references.ToIds();

        if (ids.Count == 0)
        {
            callback(new JsArray(engine));
            return;
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

            return await contentQuery.QueryAsync(requestContext, Q.Empty.WithIds(ids), ct);
        },
        contents => callback(JsValue.FromObject(engine, contents.ToArray())));
    }

    private void GetReference(Engine engine, DomainId appId, ClaimsPrincipal user,
        JsValue references, Action<JsValue> callback)
    {
        if (callback == null)
        {
            throw new JavaScriptException("Callback is not defined.");
        }

        // The javascript values are read while we are still inside the engine.
        var ids = references.ToIds();

        if (ids.Count == 0)
        {
            callback(JsValue.Null);
            return;
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

            return await contentQuery.QueryAsync(requestContext, Q.Empty.WithIds(ids), ct);
        },
        contents => callback(JsValue.FromObject(engine, contents.FirstOrDefault())));
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

        describe(JsonType.Function, "getReference(id, callback)",
            Resources.ScriptingGetReference,
            deprecationReason: Resources.ScriptingGetReferenceDeprecated);

        describe(JsonType.Function, "getReferenceV2(id, callback)",
            Resources.ScriptingGetReferenceV2);

        describe(JsonType.Function, "getReferences(ids, callback)",
            Resources.ScriptingGetReferences);
    }
}
