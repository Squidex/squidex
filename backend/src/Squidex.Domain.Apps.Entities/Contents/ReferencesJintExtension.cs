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
using Microsoft.Extensions.DependencyInjection;
using Squidex.Domain.Apps.Core.Scripting;
using Squidex.Domain.Apps.Entities.Apps;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Tasks;

namespace Squidex.Domain.Apps.Entities.Contents
{
    public sealed class ReferencesJintExtension : IJintExtension
    {
        private delegate void GetReferencesDelegate(JsValue references, Action<JsValue> callback);
        private readonly IServiceProvider serviceProvider;

        public ReferencesJintExtension(IServiceProvider serviceProvider)
        {
            this.serviceProvider = serviceProvider;
        }

        public void ExtendAsync(ExecutionContext context)
        {
            if (!context.TryGetValue<DomainId>("appId", out var appId))
            {
                return;
            }

            if (!context.TryGetValue<ClaimsPrincipal>("user", out var user))
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
                var app = await GetAppAsync(appId);

                var requestContext =
                    new Context(user, app).Clone(b => b
                        .WithoutContentEnrichment()
                        .WithUnpublished()
                        .WithoutTotal());

                var contentQuery = serviceProvider.GetRequiredService<IContentQueryService>();

                var contents = await contentQuery.QueryAsync(requestContext, Q.Empty.WithIds(ids), context.CancellationToken);

                callback(JsValue.FromObject(context.Engine, contents.ToArray()));
            }
            catch (Exception ex)
            {
                context.Fail(ex);
            }
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
}
