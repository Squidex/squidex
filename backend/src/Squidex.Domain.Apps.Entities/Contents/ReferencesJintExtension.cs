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
using Squidex.Domain.Apps.Core.Scripting;
using Squidex.Domain.Apps.Entities.Apps;
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
        if (!context.TryGetValue<DomainId>("appId", out var appId))
        {
            return;
        }

        if (!context.TryGetValue<ClaimsPrincipal>("user", out var user))
        {
            return;
        }

        var action = new GetReferencesDelegate((references, callback) =>
        {
            GetReferences(context, appId, user, references, callback);
        });

        context.Engine.SetValue("getReference", action);
        context.Engine.SetValue("getReferences", action);
    }

    public void Describe(AddDescription describe, ScriptScope scope)
    {
        if (!scope.HasFlag(ScriptScope.Async))
        {
            return;
        }

        describe(JsonType.Function, "getReferences(ids, callback)",
            Resources.ScriptingGetReferences);

        describe(JsonType.Function, "getReference(ids, callback)",
            Resources.ScriptingGetReference);
    }

    private void GetReferences(ScriptExecutionContext context, DomainId appId, ClaimsPrincipal user, JsValue references, Action<JsValue> callback)
    {
        Guard.NotNull(callback);

        context.Schedule(async (scheduler, ct) =>
        {
            var ids = references.ToIds();

            if (ids.Count == 0)
            {
                var emptyContents = Array.Empty<IEnrichedContentEntity>();

                scheduler.Run(callback, JsValue.FromObject(context.Engine, emptyContents));
                return;
            }

            var app = await GetAppAsync(appId);

            if (app == null)
            {
                var emptyContents = Array.Empty<IEnrichedContentEntity>();

                scheduler.Run(callback, JsValue.FromObject(context.Engine, emptyContents));
                return;
            }

            var contentQuery = serviceProvider.GetRequiredService<IContentQueryService>();

            var requestContext =
                new Context(user, app).Clone(b => b
                    .WithoutContentEnrichment()
                    .WithUnpublished()
                    .WithoutTotal());

            var contents = await contentQuery.QueryAsync(requestContext, Q.Empty.WithIds(ids), ct);

            scheduler.Run(callback, JsValue.FromObject(context.Engine, contents.ToArray()));
        });
    }

    private async Task<IAppEntity> GetAppAsync(DomainId appId)
    {
        var appProvider = serviceProvider.GetRequiredService<IAppProvider>();

        var app = await appProvider.GetAppAsync(appId);

        if (app == null)
        {
            throw new JavaScriptException("App does not exist.");
        }

        return app;
    }
}
