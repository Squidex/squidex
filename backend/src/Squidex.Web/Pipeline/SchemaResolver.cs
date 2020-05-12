// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Squidex.Domain.Apps.Entities;
using Squidex.Domain.Apps.Entities.Schemas;
using Squidex.Infrastructure;

namespace Squidex.Web.Pipeline
{
    public sealed class SchemaResolver : IAsyncActionFilter
    {
        private readonly IAppProvider appProvider;

        public SchemaResolver(IAppProvider appProvider)
        {
            Guard.NotNull(appProvider, nameof(appProvider));

            this.appProvider = appProvider;
        }

        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            var appId = context.HttpContext.Features.Get<IAppFeature>()?.AppId.Id ?? default;

            if (appId != default)
            {
                var schemaIdOrName = context.RouteData.Values["name"]?.ToString();

                if (!string.IsNullOrWhiteSpace(schemaIdOrName))
                {
                    var schema = await GetSchemaAsync(appId, schemaIdOrName);

                    if (schema == null)
                    {
                        context.Result = new NotFoundResult();
                        return;
                    }

                    context.HttpContext.Features.Set<ISchemaFeature>(new SchemaFeature(schema.NamedId()));
                }
            }

            await next();
        }

        private Task<ISchemaEntity?> GetSchemaAsync(Guid appId, string schemaIdOrName)
        {
            if (Guid.TryParse(schemaIdOrName, out var id))
            {
                return appProvider.GetSchemaAsync(appId, id);
            }
            else
            {
                return appProvider.GetSchemaAsync(appId, schemaIdOrName);
            }
        }
    }
}
