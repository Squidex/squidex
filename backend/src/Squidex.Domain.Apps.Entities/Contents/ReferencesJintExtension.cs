// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Security.Claims;
using Jint.Native;
using Jint.Runtime;
using Microsoft.Extensions.DependencyInjection;
using Squidex.Domain.Apps.Core.Apps;
using Squidex.Domain.Apps.Core.Scripting;
using Squidex.Domain.Apps.Core.Scripting.Internal;
using Squidex.Domain.Apps.Entities.Properties;
using Squidex.Infrastructure;

namespace Squidex.Domain.Apps.Entities.Contents;

public sealed class ReferencesJintExtension : IJintExtension, IScriptDescriptor
{
    private delegate void GetReferencesDelegate(JsValue references, Action<JsValue> callback);
    private readonly IServiceProvider serviceProvider;

    public ReferencesJintExtension(IServiceProvider serviceProvider)
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

        var getReference = new GetReferencesDelegate((references, callback) =>
        {
            GetReference(context, appId, user, references, callback);
        });

        var getReferences = new GetReferencesDelegate((references, callback) =>
        {
            GetReferences(context, appId, user, references, callback);
        });

        context.Engine.SetValue("getReference", getReferences);
        context.Engine.SetValue("getReferenceV2", getReference);
        context.Engine.SetValue("getReferences", getReferences);
    }

    private void GetReferences(ScriptExecutionContext context, DomainId appId, ClaimsPrincipal user, JsValue references, Action<JsValue> callback)
    {
        if (callback == null)
        {
            throw new JavaScriptException("Callback is not defined.");
        }

        context.Schedule(async (scheduler, ct) =>
        {
            var ids = references.ToIds();

            if (ids.Count == 0)
            {
                scheduler.Run(callback, new JsArray(context.Engine));
                return;
            }

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

            var contents = await contentQuery.QueryAsync(requestContext, Q.Empty.WithIds(ids), ct);

            scheduler.Run(callback, JsValue.FromObject(context.Engine, contents.ToArray()));
        });
    }

    private void GetReference(ScriptExecutionContext context, DomainId appId, ClaimsPrincipal user, JsValue references, Action<JsValue> callback)
    {
        if (callback == null)
        {
            throw new JavaScriptException("Callback is not defined.");
        }

        context.Schedule(async (scheduler, ct) =>
        {
            var ids = references.ToIds();

            if (ids.Count == 0)
            {
                scheduler.Run(callback, JsValue.Null);
                return;
            }

            var app = await GetAppAsync(appId);

            if (app == null)
            {
                scheduler.Run(callback, JsValue.Null);
                return;
            }

            var contentQuery = serviceProvider.GetRequiredService<IContentQueryService>();

            var requestContext =
                new Context(user, app).Clone(b => b
                    .WithFields(null)
                    .WithNoEnrichment()
                    .WithUnpublished()
                    .WithNoTotal());

            var contents = await contentQuery.QueryAsync(requestContext, Q.Empty.WithIds(ids), ct);

            scheduler.Run(callback, JsValue.FromObject(context.Engine, contents.FirstOrDefault()));
        });
    }

    private async Task<App> GetAppAsync(DomainId appId)
    {
        var appProvider = serviceProvider.GetRequiredService<IAppProvider>();

        var app = await appProvider.GetAppAsync(appId);

        if (app == null)
        {
            throw new JavaScriptException("App does not exist.");
        }

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
