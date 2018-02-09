// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Squidex.Domain.Apps.Entities;
using Squidex.Domain.Apps.Entities.Schemas;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Commands;

namespace Squidex.Pipeline.CommandMiddlewares
{
    public sealed class EnrichWithSchemaIdCommandMiddleware : ICommandMiddleware
    {
        private readonly IAppProvider appProvider;
        private readonly IActionContextAccessor actionContextAccessor;

        public EnrichWithSchemaIdCommandMiddleware(IAppProvider appProvider, IActionContextAccessor actionContextAccessor)
        {
            this.appProvider = appProvider;

            this.actionContextAccessor = actionContextAccessor;
        }

        public async Task HandleAsync(CommandContext context, Func<Task> next)
        {
            if (context.Command is ISchemaCommand schemaCommand && schemaCommand.SchemaId == null)
            {
                NamedId<Guid> appId = null;

                if (context.Command is IAppCommand appCommand)
                {
                    appId = appCommand.AppId;
                }

                if (appId == null)
                {
                    var appFeature = actionContextAccessor.ActionContext.HttpContext.Features.Get<IAppFeature>();

                    if (appFeature != null && appFeature.App != null)
                    {
                        appId = new NamedId<Guid>(appFeature.App.Id, appFeature.App.Name);
                    }
                }

                if (appId == null)
                {
                    return;
                }

                var routeValues = actionContextAccessor.ActionContext.RouteData.Values;

                if (routeValues.ContainsKey("name"))
                {
                    var schemaName = routeValues["name"].ToString();

                    ISchemaEntity schema;

                    if (Guid.TryParse(schemaName, out var id))
                    {
                        schema = await appProvider.GetSchemaAsync(appId.Id, id);
                    }
                    else
                    {
                        schema = await appProvider.GetSchemaAsync(appId.Id, schemaName);
                    }

                    if (schema == null)
                    {
                        throw new DomainObjectNotFoundException(schemaName, typeof(SchemaDomainObject));
                    }

                    schemaCommand.SchemaId = new NamedId<Guid>(schema.Id, schema.Name);
                }
            }

            await next();
        }
    }
}
