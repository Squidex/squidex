// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Jint.Native;
using Jint.Runtime;
using Squidex.Domain.Apps.Core.Scripting;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Tasks;

namespace Squidex.Domain.Apps.Entities.Contents
{
    public sealed class ReferencesJintExtension : IJintExtension
    {
        private delegate void GetReferencesDelegate(JsValue references, Action<JsValue> callback);
        private readonly IAppProvider appProvider;
        private readonly IContentQueryService contentQuery;

        public ReferencesJintExtension(IAppProvider appProvider, IContentQueryService contentQuery)
        {
            Guard.NotNull(appProvider, nameof(appProvider));
            Guard.NotNull(contentQuery, nameof(contentQuery));

            this.appProvider = appProvider;

            this.contentQuery = contentQuery;
        }

        public void ExtendAsync(ExecutionContext context)
        {
            if (!context.TryGetValue<DomainId>(nameof(ScriptVars.AppId), out var appId))
            {
                return;
            }

            if (!context.TryGetValue<ClaimsPrincipal>(nameof(ScriptVars.User), out var user))
            {
                return;
            }

            var action = new GetReferencesDelegate((references, callback) => GetReferences(context, appId, user, references, callback));

            context.Engine.SetValue("getReference", action);
            context.Engine.SetValue("getReferences", action);
        }

        private void GetReferences(ExecutionContext context, DomainId appId, ClaimsPrincipal user, JsValue references, Action<JsValue> callback)
        {
            GetReferencesAsync(context, appId, user, references, callback).Forget();
        }

        private async Task GetReferencesAsync(ExecutionContext context, DomainId appId, ClaimsPrincipal user, JsValue references, Action<JsValue> callback)
        {
            Guard.NotNull(callback, nameof(callback));

            var ids = new List<DomainId>();

            if (references.IsString())
            {
                ids.Add(DomainId.Create(references.ToString()));
            }
            else if (references.IsArray())
            {
                foreach (var value in references.AsArray())
                {
                    if (value.IsString())
                    {
                        ids.Add(DomainId.Create(value.ToString()));
                    }
                }
            }

            if (ids.Count == 0)
            {
                var emptyContents = Array.Empty<IEnrichedContentEntity>();

                callback(JsValue.FromObject(context.Engine, emptyContents));
                return;
            }

            context.MarkAsync();

            try
            {
                var app = await appProvider.GetAppAsync(appId);

                if (app == null)
                {
                    throw new JavaScriptException("App does not exist.");
                }

                var requestContext =
                    new Context(user, app).Clone(b => b
                        .WithoutContentEnrichment()
                        .WithUnpublished()
                        .WithoutTotal());

                var contents = await contentQuery.QueryAsync(requestContext, Q.Empty.WithIds(ids));

                callback(JsValue.FromObject(context.Engine, contents.ToArray()));
            }
            catch (Exception ex)
            {
                context.Fail(ex);
            }
        }
    }
}
